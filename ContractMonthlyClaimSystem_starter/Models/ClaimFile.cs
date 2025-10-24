using System.ComponentModel.DataAnnotations;

public class ClaimFile
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int ClaimRecordId { get; set; }
    public ClaimRecord ClaimRecord { get; set; }

    [Required]
    public string FileName { get; set; }

    [Required]
    public string StoredFilePath { get; set; }
    public long Size { get; set; }
}
