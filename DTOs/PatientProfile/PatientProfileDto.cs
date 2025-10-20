﻿using BookingApp.DTOs.Specialty;
using BookingApp.Models;

namespace BookingApp.DTOs.PatientProfile
{
    public class PatientProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }
    }
}
