using BookingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BookingApp.DTOs.PatientProfile
{
    public class PatientProfileUpdateDto
    {
        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required, StringLength(255)]
        public string Address { get; set; } = string.Empty;
    }
}
