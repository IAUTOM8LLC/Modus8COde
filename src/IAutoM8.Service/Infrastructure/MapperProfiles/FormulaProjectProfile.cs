using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Service.Formula.Dto;
using System.Linq;

namespace IAutoM8.Service.Infrastructure.MapperProfiles
{
    internal class FormulaProjectProfile : Profile
    {
        public FormulaProjectProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<AddFormulaDto, FormulaProject>(MemberList.None)
                .ForMember(dest => dest.FormulaProjectCategories,
                opt => opt.MapFrom(src => src.CategoryIds.Select(s => new FormulaProjectCategory
                {
                    CategoryId = s
                }).ToList()));

            CreateMap<FormulaDto, FormulaProject>(MemberList.None)
                .ForMember(dest => dest.ShareType, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.FormulaProjectCategories, opt => opt.Ignore()); //TODO:  Fix this
        }

        private void ToDto()
        {
            CreateMap<FormulaProject, FormulaListingDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.IsGlobal ? "public" : "custom"))
                .ForMember(dest => dest.TasksNumber, opt => opt.MapFrom(dest => dest.FormulaTasks.Count()))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(dest => dest.FormulaProjectCategories.Select(s => s.Category.Name)))
                 .ForMember(dest => dest.OUTSOURCER_TAT, opt => opt.Ignore())
                 .ForMember(dest => dest.TOTAL_TAT, opt => opt.Ignore());

            CreateMap<FormulaProject, FormulaDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.IsGlobal ? "public" : "custom"))
                .ForMember(dest => dest.Resources, opt => opt.MapFrom(src => src.ResourceFormula.Select(s => s.Resource)))
                .ForMember(dest => dest.FormulaShareStatus, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.IsSharingAllowedByOriginal,
                        opt => opt.MapFrom(src => src.OriginalFormulaProject != null && src.OriginalFormulaProject.IsResharingAllowed))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(dest => dest.Owner))
                .ForMember(dest => dest.TasksNumber, opt => opt.MapFrom(dest => dest.FormulaTasks.Count))
                .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(dest => dest.FormulaProjectCategories.Select(s => s.CategoryId)));
        }
    }
}
