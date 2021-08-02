using AutoMapper;
using IAutoM8.Domain.Models.User;
using IAutoM8.Service.Users.Dto;
using System.Linq;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.User
{
    internal class UserProfile : Profile
    {
        public UserProfile()
        {
            ToDto();
            FromDto();
        }

        private void ToDto()
        {
            CreateMap<Domain.Models.User.User, CompanyUserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Profile.FullName))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(r => r.Role.Name).ToList()));

            CreateMap<Domain.Models.User.User, OwnerDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opts => opts.MapFrom(u => u.Profile != null ? u.Profile.FullName : null));
        }

        private void FromDto()
        {
            CreateMap<SignUpDto, Domain.Models.User.User>(MemberList.None);
            CreateMap<InfusionSignUpDto, InfusionSignUp>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ContactId, opt => opt.Ignore());
        }
    }
}
