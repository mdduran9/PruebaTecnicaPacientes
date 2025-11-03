using System;

namespace PatientsApi.Dtos
{
    public class PatientDto
    {
        public int PatientId { get; set; }
        public string DocumentType { get; set; } = null!;
        public string DocumentNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RowVersion { get; set; } = null!; // base64
    }
}
