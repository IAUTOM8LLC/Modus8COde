using IAutoM8.Service.Formula.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.Formula.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategories();
    }
}
