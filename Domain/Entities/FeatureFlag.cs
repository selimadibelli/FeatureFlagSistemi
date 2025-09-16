using System.ComponentModel.DataAnnotations;

namespace FeatureFlagSystem.Domain.Entities;

public class FeatureFlag
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsEnabled { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [MaxLength(50)]
    public string? CreatedBy { get; set; }
    
    [MaxLength(50)]
    public string? UpdatedBy { get; set; }
    
    // Navigation property
    public virtual ICollection<PilotWhitelist> PilotWhitelists { get; set; } = new List<PilotWhitelist>();
}
