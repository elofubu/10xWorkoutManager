using Microsoft.EntityFrameworkCore;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Data;

public class WorkoutManagerDbContext : DbContext
{
    public WorkoutManagerDbContext(DbContextOptions<WorkoutManagerDbContext> options) : base(options)
    {
    }

    public DbSet<MuscleGroup> MuscleGroups => Set<MuscleGroup>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<TrainingDay> TrainingDays => Set<TrainingDay>();
    public DbSet<PlanDayExercise> PlanDayExercises => Set<PlanDayExercise>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionExercise> SessionExercises => Set<SessionExercise>();
    public DbSet<ExerciseSet> ExerciseSets => Set<ExerciseSet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        const string now = "SYSUTCDATETIME()";

        modelBuilder.Entity<MuscleGroup>(e =>
        {
            e.ToTable("muscle_groups");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql(now);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Exercise>(e =>
        {
            e.ToTable("exercises");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql(now);
            // SQL Server unique indexes treat NULLs as equal, so this one index covers both
            // predefined exercises (UserId NULL) and per-user exercises.
            e.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
            e.HasOne<MuscleGroup>().WithMany().HasForeignKey(x => x.MuscleGroupId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutPlan>(e =>
        {
            e.ToTable("workout_plans");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql(now);
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<TrainingDay>(e =>
        {
            e.ToTable("training_days", t => t.HasCheckConstraint("CK_training_days_order", "[Order] >= 0"));
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql(now);
            e.HasOne<WorkoutPlan>().WithMany(p => p.TrainingDays).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Exercises).WithMany()
                .UsingEntity<PlanDayExercise>(
                    j => j.HasOne(x => x.Exercise).WithMany().HasForeignKey(x => x.ExerciseId).OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne(x => x.TrainingDay).WithMany(d => d.PlanDayExercises).HasForeignKey(x => x.TrainingDayId).OnDelete(DeleteBehavior.Cascade));
        });

        modelBuilder.Entity<PlanDayExercise>(e =>
        {
            e.ToTable("plan_day_exercises", t => t.HasCheckConstraint("CK_plan_day_exercises_order", "[Order] >= 0"));
            e.HasKey(x => x.Id);
            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql(now);
            e.HasIndex(x => new { x.TrainingDayId, x.ExerciseId }).IsUnique();
            e.HasIndex(x => x.ExerciseId);
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.ToTable("sessions");
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.Property(x => x.StartTime).HasColumnType("datetime2").HasDefaultValueSql(now);
            e.Property(x => x.EndTime).HasColumnType("datetime2");
            e.Property(x => x.TrainingDayId).IsRequired();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.TrainingDay).WithMany().HasForeignKey(x => x.TrainingDayId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SessionExercise>(e =>
        {
            e.ToTable("session_exercises", t => t.HasCheckConstraint("CK_session_exercises_order", "[Order] >= 0"));
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasOne(x => x.Session).WithMany(s => s.SessionExercises).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Exercise).WithMany().HasForeignKey(x => x.ExerciseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExerciseSet>(e =>
        {
            e.ToTable("exercise_sets", t =>
            {
                t.HasCheckConstraint("CK_exercise_sets_weight", "[Weight] >= 0");
                t.HasCheckConstraint("CK_exercise_sets_reps", "[Reps] >= 0");
                t.HasCheckConstraint("CK_exercise_sets_order", "[Order] >= 0");
            });
            e.Property(x => x.Weight).HasColumnType("decimal(10,2)");
            e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql(now);
            e.HasOne<SessionExercise>().WithMany(se => se.Sets).HasForeignKey(x => x.SessionExerciseId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
