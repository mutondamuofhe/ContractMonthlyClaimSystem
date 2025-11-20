using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class ClaimRecord
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string LecturerId { get; set; }

    [Required]
    public System.DateTime SubmittedAt { get; set; } = System.DateTime.UtcNow;

    [Required]
    public decimal HoursWorked { get; set; }

    [Required]
    public decimal HourlyRate { get; set; }

    public decimal CalculatedAmount { get; set; } // hours * rate (auto)
    public string Notes { get; set; }

    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    public string StatusUpdatedBy { get; set; }
    public System.DateTime? StatusUpdatedAt { get; set; }

    public string RejectionReason { get; set; }
    public bool DocumentsUploaded { get; set; } // true when doc(s) present
    // optional audit
    public string LastProcessedBy { get; set; }

    public List<ClaimFile> Files { get; set; } = new();
}
