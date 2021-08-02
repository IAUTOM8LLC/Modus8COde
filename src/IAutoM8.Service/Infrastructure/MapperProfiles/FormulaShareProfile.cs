using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Service.Formula.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles
{
    internal class FormulaShareProfile : Profile
    {
        public FormulaShareProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<FormulaUserShareDto, FormulaShare>(MemberList.None);
            CreateMap<FormulaShareStatusDto, FormulaProject>()
                .ForMember(dest => dest.IsResharingAllowed, opt => opt.MapFrom(src => src.IsResharingAllowed))
                .ForMember(dest => dest.ShareType, opt => opt.MapFrom(src => src.ShareType))
                .ForAllOtherMembers(opt => opt.Ignore());
        }

        private void ToDto()
        {
            CreateMap<FormulaShare, FormulaUserShareDto>();

            CreateMap<FormulaProject, FormulaShareStatusDto>();
        }
    }
}
