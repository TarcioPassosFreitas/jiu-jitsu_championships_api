using Ardalis.Result;
using CommonUtility.Extensions;
using CommonUtility.Service;
using FluentValidation;
using iTextSharp.text;
using iTextSharp.text.pdf;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.Interfaces.Services;
using JiuJitsuMaster.Core.Validators.AthleteValidator;
using JiuJitsuMaster.Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text;

namespace JiuJitsuMaster.Core.Services;

public partial class AthleteService : BaseService<Athlete>, IAthleteService
{
    private readonly IAthleteRepository _athleteRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChampionshipRepository _championshipRepository;
    private readonly ITokenService _tokenService;
    private readonly ICaptchaService _captchaService;
    private readonly IMailKitService _mailKitService;
    private readonly IUserCreatePasswordRepository _userCreatePasswordRepository;
    private readonly IValidator<Athlete> _athleteValidator;

    public AthleteService(
        IServiceProvider serviceProvider,
        IAthleteRepository athleteRepository,
        IUserRepository userRepository,
        IChampionshipRepository championshipRepository,
        ITokenService tokenService,
        ICaptchaService captchaService,
        IMailKitService mailKitService,
        IUserCreatePasswordRepository userCreatePasswordRepository
        )
    {
        _athleteRepository = athleteRepository;
        _userRepository = userRepository;
        _championshipRepository = championshipRepository;
        _tokenService = tokenService;
        _captchaService = captchaService;
        _mailKitService = mailKitService;
        _userCreatePasswordRepository = userCreatePasswordRepository;
        _athleteValidator = serviceProvider.GetService<AddAthleteValidator>()!;
    }

    public async Task<Result<Athlete>> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var athlete = await _athleteRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);

        if (athlete == null || athlete.Deleted)
            return NotFound("Atleta não encontrado");

        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _athleteRepository.UpdateRefreshTokenAsync(athlete.Id, newRefreshToken, cancellationToken);

        athlete.RefreshToken = newRefreshToken;

        return Result<Athlete>.Success(athlete);
    }

    public async Task<Result<Athlete>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var athlete = await _athleteRepository.GetByEmailAsync(email, cancellationToken);

        if (athlete == null || athlete.Deleted)
            return NotFound("Usuário não encontrado");

        var passwordHasher = new PasswordHasher<Athlete>();
        var passwordCheckResult = passwordHasher.VerifyHashedPassword(athlete, athlete.Password, password);

        if (passwordCheckResult == PasswordVerificationResult.Failed)
            return NotFound("Credenciais inválidas");

        var refreshToken = _tokenService.GenerateRefreshToken();

        await _athleteRepository.UpdateRefreshTokenAsync(athlete.Id, refreshToken, cancellationToken);

        athlete.RefreshToken = refreshToken;

        return Result<Athlete>.Success(athlete);
    }

    public async Task<Result<Athlete>> RegisterUsersAsync(long championshipId, Athlete athlete, string captchaToken, CancellationToken cancellationToken)
    {
        await using var transaction = _athleteRepository.BeginTransaction();

        try
        {
            if (!await _captchaService.VerifyCaptcha(captchaToken))
                return InvalidFormat("Falha na verificação do Captcha.");

            var validateChampionship = await _championshipRepository.GetByIdAsync(championshipId, cancellationToken);

            if (validateChampionship == null || !(validateChampionship.Phase == ChampionshipPhase.RegistrationOpen))
                return NotFound("Não está na fase de cadastro de atletar para esse campeonato");

            var validate = await _athleteValidator.ValidateAsync(athlete);

            if (!validate.IsValid) return Result<Athlete>.Invalid(validate.Errors.ToResultValidation());

            var passwordHasher = new PasswordHasher<Athlete>();
            athlete.Password = passwordHasher.HashPassword(athlete, athlete.Password);
            var refreshToken = _tokenService.GenerateRefreshToken();
            athlete.RefreshToken = refreshToken;

            var user = await _userRepository.GetByEmailAsync(athlete.Email, cancellationToken);

            if (user != null)
                athlete.User = user;

            var result = await _athleteRepository.AddAsync(athlete, cancellationToken);

            if (user != null)
            {
                user.Athlete = result;

                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            var adminEmail = "t.passos.2017.2@gmail.com";
            var emailSent = await _mailKitService.SendMessageAsync(
                adminEmail,
                "Chegou um nova mensagem",
                "Novo Atleta Registrado",
                $"Um novo atleta se registrou: {athlete.Name}, no campeonato: {validateChampionship.Title}"
            );

            if (!emailSent)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result.Error("Não foi possível enviar o e-mail de confirmação.");
            }

            await transaction.CommitAsync(cancellationToken);

            return Result.Success(result);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Error();
        }
    }

    public async Task<Result<Athlete>> ResetPasswordAsync(long id, string newPassword, string confirmPassword, CancellationToken cancellationToken)
    {
        var sthleteById = await _athleteRepository.GetByIdAsync(id, cancellationToken);

        if (sthleteById == null || sthleteById.Deleted)
            return NotFound("Atleta não encontrado");

        return await UpdatePassword(sthleteById, newPassword, confirmPassword, cancellationToken);
    }

    private async Task<Result<Athlete>> UpdatePassword(Athlete athlete, string newPassword, string confirmPassword,
        CancellationToken cancellationToken)
    {
        var passwordHasher = new PasswordHasher<Athlete>();

        if (newPassword != confirmPassword)
            return WrongPassword("As senhas não correspondem");

        if (!newPassword.IsValidPassword())
            return InvalidFormat("A senha não corresponde às regras exigidas");

        athlete.Password = passwordHasher.HashPassword(athlete, newPassword);

        await _athleteRepository.ChangePasswordAsync(athlete.Id, athlete.Password, cancellationToken);

        return Result.Success(athlete);
    }

    public async Task<Result<PaginatedList<Athlete>>> GetAthleteRegistrationsAsync(int page, int pageSize, string? athleteName, Gender? gender, string? championshipTitle, string? city, string? state, CancellationToken cancellationToken)
    {
        var athletes = await _athleteRepository.GetAthleteRegistrationsAsync(
                page,
                pageSize,
                athleteName,
                gender,
                championshipTitle,
                city,
                state,
                cancellationToken);

        if (athletes.Count == 0)
            return Result.Success(athletes);

        return page > athletes.TotalPages ? InvalidFormat("Número de página inválido") : Result.Success(athletes);
    }

    public async Task<Result<FileData>> ExportAthleteRegistrationsToCsvAsync(string? athleteName, Gender? gender, string? championshipTitle, string? city, string? state, CancellationToken cancellationToken)
    {
        var athletes = await _athleteRepository.GetAthleteRegistrationsAsync(
                 athleteName,
                 gender,
                 championshipTitle,
                 city,
                 state,
                 cancellationToken);

        if (!athletes.Any())
        {
            return Result.NotFound("Não foram encontrados registros de atletas com os filtros fornecidos.");
        }

        var csvContent = ExportToCsv(athletes);

        var fileName = $"AthleteRegistrations-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var contentType = "text/csv";
        var fileData = new FileData(Encoding.UTF8.GetBytes(csvContent), fileName, contentType);

        return Result.Success(fileData);
    }

    public async Task<Result<FileData>> ExportAthleteRegistrationsToPdfAsync(string? athleteName, Gender? gender, string? championshipTitle, string? city, string? state, CancellationToken cancellationToken)
    {
        var athletes = await _athleteRepository.GetAthleteRegistrationsAsync(
            athleteName,
            gender,
            championshipTitle,
            city,
            state,
            cancellationToken
        );

        if (!athletes.Any())
            return Result.NotFound("Não foram encontrados registros de atletas com os filtros fornecidos.");

        var pdfContent = ExportToPdf(athletes);

        var fileName = $"AthleteRegistrations-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var contentType = "application/pdf";
        var fileData = new FileData(pdfContent, fileName, contentType);

        return Result.Success(fileData);
    }

    public async Task<Result<Athlete>> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var athlete = await _athleteRepository.GetByEmailAsync(email, cancellationToken);

        if (athlete == null || athlete.Deleted)
            return NotFound("Atleta não encontrado");

        return Result.Success(athlete);
    }




    //Métodos privados

    public string ExportToCsv(List<Athlete> athletes)
    {
        var csvBuilder = new StringBuilder();

        csvBuilder.AppendLine("Code,Name,BirthDate,CPF,Gender,Email,Password,RefreshToken,Team,Belt,Weight,UserId");

        foreach (var athlete in athletes)
        {
            csvBuilder.AppendLine($"{athlete.Code},{athlete.Name},{athlete.BirthDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)},{athlete.CPF},{athlete.Gender},{athlete.Email},{athlete.Password},{athlete.RefreshToken},{athlete.Team},{athlete.Belt},{athlete.Weight},{athlete.UserId}");
        }

        return csvBuilder.ToString();
    }

    public byte[] ExportToPdf(List<Athlete> athletes)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            var document = new iTextSharp.text.Document(PageSize.A4);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            PdfPTable table = new PdfPTable(12);

            table.AddCell("Code");
            table.AddCell("Name");
            table.AddCell("BirthDate");
            table.AddCell("CPF");
            table.AddCell("Gender");
            table.AddCell("Email");
            table.AddCell("Password");
            table.AddCell("RefreshToken");
            table.AddCell("Team");
            table.AddCell("Belt");
            table.AddCell("Weight");
            table.AddCell("UserId");
           

            foreach (var athlete in athletes)
            {
                table.AddCell(athlete.Code);
                table.AddCell(athlete.Name);
                table.AddCell(new PdfPCell(new Phrase(athlete.BirthDate.ToString("yyyy-MM-dd"))));
                table.AddCell(athlete.CPF);
                table.AddCell(new PdfPCell(new Phrase(athlete.Gender.ToString())));
                table.AddCell(athlete.Email);
                table.AddCell(athlete.Password);
                table.AddCell(athlete.RefreshToken);
                table.AddCell(athlete.Team);
                table.AddCell(new PdfPCell(new Phrase(athlete.Belt.ToString())));
                table.AddCell(new PdfPCell(new Phrase(athlete.Weight.ToString())));
                table.AddCell(athlete.UserId.HasValue ? new PdfPCell(new Phrase(athlete.UserId.Value.ToString())) : new PdfPCell(new Phrase("")));
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }
    }
}