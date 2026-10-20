using System;
using Microsoft.EntityFrameworkCore;
using WorkoutApi.Models;

namespace WorkoutApi.Data;

public class WorkoutContext : DbContext
{
    public WorkoutContext(DbContextOptions<WorkoutContext> options) : base(options){}
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises
        => Set<WorkoutExercise>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkoutExercise>()
            .HasOne(we => we.Workout)
            .WithMany(w => w.Exercises)
            .HasForeignKey(we => we.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkoutExercise>()
            .HasOne(we => we.Exercise)
            .WithMany()
            .HasForeignKey(we => we.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkoutSet>()
            .HasOne(s => s.WorkoutExercise)
            .WithMany(we => we.Sets)
            .HasForeignKey(s => s.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkoutSet>()
            .Property(s => s.Weight)
            .HasPrecision(3, 2);

        modelBuilder.Entity<WorkoutSet>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_WorkoutSet_Repetitions",
                "\"Repetitions\" > 0"));

        modelBuilder.Entity<WorkoutSet>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_WorkoutSet_Weight",
                "\"Weight\" >= 0"));
    }
}
