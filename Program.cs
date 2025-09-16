using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Serilog;
using FeatureFlagSystem.Infrastructure.Data;
using FeatureFlagSystem.Infrastructure.Services;
using FeatureFlagSystem.Application.Services;
using FeatureFlagSystem.Features.FeatureFlagCheck;
using FeatureFlagSystem.Features.FeatureFlagManagement;

// Serilog konfigürasyonu
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Feature Flag System başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u kullan
    builder.Host.UseSerilog();

    // Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { 
            Title = "Feature Flag System API", 
            Version = "v1",
            Description = "Dolandırıcılık önleme pilot whitelist sistemi için feature flag yönetimi"
        });
    });

    // Entity Framework
    builder.Services.AddDbContext<FeatureFlagDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Redis
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        return ConnectionMultiplexer.Connect(configuration);
    });

    // Cache Service
    builder.Services.AddScoped<ICacheService, RedisCacheService>();

    // Application Services
    builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Pipeline konfigürasyonu
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Feature Flag System API v1");
        c.RoutePrefix = string.Empty; // Swagger UI'ı root'ta göster
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseSerilogRequestLogging();

    // Endpoint'leri map et
    app.MapFeatureFlagCheckEndpoints();
    app.MapFeatureFlagManagementEndpoints();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { 
        Status = "Healthy", 
        Timestamp = DateTime.UtcNow,
        Service = "Feature Flag System"
    }))
    .WithTags("Health")
    .WithName("HealthCheck");

    // Veritabanını oluştur
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<FeatureFlagDbContext>();
        try
        {
            context.Database.EnsureCreated();
            Log.Information("Veritabanı başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Veritabanı oluşturulurken hata oluştu");
        }
    }

    Log.Information("Feature Flag System başlatıldı. Port: {Port}", 
        builder.Configuration["ASPNETCORE_URLS"] ?? "5000");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılırken kritik hata oluştu");
}
finally
{
    Log.CloseAndFlush();
}
