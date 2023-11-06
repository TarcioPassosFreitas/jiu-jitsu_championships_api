using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.Interfaces.Services;

namespace JiuJitsuMaster.Core.Services;

public class CaptchaService : ICaptchaService
{
    private readonly ICaptchaRepository _captchaRepository;

    public CaptchaService(ICaptchaRepository captchaRepository)
    {
        _captchaRepository = captchaRepository;
    }

    public async Task<bool> VerifyCaptcha(string captchaToken)
    {
        return await _captchaRepository.VerifyCaptcha(captchaToken);
    }
}
