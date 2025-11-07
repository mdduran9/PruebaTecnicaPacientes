using System;
using System.ComponentModel.DataAnnotations;

namespace PatientsApi.Dtos
{
    public class CreatePatientDto
    {
        [Required, StringLength(10)]
        public string DocumentType { get; set; } = null!;

        [Required, StringLength(20)]
        public string DocumentNumber { get; set; } = null!;

        [Required, StringLength(80)]
        public string FirstName { get; set; } = null!;

        [Required, StringLength(80)]
        public string LastName { get; set; } = null!;

        [Required]
        public DateTime BirthDate { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress, StringLength(120)]
        public string? Email { get; set; }
    }
}

