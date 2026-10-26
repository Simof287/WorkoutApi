using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WorkoutApi.Data;

public sealed class WorkoutContextFactory
    : IDesignTimeDbContextFactory<WorkoutContext>
{
    public WorkoutContext CreateDbContext(string[] args)
    {
        var contentRoot = FindContentRoot();
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environments.Production;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(contentRoot)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(
                $"appsettings.{environment}.json",
                optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("WorkoutDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The ConnectionStrings:WorkoutDatabase configuration is missing.");
        }

        var options = new DbContextOptionsBuilder<WorkoutContext>()
            .UseSqlite(connectionString)
            .Options;

        return new WorkoutContext(options);
    }

    private static string FindContentRoot()
    {
        foreach (var startPath in new[]
                 {
                     Directory.GetCurrentDirectory(),
                     AppContext.BaseDirectory
                 })
        {
            var directory = new DirectoryInfo(startPath);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
                    return directory.FullName;

                directory = directory.Parent;
            }
        }

        throw new InvalidOperationException(
            "Could not find appsettings.json from the current directory or application directory.");
    }
}
