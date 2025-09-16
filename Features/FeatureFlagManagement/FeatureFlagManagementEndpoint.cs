using Microsoft.AspNetCore.Mvc;
using FeatureFlagSystem.Application.Services;
using FeatureFlagSystem.Domain.DTOs;

namespace FeatureFlagSystem.Features.FeatureFlagManagement;

public static class FeatureFlagManagementEndpoint
{
    public static void MapFeatureFlagManagementEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/feature-flags")
            .WithTags("Feature Flag Management");

        // Feature Flag CRUD operations
        group.MapGet("/", GetAllFeatureFlags)
            .WithName("GetAllFeatureFlags")
            .WithSummary("Tüm feature flag'leri listeler");

        group.MapGet("/{id:int}", GetFeatureFlagById)
            .WithName("GetFeatureFlagById")
            .WithSummary("ID'ye göre feature flag getirir");

        group.MapGet("/by-name/{name}", GetFeatureFlagByName)
            .WithName("GetFeatureFlagByName")
            .WithSummary("İsme göre feature flag getirir");

        group.MapPost("/", CreateFeatureFlag)
            .WithName("CreateFeatureFlag")
            .WithSummary("Yeni feature flag oluşturur");

        group.MapPut("/{id:int}", UpdateFeatureFlag)
            .WithName("UpdateFeatureFlag")
            .WithSummary("Feature flag günceller");

        group.MapDelete("/{id:int}", DeleteFeatureFlag)
            .WithName("DeleteFeatureFlag")
            .WithSummary("Feature flag siler");

        // Pilot Whitelist operations
        group.MapGet("/{id:int}/pilot-whitelist", GetPilotWhitelists)
            .WithName("GetPilotWhitelists")
            .WithSummary("Feature flag'in pilot whitelist'ini getirir");

        group.MapPost("/pilot-whitelist", AddToPilotWhitelist)
            .WithName("AddToPilotWhitelist")
            .WithSummary("Pilot whitelist'e kullanıcı ekler");

        group.MapDelete("/pilot-whitelist/{whitelistId:int}", RemoveFromPilotWhitelist)
            .WithName("RemoveFromPilotWhitelist")
            .WithSummary("Pilot whitelist'ten kullanıcı çıkarır");

        // Cache operations
        group.MapPost("/cache", RefreshCache)
            .WithName("RefreshCache")
            .WithSummary("Cache'i yeniler");
    }

    private static async Task<IResult> GetAllFeatureFlags([FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            var featureFlags = await featureFlagService.GetAllFeatureFlagsAsync();
            return Results.Ok(featureFlags);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Feature flag'ler getirilirken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> GetFeatureFlagById(
        int id,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            var featureFlag = await featureFlagService.GetFeatureFlagByIdAsync(id);
            if (featureFlag == null)
            {
                return Results.NotFound($"ID {id} ile feature flag bulunamadı");
            }
            return Results.Ok(featureFlag);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Feature flag getirilirken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> GetFeatureFlagByName(
        string name,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            var featureFlag = await featureFlagService.GetFeatureFlagByNameAsync(name);
            if (featureFlag == null)
            {
                return Results.NotFound($"'{name}' isimli feature flag bulunamadı");
            }
            return Results.Ok(featureFlag);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Feature flag getirilirken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> CreateFeatureFlag(
        [FromBody] CreateFeatureFlagDto dto,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Name))
            {
                return Results.BadRequest("Name alanı zorunludur");
            }

            var featureFlag = await featureFlagService.CreateFeatureFlagAsync(dto);
            return Results.Created($"/api/feature-flags/{featureFlag.Id}", featureFlag);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Feature flag oluşturulurken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> UpdateFeatureFlag(
        int id,
        [FromBody] UpdateFeatureFlagDto dto,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            var featureFlag = await featureFlagService.UpdateFeatureFlagAsync(id, dto);
            if (featureFlag == null)
            {
                return Results.NotFound($"ID {id} ile feature flag bulunamadı");
            }
            return Results.Ok(featureFlag);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Feature flag güncellenirken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteFeatureFlag(
        int id,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            var result = await featureFlagService.DeleteFeatureFlagAsync(id);
            if (!result)
            {
                return Results.NotFound($"ID {id} ile feature flag bulunamadı");
            }
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Feature flag silinirken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> GetPilotWhitelists(
        int id,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            var whitelists = await featureFlagService.GetPilotWhitelistsAsync(id);
            return Results.Ok(whitelists);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Pilot whitelist getirilirken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> AddToPilotWhitelist(
        [FromBody] CreatePilotWhitelistDto dto,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.UserIdentifier))
            {
                return Results.BadRequest("UserIdentifier alanı zorunludur");
            }

            var whitelist = await featureFlagService.AddToPilotWhitelistAsync(dto);
            return Results.Created($"/api/feature-flags/pilot-whitelist/{whitelist.Id}", whitelist);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Pilot whitelist'e eklenirken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> RemoveFromPilotWhitelist(
        int whitelistId,
        [FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            var result = await featureFlagService.RemoveFromPilotWhitelistAsync(whitelistId);
            if (!result)
            {
                return Results.NotFound($"ID {whitelistId} ile pilot whitelist kaydı bulunamadı");
            }
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Pilot whitelist'ten çıkarılırken hata oluştu: {ex.Message}");
        }
    }

    private static async Task<IResult> RefreshCache([FromServices] IFeatureFlagService featureFlagService)
    {
        try
        {
            await featureFlagService.RefreshCacheAsync();
            return Results.Ok(new { message = "Cache başarıyla yenilendi" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Cache yenilenirken hata oluştu: {ex.Message}");
        }
    }
}

