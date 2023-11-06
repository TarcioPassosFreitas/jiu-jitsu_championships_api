using FluentValidation;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using System;

namespace JiuJitsuMaster.Core.Validators
{
    public class ChampionshipValidator : AbstractValidator<Championship>
    {
        public ChampionshipValidator(IChampionshipRepository championshipRepository)
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.City).NotEmpty();
            RuleFor(x => x.State).NotEmpty();
            RuleFor(x => x.Gym).NotEmpty();
            RuleFor(x => x.EventDate).NotEmpty();

            RuleFor(x => x).MustAsync(async (x, cancellationToken) =>
            {
                var existingChampionship = await championshipRepository.GetByDetailsAsync(
                    x.Title, x.State, x.City, x.Gym, x.EventDate, cancellationToken);
                return existingChampionship == null;
            }).WithMessage("Campeonato já existente com os mesmos detalhes")
              .WithErrorCode("JJ-CRD4091");
        }
    }
}