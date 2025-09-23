using BookingApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MedicalRecord
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("PatientProfile")]
    public int PatientId { get; set; }
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public PatientProfile PatientProfile { get; set; } = null!;
}