using AutoMapper;
using PatientsApi.Dtos;
using PatientsApi.Entities;

namespace PatientsApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreatePatientDto, Patient>();
            CreateMap<UpdatePatientDto, Patient>();
            CreateMap<Patient, PatientDto>();
        }
    }
}

