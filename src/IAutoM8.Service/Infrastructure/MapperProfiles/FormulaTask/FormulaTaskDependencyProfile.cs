using AutoMapper;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Service.FormulaTasks.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.FormulaTask
{
    internal class FormulaTaskDependencyProfile : Profile
    {
        public FormulaTaskDependencyProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<FormulaTaskDependencyDto, FormulaTaskDependency>()

                // Ignore
                .ForMember(dest => dest.ChildTask, opts => opts.Ignore())
                .ForMember(dest => dest.ParentTask, opts => opts.Ignore());
        }

        private void ToDto()
        {
            CreateMap<FormulaTaskDependency, FormulaTaskDependencyDto>();
        }
    }
}
