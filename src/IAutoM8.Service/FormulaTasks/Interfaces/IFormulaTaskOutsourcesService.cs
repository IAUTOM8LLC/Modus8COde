using IAutoM8.Service.FormulaTasks.Dto;
using System.Threading.Tasks;
using IAutoM8.Service.CommonDto;
using System.Collections.Generic;

namespace IAutoM8.Service.FormulaTasks.Interfaces
{
    public interface IFormulaTaskOutsourcesService
    {
        Task<IList<FormulaTaskOutsourceDto>> GetOutsources(int id, short skip, byte count);
        Task CreateRequest(OutsourceRequestDto requestDto);
    }
}
