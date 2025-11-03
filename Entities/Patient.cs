using System;
using System.ComponentModel.DataAnnotations;

namespace PatientsApi.Entities
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required, StringLength(10)]
        public string DocumentType { get; set; }

        [Required, StringLength(20)]
        public string DocumentNumber { get; set; }

        [Required, StringLength(80)]
        public string FirstName { get; set; }

        [Required, StringLength(80)]
        public string LastName { get; set; }

        public DateTime BirthDate { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(120)]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
