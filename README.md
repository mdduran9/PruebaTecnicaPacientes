# 🏥 Patients API

API RESTful construida en ASP.NET Core 8 para la gestión de pacientes.  
Incluye CRUD completo, validaciones, auditoría y exportación CSV.

## 🧰 Tecnologías
- ASP.NET Core 8
- Entity Framework Core
- AutoMapper
- Swagger UI
- SQL Server

## 🚀 Endpoints principales
- `POST /api/v1/patients` → Crear paciente  
- `GET /api/v1/patients` → Listar con filtros y paginación  
- `PUT /api/v1/patients/{id}` → Actualizar  
- `PATCH /api/v1/patients/{id}` → Actualización parcial  
- `DELETE /api/v1/patients/{id}` → Eliminar  
- `GET /api/v1/patients/export` → Exportar CSV

## 🗄️ Configuración de Base de Datos

La API utiliza **SQL Server** como base de datos.

### 🔧 Configuración local
La API utiliza Microsoft SQL Server como sistema gestor de base de datos.

1. Crea una base de datos llamada `Entidades` en tu SQL Server local.
2. Abre el archivo `appsettings.json` y ajusta la cadena de conexión:
   ```json
   "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Entidades;Trusted_Connection=True;TrustServerCertificate=True;"
  },
3. Ejecuta las migraciones de Entity Framework para crear las tablas:
  dotnet ef database update
4. La base de datos generará automáticamente las siguientes tablas:
  Patients
  AuditLogs
5. Entidades principales:
   5.1 Patients: PatientId, DocumentType, DocumentNumber, FirstName, LastName, BirthDate, PhoneNumber, Email, CreatedAt
   5.2 AuditLogs: AuditLogId, Entity, EntityId, Action, Username, CreatedAt, Changes

### ⚠️ Qué **NO** debes incluir
❌ No pongas tu cadena completa si tiene usuario y contraseña, por ejemplo:
```json
"Server=sqlserver.cloud.net;Database=PatientsDb;User Id=admin;Password=1234;"


## 🗄️ Ejecución del proyecto
  
  1. Clona este reporsitorio: git clone https://github.com/tu-usuario/PruebaTecnicaPacientes.git
  2. Instala dependencias: dotnet restore
  3. Ejecuta el proyecto: dotnet run
  4. Abre tu navegador y accede a Swagger UI: https://localhost:7152/swagger

---
Desarrollado por **Moisés Durán**
moises.duran2501@gmail.com
