using System.ComponentModel.DataAnnotations;

namespace FeatureFlagSystem.Domain.Entities;

public class PilotWhitelist
{
    public int Id { get; set; }
    
    public int FeatureFlagId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string UserIdentifier { get; set; } = string.Empty; // Müşteri numarası, kullanıcı adı vb.
    
    [MaxLength(50)]
    public string? UserType { get; set; } // "Customer", "Employee", "Admin" vb.
    
    [MaxLength(20)]
    public string? MinVersion { get; set; } // Minimum uygulama versiyonu
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; } // Pilot süresi bitiş tarihi
    
    [MaxLength(50)]
    public string? CreatedBy { get; set; }
    
    // Navigation property
    public virtual FeatureFlag FeatureFlag { get; set; } = null!;
}
