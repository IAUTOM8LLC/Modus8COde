using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IAutoM8.Service.Formula.Dto;

namespace IAutoM8.Service.Formula.Interfaces
{
    public interface IFormulaToProjectConverterService
    {
        Task<int> CreateProject(int formulaId, CreateProjectDto createProjectDto, ClaimsPrincipal user);
        Task<SkillImportDto> GetSkills(int formulaId);
        //Task<string> GetFormulaMeanTat(int formulaId);
        Task<AllFormulaMeanTatDto> GetFormulaMeanTat(int formulaId);
    }
}
