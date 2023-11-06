using Ardalis.Result;
using CommonUtility.Extensions;
using CommonUtility.Service;
using FluentValidation;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.Interfaces.Services;
using JiuJitsuMaster.Core.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace JiuJitsuMaster.Core.Services;

public partial class UserService : BaseService<User>, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAthleteRepository _athleteRepository;
    private readonly ITokenService _tokenService;
    private readonly IMailKitService _mailKitService;
    private readonly IUserCreatePasswordRepository _userCreatePasswordRepository;
    private readonly IValidator<User> _userValidator;
    private readonly IValidator<User> _userUpdateValidator;

    public UserService(
        IServiceProvider serviceProvider,
        IUserRepository userRepository,
        IAthleteRepository athleteRepository,
        ITokenService tokenService,
        IMailKitService mailKitService,
        IUserCreatePasswordRepository userCreatePasswordRepository
        )
    {
        _userRepository = userRepository;
        _athleteRepository = athleteRepository;
        _tokenService = tokenService;
        _mailKitService = mailKitService;
        _userCreatePasswordRepository = userCreatePasswordRepository;
        _userValidator = serviceProvider.GetService<UserValidator>()!;
        _userUpdateValidator = serviceProvider.GetService<UserUpdateValidator>()!;
    }

    public async Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null || user.Deleted)
            return NotFound("Usuário não encontrado");

        return Result<User>.Success(user);
    }

    public async Task<Result<User>> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);

        if (user == null || user.Deleted)
            return NotFound("Usuário não encontrado");

        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _userRepository.UpdateRefreshTokenAsync(user.Id, newRefreshToken, cancellationToken);

        user.RefreshToken = newRefreshToken;

        return Result<User>.Success(user);
    }

    public async Task<Result<User>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null || user.Deleted)
            return NotFound("Usuário não encontrado");

        var passwordHasher = new PasswordHasher<User>();
        var passwordCheckResult = passwordHasher.VerifyHashedPassword(user, user.Password, password);

        if (passwordCheckResult == PasswordVerificationResult.Failed)
            return NotFound("Credenciais inválidas");

        var refreshToken = _tokenService.GenerateRefreshToken();

        await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

        user.RefreshToken = refreshToken;

        return Result<User>.Success(user);
    }

    public async Task<Result<User>> RegisterUsersAsync(User user, CancellationToken cancellationToken)
    {
        await using var transaction = _userRepository.BeginTransaction();
        try
        {
            var validate = await _userValidator.ValidateAsync(user);

            if (!validate.IsValid) return Result<User>.Invalid(validate.Errors.ToResultValidation());

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);

            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;

            var result = await _userRepository.AddAsync(user, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result.Success(result);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Error();
        }
    }

    public async Task<Result<User>> ResetPasswordAsync(long id, string newPassword, string confirmPassword, CancellationToken cancellationToken)
    {
        var userById = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (userById == null || userById.Deleted)
            return NotFound("Usuário não encontrado");

        return await UpdatePassword(userById, newPassword, confirmPassword, cancellationToken);
    }

    public async Task<Result<Athlete>> ResetAthletePasswordAsync(long id, string newPassword, string confirmPassword, CancellationToken cancellationToken)
    {
        var athleteById = await _athleteRepository.GetByIdAsync(id, cancellationToken);

        if (athleteById == null || athleteById.Deleted)
            return NotFound("Usuário não encontrado");

        return await UpdateAthletePassword(athleteById, newPassword, confirmPassword, cancellationToken);
    }

    private async Task<Result<User>> UpdatePassword(User user, string newPassword, string confirmPassword,
        CancellationToken cancellationToken)
    {
        var passwordHasher = new PasswordHasher<User>();

        if (newPassword != confirmPassword)
            return WrongPassword("As senhas não correspondem");

        if (!newPassword.IsValidPassword())
            return InvalidFormat("A senha não corresponde às regras exigidas");

        user.Password = passwordHasher.HashPassword(user, newPassword);

        await _userRepository.ChangePasswordAsync(user.Id, user.Password, cancellationToken);

        return Result.Success(user);
    }

    private async Task<Result<Athlete>> UpdateAthletePassword(Athlete athlete, string newPassword, string confirmPassword,
       CancellationToken cancellationToken)
    {
        var passwordHasher = new PasswordHasher<Athlete>();

        if (newPassword != confirmPassword)
            return WrongPassword("As senhas não correspondem");

        if (!newPassword.IsValidPassword())
            return InvalidFormat("A senha não corresponde às regras exigidas");

        athlete.Password = passwordHasher.HashPassword(athlete, newPassword);

        await _userRepository.ChangePasswordAsync(athlete.Id, athlete.Password, cancellationToken);

        return Result.Success(athlete);
    }

    public async Task<Result<User>> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        await using var transaction = _userRepository.BeginTransaction();

        try
        {
            var verifyUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);

            if (verifyUser == null || verifyUser.Deleted)
                return NotFound("Usuário não encontrado");

            var validatedUser = await _userUpdateValidator.ValidateAsync(user, cancellationToken);

            if (!validatedUser.IsValid)
                return Result<User>.Invalid(validatedUser.Errors.ToResultValidation());

            if (user.Name != null)
                verifyUser.Name = user.Name;

            if (user.Email != null)
                verifyUser.Email = user.Email;

            await _userRepository.UpdateAsync(verifyUser, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result<User>.Success(user);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Error();
        }
    }

    public async Task<Result<User>> ExcludeAsync(long id, CancellationToken cancellationToken)
    {
        await using var transaction = _userRepository.BeginTransaction();

        try
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            if (user == null || user.Deleted)
                return NotFound("Usuário não encontrado");

            await _userRepository.ExcludeAsync(user, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result<User>.Success(user);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Error();
        }
    }

    public async Task<Result<PaginatedList<User>>> ListAllAsync(int page, int limit, string? name, CancellationToken cancellationToken)
    {
        var data = await _userRepository.ListAllAsync(page, limit, name, cancellationToken);

        if (data.Count == 0)
            return Result.Success(data);

        return page > data.TotalPages ? InvalidFormat("Número de página inválido") : Result.Success(data);
    }
}
