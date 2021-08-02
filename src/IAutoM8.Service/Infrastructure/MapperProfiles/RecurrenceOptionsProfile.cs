using AutoMapper;
using IAutoM8.Domain.Models;
using IAutoM8.Service.CommonDto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles
{
    internal class RecurrenceOptionsProfile : Profile
    {

        public RecurrenceOptionsProfile()
        {
            ToDto();
            FromDto();
            CreateMap<RecurrenceOptions, RecurrenceOptions>()
                .ForMember(dest => dest.Id, opts => opts.Ignore());
        }

        private void ToDto()
        {
            CreateMap<RecurrenceOptions, RecurrenceOptionsDto>();
        }

        private void FromDto()
        {
            CreateMap<RecurrenceOptionsDto, RecurrenceOptions>(MemberList.None)
                .ForMember(dest => dest.Occurrences, src => src.Ignore());
        }
    }
}
