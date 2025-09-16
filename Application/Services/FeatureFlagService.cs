using Microsoft.EntityFrameworkCore;
using FeatureFlagSystem.Domain.Entities;
using FeatureFlagSystem.Domain.DTOs;
using FeatureFlagSystem.Infrastructure.Data;
using FeatureFlagSystem.Infrastructure.Services;

namespace FeatureFlagSystem.Application.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly FeatureFlagDbContext _context;
    private readonly ICacheService _cacheService;
    private const string FeatureFlagCacheKey = "feature_flag:";
    private const string AllFeatureFlagsCacheKey = "all_feature_flags";
    private const int CacheExpirationMinutes = 30;

    public FeatureFlagService(FeatureFlagDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<FeatureFlagCheckResultDto> CheckFeatureFlagAsync(FeatureFlagCheckDto request)
    {
        var cacheKey = $"{FeatureFlagCacheKey}{request.FeatureName}";
        
        // Önce cache'den kontrol et
        var cachedFeature = await _cacheService.GetAsync<FeatureFlagDto>(cacheKey);
        
        if (cachedFeature == null)
        {
            // Cache'de yoksa veritabanından oku ve cache'e ekle
            var feature = await _context.FeatureFlags
                .Include(f => f.PilotWhitelists)
                .FirstOrDefaultAsync(f => f.Name == request.FeatureName);

            if (feature == null)
            {
                return new FeatureFlagCheckResultDto
                {
                    IsEnabled = false,
                    IsInPilot = false,
                    Reason = "Feature flag bulunamadı"
                };
            }

            cachedFeature = MapToDto(feature);
            await _cacheService.SetAsync(cacheKey, cachedFeature, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }

        // Feature flag kapalıysa
        if (!cachedFeature.IsEnabled)
        {
            return new FeatureFlagCheckResultDto
            {
                IsEnabled = false,
                IsInPilot = false,
                Reason = "Feature flag devre dışı"
            };
        }

        // Pilot whitelist kontrolü
        var pilotEntry = cachedFeature.PilotWhitelists.FirstOrDefault(p => 
            p.UserIdentifier == request.UserIdentifier &&
            (string.IsNullOrEmpty(p.UserType) || p.UserType == request.UserType) &&
            (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow));

        if (pilotEntry != null)
        {
            // Minimum versiyon kontrolü
            if (!string.IsNullOrEmpty(pilotEntry.MinVersion) && !string.IsNullOrEmpty(request.AppVersion))
            {
                if (!IsVersionGreaterOrEqual(request.AppVersion, pilotEntry.MinVersion))
                {
                    return new FeatureFlagCheckResultDto
                    {
                        IsEnabled = false,
                        IsInPilot = false,
                        Reason = $"Minimum versiyon gereksinimi: {pilotEntry.MinVersion}"
                    };
                }
            }

            return new FeatureFlagCheckResultDto
            {
                IsEnabled = true,
                IsInPilot = true,
                Reason = "Pilot kullanıcısı"
            };
        }

        return new FeatureFlagCheckResultDto
        {
            IsEnabled = false,
            IsInPilot = false,
            Reason = "Pilot whitelist'te değil"
        };
    }

    public async Task<List<FeatureFlagDto>> GetAllFeatureFlagsAsync()
    {
        var cachedFlags = await _cacheService.GetAsync<List<FeatureFlagDto>>(AllFeatureFlagsCacheKey);
        
        if (cachedFlags != null)
            return cachedFlags;

        var features = await _context.FeatureFlags
            .Include(f => f.PilotWhitelists)
            .OrderBy(f => f.Name)
            .ToListAsync();

        var result = features.Select(MapToDto).ToList();
        await _cacheService.SetAsync(AllFeatureFlagsCacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    public async Task<FeatureFlagDto?> GetFeatureFlagByIdAsync(int id)
    {
        var cacheKey = $"{FeatureFlagCacheKey}id:{id}";
        var cachedFeature = await _cacheService.GetAsync<FeatureFlagDto>(cacheKey);
        
        if (cachedFeature != null)
            return cachedFeature;

        var feature = await _context.FeatureFlags
            .Include(f => f.PilotWhitelists)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (feature == null)
            return null;

        var result = MapToDto(feature);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    public async Task<FeatureFlagDto?> GetFeatureFlagByNameAsync(string name)
    {
        var cacheKey = $"{FeatureFlagCacheKey}{name}";
        var cachedFeature = await _cacheService.GetAsync<FeatureFlagDto>(cacheKey);
        
        if (cachedFeature != null)
            return cachedFeature;

        var feature = await _context.FeatureFlags
            .Include(f => f.PilotWhitelists)
            .FirstOrDefaultAsync(f => f.Name == name);

        if (feature == null)
            return null;

        var result = MapToDto(feature);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    public async Task<FeatureFlagDto> CreateFeatureFlagAsync(CreateFeatureFlagDto dto)
    {
        var feature = new FeatureFlag
        {
            Name = dto.Name,
            Description = dto.Description,
            IsEnabled = dto.IsEnabled,
            CreatedBy = dto.CreatedBy
        };

        _context.FeatureFlags.Add(feature);
        await _context.SaveChangesAsync();

        // Cache'i temizle
        await InvalidateCacheAsync();

        return MapToDto(feature);
    }

    public async Task<FeatureFlagDto?> UpdateFeatureFlagAsync(int id, UpdateFeatureFlagDto dto)
    {
        var feature = await _context.FeatureFlags.FindAsync(id);
        if (feature == null)
            return null;

        feature.Description = dto.Description;
        feature.IsEnabled = dto.IsEnabled;
        feature.UpdatedBy = dto.UpdatedBy;
        feature.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Cache'i temizle
        await InvalidateCacheAsync();

        return await GetFeatureFlagByIdAsync(id);
    }

    public async Task<bool> DeleteFeatureFlagAsync(int id)
    {
        var feature = await _context.FeatureFlags.FindAsync(id);
        if (feature == null)
            return false;

        _context.FeatureFlags.Remove(feature);
        await _context.SaveChangesAsync();

        // Cache'i temizle
        await InvalidateCacheAsync();

        return true;
    }

    public async Task<List<PilotWhitelistDto>> GetPilotWhitelistsAsync(int featureFlagId)
    {
        var whitelists = await _context.PilotWhitelists
            .Where(p => p.FeatureFlagId == featureFlagId)
            .OrderBy(p => p.UserIdentifier)
            .ToListAsync();

        return whitelists.Select(MapToDto).ToList();
    }

    public async Task<PilotWhitelistDto> AddToPilotWhitelistAsync(CreatePilotWhitelistDto dto)
    {
        var whitelist = new PilotWhitelist
        {
            FeatureFlagId = dto.FeatureFlagId,
            UserIdentifier = dto.UserIdentifier,
            UserType = dto.UserType,
            MinVersion = dto.MinVersion,
            ExpiresAt = dto.ExpiresAt,
            CreatedBy = dto.CreatedBy
        };

        _context.PilotWhitelists.Add(whitelist);
        await _context.SaveChangesAsync();

        // Cache'i temizle
        await InvalidateCacheAsync();

        return MapToDto(whitelist);
    }

    public async Task<bool> RemoveFromPilotWhitelistAsync(int id)
    {
        var whitelist = await _context.PilotWhitelists.FindAsync(id);
        if (whitelist == null)
            return false;

        _context.PilotWhitelists.Remove(whitelist);
        await _context.SaveChangesAsync();

        // Cache'i temizle
        await InvalidateCacheAsync();

        return true;
    }

    public async Task RefreshCacheAsync()
    {
        await InvalidateCacheAsync();
        
        // Cache'i yeniden doldur
        await GetAllFeatureFlagsAsync();
    }

    private async Task InvalidateCacheAsync()
    {
        await _cacheService.RemoveByPatternAsync($"{FeatureFlagCacheKey}*");
        await _cacheService.RemoveAsync(AllFeatureFlagsCacheKey);
    }

    private static FeatureFlagDto MapToDto(FeatureFlag feature)
    {
        return new FeatureFlagDto
        {
            Id = feature.Id,
            Name = feature.Name,
            Description = feature.Description,
            IsEnabled = feature.IsEnabled,
            CreatedAt = feature.CreatedAt,
            UpdatedAt = feature.UpdatedAt,
            CreatedBy = feature.CreatedBy,
            UpdatedBy = feature.UpdatedBy,
            PilotWhitelists = feature.PilotWhitelists.Select(MapToDto).ToList()
        };
    }

    private static PilotWhitelistDto MapToDto(PilotWhitelist whitelist)
    {
        return new PilotWhitelistDto
        {
            Id = whitelist.Id,
            FeatureFlagId = whitelist.FeatureFlagId,
            UserIdentifier = whitelist.UserIdentifier,
            UserType = whitelist.UserType,
            MinVersion = whitelist.MinVersion,
            CreatedAt = whitelist.CreatedAt,
            ExpiresAt = whitelist.ExpiresAt,
            CreatedBy = whitelist.CreatedBy
        };
    }

    private static bool IsVersionGreaterOrEqual(string currentVersion, string minVersion)
    {
        try
        {
            var current = new Version(currentVersion);
            var minimum = new Version(minVersion);
            return current >= minimum;
        }
        catch
        {
            return false;
        }
    }
}
