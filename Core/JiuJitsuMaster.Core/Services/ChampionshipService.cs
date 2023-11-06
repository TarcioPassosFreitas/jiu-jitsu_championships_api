using Ardalis.Result;
using CommonUtility.Extensions;
using CommonUtility.Service;
using FluentValidation;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.Interfaces.Services;
using JiuJitsuMaster.Core.Validators;
using JiuJitsuMaster.Core.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace JiuJitsuMaster.Core.Services
{
    public partial class ChampionshipService : BaseService<Championship>, IChampionshipService
    {
        private readonly IChampionshipRepository _championshipRepository;
        private readonly IAthleteRepository _athleteRepository;
        private readonly IAthleteChampionshipRepository _athleteChampionshipRepository;
        private readonly IFightRepository _fightRepository;
        private readonly IAthleteFightRepository _athleteFightRepository;
        private readonly IAthleteResultRepository _athleteResultRepository;
        private readonly IResultRepository _resultRepository;
        private readonly IGitHubStorageService _gitHubStorageService;
        private readonly IValidator<Championship> _championshipValidator;
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public ChampionshipService(
            IServiceProvider serviceProvider,
            IChampionshipRepository championshipRepository,
            IAthleteRepository athleteRepository,
            IAthleteChampionshipRepository athleteChampionshipRepository,
            IAthleteResultRepository athleteResultRepository,
            IResultRepository resultRepository,
            IFightRepository fightRepository,
            IAthleteFightRepository athleteFightRepository,
            IGitHubStorageService gitHubStorageService
        )
        {
            _championshipRepository = championshipRepository;
            _athleteRepository = athleteRepository;
            _fightRepository = fightRepository;
            _athleteFightRepository = athleteFightRepository;
            _athleteChampionshipRepository = athleteChampionshipRepository;
            _resultRepository = resultRepository;
            _athleteResultRepository = athleteResultRepository;
            _gitHubStorageService = gitHubStorageService;
            _championshipValidator = serviceProvider.GetService<ChampionshipValidator>()!;
        }

        public async Task<Result<Championship>> AddAsync(Championship championship, IFormFile imageFile, 
            int? x, int? y, int? width, int? height, CancellationToken cancellationToken)
        {
            await using var transaction = _championshipRepository.BeginTransaction();

            try
            {
                var validatedChampionship = await _championshipValidator.ValidateAsync(championship, cancellationToken);

                if (!validatedChampionship.IsValid)
                    return Result<Championship>.Invalid(validatedChampionship.Errors.ToResultValidation());

                var storageImageResponse = await _gitHubStorageService.AddImageAsync(imageFile, x, y, width, height, cancellationToken);

                if (storageImageResponse.IsSuccess)
                    championship.Image = storageImageResponse.Value;
                else
                    return InvalidFormat(storageImageResponse.Errors.FirstOrDefault());

                var result = await _championshipRepository.AddAsync(championship, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Result.Success(result);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Error();
            }
        }

        public async Task<Result<Championship>> ChangePhaseAsync(long id, ChampionshipPhase phase, CancellationToken cancellationToken)
        {
            var championship = await _championshipRepository.GetByIdAsync(id, cancellationToken);

            if (championship == null || championship.Deleted)
                return NotFound("Campeonato não encontrado");

            if (phase == ChampionshipPhase.Brackets)
            {
                var athletes = await _athleteChampionshipRepository.GetByChampionshipAsync(championship.Id, cancellationToken);

                if (athletes == null)
                    return NotFound("Não tem inscritos nesse campeonato para a fase de chave de lutas");

                var fights =  await GenerateFightsAsync(championship.Id, athletes, 1, cancellationToken);

                await _fightRepository.AddAllAsync(fights, cancellationToken);
            }

            await _championshipRepository.ChangePhaseAsync(id, phase, cancellationToken);

            return Result.Success();
        }

        public async Task<Result<Championship>> ExcludeAsync(long id, CancellationToken cancellationToken)
        {
            await using var transaction = _championshipRepository.BeginTransaction();

            try
            {
                var championship = await _championshipRepository.GetByIdAsync(id, cancellationToken);

                if (championship == null || championship.Deleted)
                    return NotFound("Usuário não encontrado");

                await _championshipRepository.ExcludeAsync(championship, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Result<Championship>.Success(championship);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result.Error();
            }
        }

        public async Task<Result<List<Championship>>> GetHighlightedAsync(CancellationToken cancellationToken)
        {
            var data = await _championshipRepository.GetHighlightedAsync(cancellationToken);

            return Result.Success(data);
        }

        public async Task<Result<List<Championship>>> GetNonHighlightedAsync(int page, int limit, string? state, string? city,
        ChampionshipType? type, string? title, CancellationToken cancellationToken)
        {
            var data = await _championshipRepository.GetNonHighlightedAsync(page, limit, state, city, type,
                title, cancellationToken);

            return Result.Success(data);
        }

        public async Task<Result<PaginatedList<Championship>>> ListAllAsync(int page, int limit, string? title,
        string? city, string? state, ChampionshipStatus? status, CancellationToken cancellationToken)
        {
            var data = await _championshipRepository.ListAllAsync(page, limit, title, city, state, status, cancellationToken);

            if (data.Count == 0)
                return Result.Success(data);

            return page > data.TotalPages ? InvalidFormat("Número de página inválido") : Result.Success(data);
        }

        public async Task<Result<Championship>> UpdateAsync(Championship? championship, IFormFile? imageFile,
            int? x, int? y, int? width, int? height, CancellationToken cancellationToken)
        {
            await using var transaction = _championshipRepository.BeginTransaction();

            try
            {
                var verifyChampionship = await _championshipRepository.GetByIdAsync(championship!.Id, cancellationToken);

                if (verifyChampionship == null || verifyChampionship.Deleted)
                    return NotFound("Campeonato não encontrado");

                var conflicts = await _championshipRepository.GetByDetailsAsync(
                    championship.Title, championship.State, championship.City, championship.Gym,
                    championship.EventDate, cancellationToken);


                if (conflicts.Any())
                {
                    var conflictError = new ValidationError
                    {
                        
                        ErrorCode = "JJ-CRD4092",
                        Identifier = "Conflito de dados",
                        ErrorMessage = $"Campeonato tem conflitos nos seguintes campos: {string.Join(", ", conflicts)}"
                    };

                    return Result<Championship>.Invalid(new List<ValidationError> { conflictError });
                }

                if (championship.Code != null)
                    verifyChampionship.Code = championship.Code;

                if (championship.Title != null)
                    verifyChampionship.Title = championship.Title;

                if (championship.City != null)
                    verifyChampionship.City = championship.City;

                if (championship.State != null)
                    verifyChampionship.State = championship.State;

                if (!(championship.EventDate.Equals(verifyChampionship.EventDate)) && championship.EventDate != DateTime.MinValue)
                    verifyChampionship.EventDate = championship.EventDate;

                if (championship.AboutEvent != null)
                    verifyChampionship.AboutEvent = championship.AboutEvent;

                if (championship.Gym != null)
                    verifyChampionship.Gym = championship.Gym;

                if (championship.GeneralInfo != null)
                    verifyChampionship.GeneralInfo = championship.GeneralInfo;

                if (championship.PublicEntry != null)
                    verifyChampionship.PublicEntry = championship.PublicEntry;

                if (championship.Type != 0 && championship.Type != verifyChampionship.Type)
                    verifyChampionship.Type = championship.Type;

                if (championship.Phase != 0 && championship.Phase != verifyChampionship.Phase)
                    verifyChampionship.Phase = championship.Phase;

                if (championship.Status != 0 && championship.Status != verifyChampionship.Status)
                    verifyChampionship.Status = championship.Status;

                if (imageFile != null)
                {
                    var removeImageStorage = await _gitHubStorageService.RemoveImageAsync(verifyChampionship.Image);

                    if (!removeImageStorage.IsSuccess)
                        return InvalidFormat(removeImageStorage.Errors.FirstOrDefault());

                    var storageImageResponse = await _gitHubStorageService.AddImageAsync(imageFile, x, y, width, height, cancellationToken);

                    if (storageImageResponse.IsSuccess)
                        verifyChampionship.Image = storageImageResponse.Value;
                    else
                        return InvalidFormat(storageImageResponse.Errors.FirstOrDefault());
                }

                await _championshipRepository.UpdateAsync(verifyChampionship, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Result<Championship>.Success(championship);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result.Error();
            }
        }

        public async Task<Result> UpdateHighlightsAsync(List<long> highlights, CancellationToken cancellationToken)
        {
            await using var transaction = _championshipRepository.BeginTransaction();

            try
            {
                if (highlights.Count > 8)
                    return InvalidFormat("Você só pode destacar até 8 campeonatos.");

                var validChampionships = await _championshipRepository.GetByIdsAsync(highlights, cancellationToken);

                if (!validChampionships)
                    return NotFound("Campeonatos não foram encontrados ou não estão ativos.");

                var previouslyHighlighted = await _championshipRepository.GetHighlightedAsync(cancellationToken);

                if (previouslyHighlighted.Any())
                {
                    foreach (var championship in previouslyHighlighted)
                        await _championshipRepository.UpdatePreviouslyHighlightedAsync(championship.Id, false, null, cancellationToken);
                }

                var count = 0;
                foreach (var id in highlights)
                {
                    await _championshipRepository.UpdatePreviouslyHighlightedAsync(id, true, count++, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result.Error();
            }
        }

        public async Task<Result> RecordFightResultsAsync(long championshipId, List<long> fightResults, CancellationToken cancellationToken)
        {
            await using var transaction = _championshipRepository.BeginTransaction();

            try
            {
                var championship = await _championshipRepository.GetByIdAsync(championshipId, cancellationToken);

                if (championship == null || championship.Deleted)
                    return NotFound("Campeonato não encontrado.");

                foreach (var fightResult in fightResults)
                {
                    var fight = await _athleteFightRepository.GetByIdAsync(fightResult, cancellationToken);

                    if (fight == null || fight.Deleted)
                        return NotFound($"Luta com ID {fightResult} não encontrada.");

                    fight.IsFinished = true;
                    fight.Winner = true;

                    await _athleteFightRepository.UpdateAsync(fight, cancellationToken);
                }

                var unfinishedFights = await _athleteFightRepository.GetUnfinishedFightsAsync(cancellationToken);

                foreach (var fight in unfinishedFights)
                {
                    fight.IsFinished = true;

                    if (!fight.Winner.HasValue)
                        fight.Winner = false;

                    await _athleteFightRepository.UpdateAsync(fight, cancellationToken);
                }

                var athleteFightsResultRounds = await _athleteFightRepository.GetByRoundAsync(cancellationToken);

                if (!athleteFightsResultRounds)
                    return InvalidFormat("A rodada atual ainda não chegou ao fim");
                else
                {
                    var athleteChampionships = new List<AthleteChampionship>();
                    int round = 0;

                    foreach (var fightResult in fightResults)
                    {
                        var athleteResult = _athleteFightRepository.GetByIdAsync(fightResult, cancellationToken);

                        var winners = await _athleteChampionshipRepository.GetByChampionshipAndAthleteAsync(championshipId, athleteResult.Result!.AthleteId, cancellationToken);

                        if (winners != null)
                            athleteChampionships.Add(winners);
                    }

                    var roundCurrent = await _athleteFightRepository.GetByNumberRoundAsync(cancellationToken);

                    var nextRoundFights = await GenerateFightsAsync(championshipId, athleteChampionships, roundCurrent + 1, cancellationToken);

                    await _fightRepository.AddAllAsync(nextRoundFights, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result.Error();
            }
        }

        public async Task<Result<FileData>> ExportResultsAsync(long championshipId, CancellationToken cancellationToken)
        {
            await using var transaction = _championshipRepository.BeginTransaction();
            try
            {
                var championship = await _championshipRepository.GetByIdAsync(championshipId, cancellationToken);
                if (championship == null || championship.Deleted)
                {
                    return NotFound("Campeonato não encontrado.");
                }

                var fights = await _fightRepository.GetFightsByChampionshipId(championshipId, cancellationToken);

                var finalResults = ProcessFightsToResults(fights);

                foreach (var result in finalResults)
                {
                    var resultEntity = new Aggregates.Results
                    {
                        ChampionshipId = championshipId,
                        Belt = result.Belt,
                        Weight = result.Weight,
                        AthleteResults = new List<AthleteResult>()
                    };
                    await _resultRepository.AddAsync(resultEntity, cancellationToken);

                    foreach (var athleteResult in result.AthleteResults)
                    {
                        athleteResult.ResultId = resultEntity.Id;
                        await _athleteResultRepository.AddAsync(athleteResult, cancellationToken);
                    }
                }

                var fileData = GenerateCsv(finalResults);

                await transaction.CommitAsync(cancellationToken);

                return Result.Success(fileData);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Error();
            }
        }



        //Métodos privados

        private async Task<List<Fight?>> GenerateFightsAsync(long championshipId, List<AthleteChampionship> athletes, int round, CancellationToken cancellationToken)
        {
            var fights = new List<Fight>();
            var random = new Random();

            var athletesByCategory = athletes
                .GroupBy(a => new { a.Athlete.Belt, a.Athlete.Weight, a.Athlete.Gender })
                .ToList();

            await _lock.WaitAsync(cancellationToken);
            try
            {
                foreach (var categoryGroup in athletesByCategory)
                {
                    var shuffledAthletes = ShuffleList(categoryGroup.ToList(), random);
                    var queue = new Queue<AthleteChampionship>(shuffledAthletes);

                    while (queue.Count > 1)
                    {
                        var firstAthlete = queue.Dequeue();
                        var secondAthlete = queue.FirstOrDefault(a => a.Athlete.Team != firstAthlete.Athlete.Team);

                        if (secondAthlete != null)
                        {
                            queue = new Queue<AthleteChampionship>(queue.Where(a => a != secondAthlete));
                            fights.Add(CreateFight(championshipId, firstAthlete, secondAthlete, false, round));
                        }
                        else
                        {
                            if (queue.All(a => a.Athlete.Team == firstAthlete.Athlete.Team))
                            {
                                secondAthlete = queue.Dequeue();
                                fights.Add(CreateFight(championshipId, firstAthlete, secondAthlete, true, round));
                            }
                            else
                            {
                                queue.Enqueue(firstAthlete);
                            }
                        }
                    }

                    if (queue.Count == 1)
                    {
                        var lastAthlete = queue.Dequeue();
                        fights.Add(CreateFight(championshipId, lastAthlete, null, false, round));
                    }
                }
            }
            finally
            {
                _lock.Release();
            }

            return fights;
        }

        private Fight CreateFight(long championshipId, AthleteChampionship athlete1, AthleteChampionship athlete2, bool isInternal, int round)
        {
            var fight = new Fight
            {
                ChampionshipId = championshipId,
                Belt = athlete1.Athlete.Belt,
                Weight = athlete1.Athlete.Weight,
                Round = round,
                IsInternalTeamFight = isInternal,
                AthleteFights = new List<AthleteFight>()
            };

            fight.AthleteFights.Add(new AthleteFight { AthleteId = athlete1.AthleteId, Fight = fight });

            if (athlete2 != null)
            {
                fight.AthleteFights.Add(new AthleteFight { AthleteId = athlete2.AthleteId, Fight = fight });
            }

            return fight;
        }

        private List<T> ShuffleList<T>(List<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        private List<Aggregates.Results> ProcessFightsToResults(List<Fight> fights)
        {
            var finalResults = new List<Aggregates.Results>();

            var groupedFights = fights
                .GroupBy(f => new { f.Belt, f.Weight })
                .ToList();

            foreach (var group in groupedFights)
            {
                var winners = group
                    .SelectMany(f => f.AthleteFights)
                    .Where(af => af.Winner.HasValue && af.Winner.Value)
                    .GroupBy(af => af.AthleteId)
                    .Select(g => new
                    {
                        AthleteId = g.Key,
                        Wins = g.Count(),
                        LastWinRound = g.Max(f => f.Fight.Round)
                    })
                    .OrderByDescending(w => w.LastWinRound)
                    .ThenByDescending(w => w.Wins)
                    .ToList();

                var resultsEntity = new Aggregates.Results
                {
                    ChampionshipId = group.First().ChampionshipId,
                    Belt = group.Key.Belt,
                    Weight = group.Key.Weight,
                    AthleteResults = winners.Select(winner => new AthleteResult
                    {
                        AthleteId = winner.AthleteId,
                        Placement = DeterminePlacement(winner.LastWinRound, group.Max(f => f.Round))
                    }).ToList()
                };

                finalResults.Add(resultsEntity);
            }

            return finalResults;
        }

        private PlacementType DeterminePlacement(int lastWinRound, int totalRounds)
        {
            if (lastWinRound == totalRounds)
            {
                return PlacementType.First;
            }
            else if (lastWinRound == totalRounds - 1)
            {
                return PlacementType.Second;
            }
            else if (lastWinRound <= totalRounds - 2) 
            {
                return PlacementType.Third;
            }
            else
            {
                return PlacementType.Unplaced;
            }
        }

        private FileData GenerateCsv(List<Aggregates.Results> finalResults)
        {
            var csvBuilder = new StringBuilder();

            csvBuilder.AppendLine("ChampionshipId,Belt,Weight,AthleteId,Placement");

            foreach (var result in finalResults)
            {
                foreach (var athleteResult in result.AthleteResults)
                {
                    csvBuilder.AppendLine($"{result.ChampionshipId},{result.Belt},{result.Weight},{athleteResult.AthleteId},{athleteResult.Placement}");
                }
            }

            var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());

            var fileName = $"ChampionshipResults-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return new FileData(csvBytes, fileName, "text/csv");
        }

        public async Task<Result<List<Championship>>> GetFightKeysAsync(long championshipId, CancellationToken cancellationToken)
        {
            var championship = await _championshipRepository.GetByIdAsync(championshipId, cancellationToken);

            if (championship == null)
                return NotFound("Campeonato não encontrado");

            if (championship.Phase != ChampionshipPhase.Brackets)
                return InvalidFormat("Não está na estapa de chaveamento");

            var fights = await _championshipRepository.GetFightKeysAsync(championshipId, cancellationToken);

            if (!fights.Any())
                return NotFound("Não foram encontrados fights");

            return fights;
        }
    }
}