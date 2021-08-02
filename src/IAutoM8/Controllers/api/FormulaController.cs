using IAutoM8.Controllers.Abstract;
using IAutoM8.Global;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IAutoM8.Service.Infrastructure.Extensions;
using System.Linq;
using System;
using System.Collections;

namespace IAutoM8.Controllers.api
{
    [Authorize(Roles = OwnerOrManagerOrAdmin)]
    public class FormulaController : BaseController
    {
        private readonly IFormulaService _formulaService;
        private readonly IFormulaToProjectConverterService _formulaToProjectConverterService;
        private readonly ICategoryService _categoryService;
        private readonly System.Security.Claims.ClaimsPrincipal _principal;

        public FormulaController(IFormulaService formulaService, IFormulaToProjectConverterService formulaToProjectConverterService,
            ICategoryService categoryService, System.Security.Claims.ClaimsPrincipal principal)
        {
            _formulaService = formulaService;
            _formulaToProjectConverterService = formulaToProjectConverterService;
            _categoryService = categoryService;
            _principal = principal;
        }

        [HttpGet]
        public async Task<JsonResult> GetFormulas([FromQuery] SearchFormulaDto search)
        {
            var result = await _formulaService.GetFormulas(search);
            return Json(result);
        }

        [HttpGet]
        [Route("public")]
        public async Task<JsonResult> GetPublicFormulas([FromQuery] SearchFormulaDto search)
        {
            var result = await _formulaService.GetPublicFormulas(search);
            result = await SetTAT(result);
            return Json(result);
        }

        public double GetValueAfterDecimal(int value)
        {

            if (value >= 1 && value < 13)
            {
                return 0;
            }
            else if (value >= 13 && value <= 25)
            {
                return 25;
            }
            else if (value >= 26 && value <= 50)
            {
                return 50;
            }
            else if (value >= 51 && value <= 75)
            {
                return 75;
            }
            else if (value >= 76 && value <= 100)
            {
                return 1;
            }
            return -1;
        }


        [HttpGet]
        [Route("custom")]
        public async Task<JsonResult> GetCustomFormulas([FromQuery] SearchFormulaDto search)
        {
            var result = await _formulaService.GetCustomFormulas(search);

            result = await SetTAT(result);

            return Json(result);
        }


        public async Task<SearchFormulaResultDto<FormulaListingDto>> SetTAT(SearchFormulaResultDto<FormulaListingDto> result)
        {
            var tatData = await _formulaService.GetFormulaMeanTatValue(_principal.GetUserId(), false);

            foreach (var obj in result.Formulas)
            {
                var formulaTat = tatData.Where(x => x.FORMULAID == obj.Id).FirstOrDefault();
                if (formulaTat != null)
                {
                    if (formulaTat.OUTSOURCER_TAT > 0)
                    {
                        TimeSpan tOutsource = TimeSpan.FromMinutes(Convert.ToDouble(formulaTat.OUTSOURCER_TAT));

                        double totalDaysOutsource = Math.Round(Math.Round(Convert.ToDouble(tOutsource.TotalHours)) / 8, 2);

                        string[] valueOutsource = Convert.ToString(totalDaysOutsource).Split('.');

                        if (valueOutsource.Length == 2)
                        {
                            var afterDecimalOutsource = GetValueAfterDecimal(Convert.ToInt32(valueOutsource[1]));

                            var beforDecimalOutsource = valueOutsource[0];

                            if (afterDecimalOutsource > 0 && afterDecimalOutsource != 1)
                            {
                                beforDecimalOutsource += "." + afterDecimalOutsource;
                            }
                            else if (afterDecimalOutsource == 1)
                            {
                                int beforDecimalWithOne = Convert.ToInt32(beforDecimalOutsource) + 1;
                                beforDecimalOutsource = Convert.ToString(beforDecimalWithOne);
                            }

                            result.Formulas.Where(x => x.Id == obj.Id).FirstOrDefault().OUTSOURCER_TAT = Convert.ToDecimal(beforDecimalOutsource);
                        }
                        else
                        {
                            result.Formulas.Where(x => x.Id == obj.Id).FirstOrDefault().OUTSOURCER_TAT = Convert.ToDecimal(valueOutsource[0]);
                        }



                    }
                    else
                    {

                        result.Formulas.Where(x => x.Id == obj.Id).FirstOrDefault().OUTSOURCER_TAT = 0;
                    }


                    if (formulaTat.TOTAL_TAT > 0)
                    {
                        TimeSpan tTotalTAT = TimeSpan.FromMinutes(Convert.ToDouble(formulaTat.TOTAL_TAT));

                        double totalDaysTotalTAT = Math.Round(Math.Round(Convert.ToDouble(tTotalTAT.TotalHours)) / 8, 2);

                        string[] valueTotalTAT = Convert.ToString(totalDaysTotalTAT).Split('.');

                        if (valueTotalTAT.Length == 2)
                        {
                            var afterDecimalTotalTAT = GetValueAfterDecimal(Convert.ToInt32(valueTotalTAT[1]));

                            var beforDecimalTotalTAT = valueTotalTAT[0];

                            if (afterDecimalTotalTAT > 0 && afterDecimalTotalTAT != 1)
                            {
                                beforDecimalTotalTAT += "." + afterDecimalTotalTAT;
                            }
                            else if (afterDecimalTotalTAT == 1)
                            {
                                int beforDecimalWithOneTotalTAT = Convert.ToInt32(beforDecimalTotalTAT) + 1;
                                beforDecimalTotalTAT = Convert.ToString(beforDecimalWithOneTotalTAT);
                            }

                            result.Formulas.Where(x => x.Id == obj.Id).FirstOrDefault().TOTAL_TAT = Convert.ToDecimal(beforDecimalTotalTAT);
                        }
                        else
                        {
                            result.Formulas.Where(x => x.Id == obj.Id).FirstOrDefault().TOTAL_TAT = Convert.ToDecimal(valueTotalTAT[0]);
                        }
                    }
                    else
                    {
                        result.Formulas.Where(x => x.Id == obj.Id).FirstOrDefault().TOTAL_TAT = 0;
                    }
                }
                else
                {
                    result.Formulas.Where(x => x.Id == obj.Id).FirstOrDefault().TOTAL_TAT = 0;
                    result.Formulas.Where(x => x.Id == obj.Id).FirstOrDefault().OUTSOURCER_TAT = 0;
                }

            }

            return result;
        }


        [HttpGet]
        [Route("ownedformulas")]
        public async Task<IActionResult> GetOwnedFormulas()
        {
            var result = await _formulaService.GetOwnedFormulas();
            return Ok(result);
        }

        [HttpGet]
        [Route("allformulas")]
        public async Task<JsonResult> GetAllFormulas([FromQuery] SearchFormulaDto search)
        {
            var result = await _formulaService.GetAllFormulas(search);
            return Json(result);
        }

        [HttpGet]
        [Route("categories")]
        public async Task<JsonResult> GetCategories()
        {
            var result = await _categoryService.GetCategories();
            return Json(result);
        }

        [HttpGet]
        [Route("{formulaId:int}")]
        public async Task<IActionResult> GetFormula([FromRoute] int formulaId)
        {
            var formula = await _formulaService.GetFormula(formulaId);
            return Json(formula);
        }

        [HttpGet]
        [Route("get-skills/{formulaId:int}")]
        public async Task<IActionResult> GetSkills([FromRoute] int formulaId)
        {

            return Json(await _formulaToProjectConverterService.GetSkills(formulaId));
        }

        [HttpGet]
        [Route("get-formula-mean-tat/{formulaId:int}")]
        public async Task<IActionResult> GetFormulaMeanTat([FromRoute] int formulaId)
        {
            var result = await _formulaToProjectConverterService.GetFormulaMeanTat(formulaId);

            AllFormulaMeanTatSingleDto obj = new AllFormulaMeanTatSingleDto();

            obj.FORMULAID = result.FORMULAID;
            obj.ISGLOBAL = result.ISGLOBAL;
            obj.OWNERGUID = result.OWNERGUID;

            if (result.OUTSOURCER_TAT > 0)
            {
                TimeSpan tOutsource = TimeSpan.FromMinutes(Convert.ToDouble(result.OUTSOURCER_TAT));

                double totalDaysOutsource = Math.Round(Math.Round(Convert.ToDouble(tOutsource.TotalHours)) / 8, 2);

                string[] valueOutsource = Convert.ToString(totalDaysOutsource).Split('.');

                if (valueOutsource.Length == 2)
                {
                    var afterDecimalOutsource = GetValueAfterDecimal(Convert.ToInt32(valueOutsource[1]));

                    var beforDecimalOutsource = valueOutsource[0];

                    if (afterDecimalOutsource > 0 && afterDecimalOutsource != 1)
                    {
                        beforDecimalOutsource += "." + afterDecimalOutsource;
                    }
                    else if (afterDecimalOutsource == 1)
                    {
                        int beforDecimalWithOne = Convert.ToInt32(beforDecimalOutsource) + 1;
                        beforDecimalOutsource = Convert.ToString(beforDecimalWithOne);
                    }

                    obj.OUTSOURCER_TAT = Convert.ToDecimal(beforDecimalOutsource);
                }
                else
                {
                    obj.OUTSOURCER_TAT = Convert.ToDecimal(valueOutsource[0]);
                }
            }
            else
            {

                obj.OUTSOURCER_TAT = 0;
            }


            if (result.TOTAL_TAT > 0)
            {
                TimeSpan tTotalTAT = TimeSpan.FromMinutes(Convert.ToDouble(result.TOTAL_TAT));

                double totalDaysTotalTAT = Math.Round(Math.Round(Convert.ToDouble(tTotalTAT.TotalHours)) / 8, 2);

                string[] valueTotalTAT = Convert.ToString(totalDaysTotalTAT).Split('.');

                if (valueTotalTAT.Length == 2)
                {
                    var afterDecimalTotalTAT = GetValueAfterDecimal(Convert.ToInt32(valueTotalTAT[1]));

                    var beforDecimalTotalTAT = valueTotalTAT[0];

                    if (afterDecimalTotalTAT > 0 && afterDecimalTotalTAT != 1)
                    {
                        beforDecimalTotalTAT += "." + afterDecimalTotalTAT;
                    }
                    else if (afterDecimalTotalTAT == 1)
                    {
                        int beforDecimalWithOneTotalTAT = Convert.ToInt32(beforDecimalTotalTAT) + 1;
                        beforDecimalTotalTAT = Convert.ToString(beforDecimalWithOneTotalTAT);
                    }

                    obj.TOTAL_TAT = Convert.ToDecimal(beforDecimalTotalTAT);
                }
                else
                {
                    obj.TOTAL_TAT = Convert.ToDecimal(valueTotalTAT[0]);
                }
            }
            else
            {
                obj.TOTAL_TAT = 0;
            }



            return Ok(obj);
        }

        [HttpPost]
        public async Task<JsonResult> AddFormula([FromBody] AddFormulaDto data)
        {
            var formula = await _formulaService.AddFormula(data);
            return Json(formula);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateFormula([FromBody] FormulaDto data)
        {
            var formula = await _formulaService.UpdateFormula(data);
            return Ok(formula);
        }

        [HttpDelete]
        [Route("{formulaId:int}")]
        public async Task<IActionResult> DeleteFormula([FromRoute] int formulaId)
        {
            await _formulaService.DeleteFormula(formulaId);
            return Ok();
        }

        [HttpPost]
        [Route("{formulaId:int}/project")]
        public async Task<IActionResult> CreateProject([FromRoute] int formulaId, [FromBody] CreateProjectDto createProjectDto)
        {
            var projectId = await _formulaToProjectConverterService.CreateProject(formulaId, createProjectDto, User);
            return Ok(projectId);
        }


        [Authorize(Roles = UserRoles.Owner + "," + UserRoles.Admin)]
        [Route("lock")]
        [HttpPut]
        public async Task<IActionResult> LockUser([FromBody] int formulaId)
        {
            await _formulaService.SetLockStatus(formulaId, true);
            return Ok(formulaId);
        }

        [Authorize(Roles = UserRoles.Owner + "," + UserRoles.Admin)]
        [Route("unlock")]
        [HttpPut]
        public async Task<IActionResult> UnlockUser([FromBody] int formulaId)
        {
            await _formulaService.SetLockStatus(formulaId, false);
            return Ok(formulaId);
        }

        [Route("ChangeFormulaStatus/{formulaId:int}")]
        [HttpGet]
        public async Task<IActionResult> ChangeFormulaStatus(int formulaId)
        {
            var result = await _formulaService.ChangeFormulaStatus(formulaId);
            return Ok(result);
        }

        [HttpPatch]
        [Route("addstar")]
        public async Task<IActionResult> AddStar([FromBody] int formulaId)
        {
            await _formulaService.SetStarredStatus(formulaId, true);
            return Ok(formulaId);
        }

        [HttpPatch]
        [Route("removestar")]
        public async Task<IActionResult> RemoveStar([FromBody] int formulaId)
        {
            await _formulaService.SetStarredStatus(formulaId, false);
            return Ok(formulaId);
        }

        [HttpPost]
        [Route("copyformula")]
        public async Task<IActionResult> CopyFormula([FromBody] FormulaDto formula)
        {
            bool IsAdmin = _principal.IsAdmin();
            var result = await _formulaService.CopyFormula(formula.Id, formula.Name, formula.Description, IsAdmin);
            return Ok(result);
        }
    }
}
