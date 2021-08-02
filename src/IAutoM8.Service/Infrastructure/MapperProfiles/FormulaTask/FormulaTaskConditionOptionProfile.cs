using AutoMapper;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Service.FormulaTasks.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.FormulaTask
{
    internal class FormulaTaskConditionOptionProfile : Profile
    {
        public FormulaTaskConditionOptionProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<FormulaTaskConditionOptionDto, FormulaTaskConditionOption>(MemberList.None)
                .ForMember(dest => dest.AssignedTaskId, opt =>
                    opt.MapFrom(src => src.AssignedTaskId == 0 ? (int?) null : src.AssignedTaskId))
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }

        private void ToDto()
        {
            CreateMap<FormulaTaskConditionOption, FormulaTaskConditionOptionDto>()
               .ForMember(dest => dest.AssignedTaskId, opt => opt.MapFrom(src => src.AssignedTaskId ?? 0));
        }
    }
}
