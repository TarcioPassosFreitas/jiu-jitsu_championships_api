using JiuJitsuMaster.Core.Aggregates;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace JiuJitsuMaster.Infrastructure.Database.Contexts;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRecoverPassword> UserRecoverPasswords => Set<UserRecoverPassword>();
    public DbSet<UserCreatePassword> UserCreatePasswords => Set<UserCreatePassword>();
    public DbSet<Athlete> Athletes => Set<Athlete>();
    public DbSet<AthleteFight> AthleteFights => Set<AthleteFight>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Championship> Championships => Set<Championship>();
    public DbSet<Fight> Fights => Set<Fight>();
    public DbSet<Results> Results => Set<Results>();
    public DbSet<AthleteResult> AthleteResults => Set<AthleteResult>();
    public DbSet<AthleteChampionship> AthleteChampionships => Set<AthleteChampionship>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}