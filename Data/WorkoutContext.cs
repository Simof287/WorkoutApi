using System;
using Microsoft.EntityFrameworkCore;
using WorkoutApi.Models;

namespace WorkoutApi.Data;

public class WorkoutContext : DbContext
{
    public WorkoutContext(DbContextOptions<WorkoutContext> options) : base(options){}
    public DbSet<Exercise> Exercises => Set<Exercise>();
}
