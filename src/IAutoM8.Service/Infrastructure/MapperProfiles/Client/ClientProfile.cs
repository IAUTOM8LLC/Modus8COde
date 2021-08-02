using AutoMapper;
using IAutoM8.Service.Client.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.Client
{
    internal class ClientProfile : Profile
    {
        public ClientProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<UpdateClientDto, Domain.Models.Client.Client>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.MobilePhone, opt => opt.MapFrom(src => src.MobilePhone))
                .ForMember(dest => dest.OfficePhone, opt => opt.MapFrom(src => src.OfficePhone))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.StreetAddress1, opt => opt.MapFrom(src => src.StreetAddress1))
                .ForMember(dest => dest.StreetAddress2, opt => opt.MapFrom(src => src.StreetAddress2))
                .ForMember(dest => dest.Zip, opt => opt.MapFrom(src => src.Zip))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForAllOtherMembers(opt => opt.Ignore());
        }

        private void ToDto()
        {
            CreateMap<Domain.Models.Client.Client, UpdateClientDto>();
            CreateMap<Domain.Models.Client.Client, ClientDto>()
                .IncludeBase<Domain.Models.Client.Client, UpdateClientDto>()
                .ForMember(dest => dest.HasAssignedProjects, opt => opt.MapFrom(src => src.Projects.Count > 0));
        }
    }
}
