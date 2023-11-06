using FluentValidation;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.Aggregates;
using CommonUtility.Extensions;

namespace JiuJitsuMaster.Core.Validators.AthleteValidator;

public class AddAthleteValidator : AbstractValidator<Athlete>
{
    public AddAthleteValidator(IAthleteRepository athleteRepository, IServiceProvider serviceProvider)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O nome é obrigatório.")
            .WithErrorCode("JJ-CRD4001");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("A senha é obrigatória.")
            .WithErrorCode("JJ-CRD4002")
            .Must(password => password.IsValidPassword())
            .WithMessage("A senha deve ter pelo menos 8 caracteres, incluindo pelo menos um caractere maiúsculo," +
            " um caractere minúsculo, um número e um caractere especial.")
            .WithErrorCode("JJ-CRD4003");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("O e-mail é obrigatório.")
            .WithErrorCode("JJ-CRD4004")
            .EmailAddress()
            .WithMessage("O e-mail é inválido.")
            .WithErrorCode("JJ-CRD4005")
            .MustAsync(async (x, email, cancellationToken) =>
            {
                var athlete = await athleteRepository.GetByEmailAsync(email, cancellationToken);
                return athlete == null || athlete.Id == x.Id;
            })
            .WithMessage("O e-mail já está em uso.")
            .WithErrorCode("JJ-CRD4090");

        RuleFor(x => x.CPF)
            .NotEmpty()
            .WithMessage("O CPF é obrigatório.")
            .WithErrorCode("JJ-CRD4006")
            .Matches(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$")
            .WithMessage("O CPF é inválido.")
            .WithErrorCode("JJ-CRD4007");

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("A data de nascimento é obrigatória.")
            .WithErrorCode("JJ-CRD4008");

        RuleFor(x => x.Team)
            .NotEmpty()
            .WithMessage("A equipe é obrigatória.")
            .WithErrorCode("JJ-CRD4009");

        RuleFor(x => x.Belt)
            .IsInEnum()
            .WithMessage("A faixa é inválida.")
            .WithErrorCode("JJ-CRD4010");

        RuleFor(x => x.Weight)
            .IsInEnum()
            .WithMessage("O peso é inválido.")
            .WithErrorCode("JJ-CRD4011");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("O gênero é inválido.")
            .WithErrorCode("JJ-CRD4012");
    }
}
