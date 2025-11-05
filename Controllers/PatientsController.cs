using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientsApi.Data;
using PatientsApi.Dtos;
using PatientsApi.Entities;
using System.Globalization;
using System.Text;

namespace PatientsApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PatientsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // POST /api/v1/patients
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.BirthDate > DateTime.UtcNow)
                return BadRequest(new { message = "La fecha de nacimiento no puede ser futura." });

            bool exists = await _context.Patients.AnyAsync(p =>
                p.DocumentType == dto.DocumentType && p.DocumentNumber == dto.DocumentNumber);

            if (exists)
                return Conflict(new { message = "Ya existe un paciente con este tipo y número de documento." });

            var patient = _mapper.Map<Patient>(dto);
            patient.CreatedAt = DateTime.UtcNow;

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Auditoría
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

            var result = _mapper.Map<PatientDto>(patient);
            return CreatedAtAction(nameof(GetById), new { id = patient.PatientId }, result);
        }

        // GET /api/v1/patients
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? documentNumber = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] string sortDir = "desc")
        {
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            if (page <= 0) page = 1;

            IQueryable<Patient> q = _context.Patients.AsNoTracking();

            // Filtros
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameLower = name.ToLower();
                q = q.Where(p => p.FirstName.ToLower().Contains(nameLower) || p.LastName.ToLower().Contains(nameLower));
            }

            if (!string.IsNullOrWhiteSpace(documentNumber))
                q = q.Where(p => p.DocumentNumber == documentNumber.Trim());

            if (createdFrom.HasValue)
                q = q.Where(p => p.CreatedAt >= createdFrom.Value);

            if (createdTo.HasValue)
                q = q.Where(p => p.CreatedAt <= createdTo.Value);

            // Ordenamiento
            bool asc = sortDir.ToLower() == "asc";
            q = sortBy?.ToLower() switch
            {
                "firstname" => asc ? q.OrderBy(p => p.FirstName) : q.OrderByDescending(p => p.FirstName),
                "createdat" => asc ? q.OrderBy(p => p.CreatedAt) : q.OrderByDescending(p => p.CreatedAt),
                _ => asc ? q.OrderBy(p => p.PatientId) : q.OrderByDescending(p => p.PatientId),
            };

            var total = await q.CountAsync();

            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var dtoItems = items.Select(p => _mapper.Map<PatientDto>(p)).ToList();

            var paged = new PagedResult<PatientDto>
            {
                Items = dtoItems,
                Total = total,
                Page = page,
                PageSize = pageSize
            };

            return Ok(paged);
        }

        // GET /api/v1/patients/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound(new { message = "Paciente no encontrado." });
            return Ok(_mapper.Map<PatientDto>(patient));
        }

        // PUT /api/v1/patients/{id} ACTUALIZACION COMPLETA
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.BirthDate > DateTime.UtcNow)
                return BadRequest(new { message = "La fecha de nacimiento no puede ser futura." });

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null) return NotFound();

            bool duplicate = await _context.Patients.AnyAsync(p =>
                p.PatientId != id &&
                p.DocumentType == dto.DocumentType &&
                p.DocumentNumber == dto.DocumentNumber);
            if (duplicate)
                return Conflict(new { message = "Otro paciente con el mismo documento ya existe." });

            // Actualizar datos
            patient.DocumentType = dto.DocumentType;
            patient.DocumentNumber = dto.DocumentNumber;
            patient.FirstName = dto.FirstName;
            patient.LastName = dto.LastName;
            patient.BirthDate = dto.BirthDate;
            patient.PhoneNumber = dto.PhoneNumber;
            patient.Email = dto.Email;

            try
            {
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog
                {
                    Entity = "Patient",
                    EntityId = patient.PatientId,
                    Action = "UPDATE",
                    Username = !string.IsNullOrEmpty(Environment.UserName) ? Environment.UserName : "System",
                    CreatedAt = DateTime.UtcNow,
                    Changes = $"Paciente actualizado: {patient.FirstName} {patient.LastName}"
                });
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Conflicto de concurrencia. El registro fue modificado por otro usuario." });
            }
        }

        // PATCH /api/v1/patients/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<Patient> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null)
                return NotFound();

            patchDoc.ApplyTo(patient, ModelState);

            if (!TryValidateModel(patient))
                return BadRequest(ModelState);

            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = "Patient",
                EntityId = patient.PatientId,
                Action = "PATCH",
                Username = Environment.UserName,
                CreatedAt = DateTime.UtcNow,
                Changes = "Actualización parcial aplicada"
            });
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<PatientDto>(patient));
        }

        // DELETE /api/v1/patients/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = "Patient",
                EntityId = id,
                Action = "DELETE",
                Username = Environment.UserName,
                CreatedAt = DateTime.UtcNow,
                Changes = $"Paciente eliminado: {patient.FirstName} {patient.LastName}"
            });
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/v1/patients/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportCsv([FromQuery] DateTime? createdFrom = null,
                                                   [FromQuery] string? name = null,
                                                   [FromQuery] string? documentNumber = null)
        {
            IQueryable<Patient> q = _context.Patients.AsNoTracking();

            if (createdFrom.HasValue)
                q = q.Where(p => p.CreatedAt >= createdFrom.Value);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var nl = name.ToLower();
                q = q.Where(p => p.FirstName.ToLower().Contains(nl) || p.LastName.ToLower().Contains(nl));
            }

            if (!string.IsNullOrWhiteSpace(documentNumber))
                q = q.Where(p => p.DocumentNumber == documentNumber.Trim());

            var list = await q.OrderBy(p => p.PatientId).ToListAsync();

            // Construcción del CSV
            using var mem = new MemoryStream();
            using var writer = new StreamWriter(mem, Encoding.UTF8, leaveOpen: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteField("PatientId");
            csv.WriteField("DocumentType");
            csv.WriteField("DocumentNumber");
            csv.WriteField("FirstName");
            csv.WriteField("LastName");
            csv.WriteField("BirthDate");
            csv.WriteField("PhoneNumber");
            csv.WriteField("Email");
            csv.WriteField("CreatedAt");
            csv.NextRecord();

            foreach (var p in list)
            {
                csv.WriteField(p.PatientId);
                csv.WriteField(p.DocumentType);
                csv.WriteField(p.DocumentNumber);
                csv.WriteField(p.FirstName);
                csv.WriteField(p.LastName);
                csv.WriteField(p.BirthDate.ToString("yyyy-MM-dd"));
                csv.WriteField(p.PhoneNumber);
                csv.WriteField(p.Email);
                csv.WriteField(p.CreatedAt.ToString("o"));
                csv.NextRecord();
            }

            await writer.FlushAsync();
            mem.Position = 0;

            return File(mem.ToArray(), "text/csv", $"patients_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }
}


