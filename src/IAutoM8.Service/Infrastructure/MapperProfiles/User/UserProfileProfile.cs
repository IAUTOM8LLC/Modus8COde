using AutoMapper;
using IAutoM8.Service.Users.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.User
{
    internal class UserProfileProfile : Profile
    {
        public UserProfileProfile()
        {
            ToDto();
            FromDto();
        }

        private void ToDto()
        {
            CreateMap<Domain.Models.User.UserProfile, UserProfileDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(x => x.User.Email))
                .ForMember(dest => dest.ProfileImage, opt => opt.Ignore());

            CreateMap<Domain.Models.User.UserProfile, UserDropdownItemDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<InfusionSoft.Dto.AffiliateDto, Domain.Models.User.UserProfile>(MemberList.None)
                .ForMember(dest => dest.AffiliateCode, opt => opt.MapFrom(src => src.AffCode))
                .ForMember(dest => dest.AffiliateId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AffiliatePass, opt => opt.MapFrom(src => src.Password));
        }

        private void FromDto()
        {
            CreateMap<UserProfileDto, Domain.Models.User.UserProfile>()

                // Ignore
                .ForMember(dest => dest.User, opts => opts.Ignore())
                .ForMember(dest => dest.UserId, opts => opts.Ignore())
                .ForMember(dest => dest.AffiliateCode, opts => opts.Ignore())
                .ForMember(dest => dest.AffiliateId, opts => opts.Ignore())
                .ForMember(dest => dest.AffiliatePass, opts => opts.Ignore())
                .ForMember(dest => dest.ContactId, opts => opts.Ignore())
                .ForMember(dest => dest.Path, opts => opts.Ignore())
                .ForMember(dest => dest.CompanyWorkerOwnerID, opts => opts.Ignore());
        }
    }
}
