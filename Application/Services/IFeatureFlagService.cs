using FeatureFlagSystem.Domain.DTOs;

namespace FeatureFlagSystem.Application.Services;

public interface IFeatureFlagService
{
    Task<FeatureFlagCheckResultDto> CheckFeatureFlagAsync(FeatureFlagCheckDto request);
    Task<List<FeatureFlagDto>> GetAllFeatureFlagsAsync();
    Task<FeatureFlagDto?> GetFeatureFlagByIdAsync(int id);
    Task<FeatureFlagDto?> GetFeatureFlagByNameAsync(string name);
    Task<FeatureFlagDto> CreateFeatureFlagAsync(CreateFeatureFlagDto dto);
    Task<FeatureFlagDto?> UpdateFeatureFlagAsync(int id, UpdateFeatureFlagDto dto);
    Task<bool> DeleteFeatureFlagAsync(int id);
    Task<List<PilotWhitelistDto>> GetPilotWhitelistsAsync(int featureFlagId);
    Task<PilotWhitelistDto> AddToPilotWhitelistAsync(CreatePilotWhitelistDto dto);
    Task<bool> RemoveFromPilotWhitelistAsync(int id);
    Task RefreshCacheAsync();
}
