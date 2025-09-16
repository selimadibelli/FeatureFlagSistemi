using Microsoft.AspNetCore.Mvc;
using FeatureFlagSystem.Application.Services;
using FeatureFlagSystem.Domain.DTOs;

namespace FeatureFlagSystem.Features.FeatureFlagCheck;

public static class FeatureFlagCheckEndpoint
{
    public static void MapFeatureFlagCheckEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/feature-flags")
            .WithTags("Feature Flag Check");

        group.MapPost("/validation", CheckFeatureFlag)
            .WithName("CheckFeatureFlag")
            .WithSummary("Feature flag kontrolü yapar")
            .WithDescription("Belirtilen kullanıcı için feature flag'in aktif olup olmadığını kontrol eder");

        group.MapPost("/validation/batch", CheckMultipleFeatureFlags)
            .WithName("CheckMultipleFeatureFlags")
            .WithSummary("Birden fazla feature flag kontrolü yapar")
            .WithDescription("Belirtilen kullanıcı için birden fazla feature flag'in aktif olup olmadığını kontrol eder");
    }

    private static async Task<IResult> CheckFeatureFlag(
        [FromBody] FeatureFlagCheckDto request,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            if (string.IsNullOrEmpty(request.FeatureName) || string.IsNullOrEmpty(request.UserIdentifier))
            {
                return Results.BadRequest("FeatureName ve UserIdentifier alanları zorunludur");
            }

            var result = await featureFlagService.CheckFeatureFlagAsync(request);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Feature flag kontrolü sırasında hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> CheckMultipleFeatureFlags(
        [FromBody] List<FeatureFlagCheckDto> requests,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            if (requests == null || !requests.Any())
            {
                return Results.BadRequest("En az bir feature flag kontrolü gereklidir");
            }

            var results = new List<FeatureFlagCheckResultDto>();
            
            foreach (var request in requests)
            {
                if (string.IsNullOrEmpty(request.FeatureName) || string.IsNullOrEmpty(request.UserIdentifier))
                {
                    results.Add(new FeatureFlagCheckResultDto
                    {
                        IsEnabled = false,
                        IsInPilot = false,
                        Reason = "FeatureName ve UserIdentifier alanları zorunludur"
                    });
                    continue;
                }

                var result = await featureFlagService.CheckFeatureFlagAsync(request);
                results.Add(result);
            }

            return Results.Ok(results);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Feature flag kontrolleri sırasında hata oluştu: {ex.Message}");
        }
    }
}

