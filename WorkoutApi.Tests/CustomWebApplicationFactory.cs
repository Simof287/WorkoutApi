
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Data.Sqlite;
using WorkoutApi.Data;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    public CustomWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Issuer", "WorkoutApi.Tests");
        builder.UseSetting("Jwt:Audience", "WorkoutApi.Tests");
        builder.UseSetting(
            "Jwt:SigningKey",
            "WorkoutApi.Tests.Signing.Key.Must.Be.At.Least.32.Bytes.Long");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<WorkoutContext>>();

            services.AddDbContext<WorkoutContext>(options =>
                options.UseSqlite(_connection));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<WorkoutContext>();

            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
    }
}
