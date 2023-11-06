using FluentValidation;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;

namespace JiuJitsuMaster.Core.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Email).MustAsync(async (x, email, cancellationToken) =>
        {
            var user = await userRepository.GetByEmailAsync(email, cancellationToken);

            return user == null || user.Id == x.Id;
        }).WithMessage("E-mail já em uso").WithErrorCode("JJ-CRD4090");
    }
}