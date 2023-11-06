using FluentValidation;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;

namespace JiuJitsuMaster.Core.Validators;

public class UserUpdateValidator : AbstractValidator<User>
{
    public UserUpdateValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).MustAsync(async (x, email, cancellationToken) =>
        {
            var user = await userRepository.GetByEmailAsync(email, cancellationToken);

            return user == null || user.Id == x.Id;
        }).WithMessage("E-mail já em uso").WithErrorCode("JJ-CRD4090");
    }
}
