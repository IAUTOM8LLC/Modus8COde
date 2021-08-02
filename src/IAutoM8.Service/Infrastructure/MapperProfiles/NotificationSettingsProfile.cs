using AutoMapper;
using IAutoM8.Domain.Models;
using IAutoM8.Service.Notification.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles
{
    public class NotificationSettingsProfile: Profile
    {
        public NotificationSettingsProfile()
        {
            ToDto();
            FromDto();
        }

        private void ToDto()
        {
            CreateMap<NotificationSetting, NotificationSettingDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
        }

        private void FromDto()
        {
            CreateMap<NotificationSettingDto, NotificationSetting>()
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
