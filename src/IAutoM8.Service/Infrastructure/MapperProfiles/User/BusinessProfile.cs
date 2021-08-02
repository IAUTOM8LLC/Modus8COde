using AutoMapper;
using IAutoM8.Domain.Models.Business;
using IAutoM8.Service.Users.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.User
{
    internal class BusinessProfile : Profile
    {
        public BusinessProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<BusinessProfileDto, Business>()

                // Ignore
                .ForMember(dest => dest.UserId, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore())
                .ForMember(dest => dest.NotificationSettings, src => src.Ignore());
        }

        private void ToDto()
        {
            CreateMap<Business, BusinessProfileDto>(MemberList.None);

            CreateMap<Domain.Models.User.UserProfile, BusinessProfileDto>(MemberList.None)
                .ForMember(dest => dest.AffCode, opt => opt.MapFrom(x => x.AffiliateCode))
                .ForMember(dest => dest.AffPass, opt => opt.MapFrom(x => x.AffiliatePass))
                .ForMember(dest => dest.GoldAffUrl, opt => opt
                    .ResolveUsing((source, dest, member, context) =>
                    {
                        return string.Format((string)context.Items["GoldAffUrl"], source.AffiliateId);
                    }))
                .ForMember(dest => dest.SilverAffUrl, opt => opt
                    .ResolveUsing((source, dest, member, context) =>
                    {
                        return string.Format((string)context.Items["SilverAffUrl"], source.AffiliateId);
                    }))
                .ForMember(dest => dest.AffLoginUrl, opt => opt
                    .ResolveUsing((source, dest, member, context) =>
                    {
                        return (string)context.Items["AffLoginUrl"];
                    }));
        }
    }
}
