using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Repository;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IAutoM8.Service.Formula
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepo _repo;
        private readonly IMapper _mapper;

        public CategoryService(
            IRepo repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetCategories()
        {
            return await _repo.Read<Category>()
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
