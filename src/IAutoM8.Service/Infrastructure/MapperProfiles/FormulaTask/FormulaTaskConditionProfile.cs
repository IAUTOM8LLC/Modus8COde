using AutoMapper;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Service.FormulaTasks.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.FormulaTask
{
    internal class FormulaTaskConditionProfile : Profile
    {
        public FormulaTaskConditionProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<FormulaTaskConditionDto, FormulaTaskCondition>()

                // Ignore
                .ForMember(dest => dest.Id, opts => opts.Ignore())
                .ForMember(dest => dest.Task, opts => opts.Ignore());
        }

        private void ToDto()
        {
            CreateMap<FormulaTaskCondition, FormulaTaskConditionDto>();
        }
    }
}
