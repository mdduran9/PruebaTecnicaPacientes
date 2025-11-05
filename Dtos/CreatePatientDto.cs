try
{
    _context.AuditLogs.Add(new AuditLog
    {
        Entity = "Patient",
        EntityId = patient.PatientId,
        Action = "CREATE",
        Username = Environment.UserName,
        CreatedAt = DateTime.UtcNow,
        Changes = $"Paciente creado: {patient.FirstName} {patient.LastName}"
    });
    await _context.SaveChangesAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error guardando auditoría: {ex.Message}");
}

