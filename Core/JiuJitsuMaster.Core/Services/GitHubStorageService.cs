using Ardalis.Result;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Drawing.Imaging;

namespace JiuJitsuMaster.Core.Services
{
    public class GitHubStorageService : IGitHubStorageService
    {
        private readonly IGitHubStorageRepository _gitHubStorageRepository;

        public GitHubStorageService(IGitHubStorageRepository gitHubStorageRepository)
        {
            _gitHubStorageRepository = gitHubStorageRepository;
        }

        public async Task<Result<string>> AddImageAsync(IFormFile imageFile, int? x, int? y, int? width, int? height,
       CancellationToken cancellationToken)
        {
            try
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                var fileByteInMb = (double)imageFile.Length / (1024 * 1024);

                if (fileByteInMb > (long)EnumFileLength.Photo)
                {
                    return Result.Error("Arquivo de imagem muito grande");
                }

                if (!(extension.Equals(".jpg") || extension.Equals(".jpeg") || extension.Equals(".png")))
                {
                    return Result.Error("Tipo de arquivo não reconhecido");
                }

                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms, cancellationToken);
                var imageBytes = ms.ToArray();

                using var originalImage = new Bitmap(new MemoryStream(imageBytes));

                if (x.HasValue && y.HasValue && width.HasValue && height.HasValue)
                {
                    if (x.Value + width.Value > originalImage.Width || y.Value + height.Value > originalImage.Height)
                    {
                        return Result.Error("Dimensões de corte fora dos limites");
                    }
                }

                var jpegEncoder = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 40L);

                byte[] finalBytes;

                if (x.HasValue && y.HasValue && width.HasValue && height.HasValue)
                {
                    var rect = new Rectangle(x.Value, y.Value, width.Value, height.Value);
                    using var croppedImage = originalImage.Clone(rect, originalImage.PixelFormat);

                    using var croppedStream = new MemoryStream();
                    croppedImage.Save(croppedStream, jpegEncoder, encoderParameters);
                    finalBytes = croppedStream.ToArray();
                }
                else
                {
                    using var compressedStream = new MemoryStream();
                    originalImage.Save(compressedStream, jpegEncoder, encoderParameters);
                    finalBytes = compressedStream.ToArray();
                }

                using var finalStream = new MemoryStream(finalBytes);
                var finalFormFile = new FormFile(finalStream, 0, finalBytes.Length, "name", "fileName.jpg");

                return await _gitHubStorageRepository.AddImageAsync(finalFormFile, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<string>.Error(ex.Message);
            }
        }

        public async Task<Result> RemoveImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return Result.Error("A URL da imagem não pode ser vazia.");

            Uri uri;
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out uri))
                return Result.Error("A URL da imagem é inválida.");

            var fileName = Path.GetFileName(uri.AbsolutePath);

            return await _gitHubStorageRepository.DeleteFileAsync(fileName);
        }
    }
}