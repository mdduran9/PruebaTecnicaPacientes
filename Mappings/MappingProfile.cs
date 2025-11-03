using AutoMapper;
using PatientsApi.Dtos;
using PatientsApi.Entities;
using System;

namespace PatientsApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreatePatientDto, Patient>();
            CreateMap<UpdatePatientDto, Patient>()
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore()); // rowversion la manejamos manualmente

            CreateMap<Patient, PatientDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .ForMember(d => d.RowVersion, o => o.MapFrom(s => Convert.ToBase64String(s.RowVersion)));
        }
    }
}

