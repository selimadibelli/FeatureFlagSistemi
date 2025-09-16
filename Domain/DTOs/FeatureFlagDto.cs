namespace FeatureFlagSystem.Domain.DTOs;

public class FeatureFlagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public List<PilotWhitelistDto> PilotWhitelists { get; set; } = new();
}

public class CreateFeatureFlagDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public string? CreatedBy { get; set; }
}

public class UpdateFeatureFlagDto
{
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public string? UpdatedBy { get; set; }
}

public class PilotWhitelistDto
{
    public int Id { get; set; }
    public int FeatureFlagId { get; set; }
    public string UserIdentifier { get; set; } = string.Empty;
    public string? UserType { get; set; }
    public string? MinVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreatePilotWhitelistDto
{
    public int FeatureFlagId { get; set; }
    public string UserIdentifier { get; set; } = string.Empty;
    public string? UserType { get; set; }
    public string? MinVersion { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class FeatureFlagCheckDto
{
    public string FeatureName { get; set; } = string.Empty;
    public string UserIdentifier { get; set; } = string.Empty;
    public string? UserType { get; set; }
    public string? AppVersion { get; set; }
}

public class FeatureFlagCheckResultDto
{
    public bool IsEnabled { get; set; }
    public bool IsInPilot { get; set; }
    public string? Reason { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
