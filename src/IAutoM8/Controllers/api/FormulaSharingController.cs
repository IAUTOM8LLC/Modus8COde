using IAutoM8.Controllers.Abstract;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Route("api/formula-sharing")]
    [Authorize(Roles = OwnerOrManager)]
    public class FormulaSharingController : BaseController
    {
        private readonly IFormulaShareService _formulaShareService;

        public FormulaSharingController(IFormulaShareService formulaShareService)
        {
            _formulaShareService = formulaShareService;
        }

        [HttpPost]
        public async Task<IActionResult> GetUniqueFormulas([FromBody] IList<int> formulaIds)
        {
            return Ok(await _formulaShareService.GetUniqueFormulas(formulaIds));
        }

        [HttpGet]
        [Route("{formulaId:int}")]
        public async Task<IActionResult> CopyFormula([FromRoute] int formulaId)
        {
            var validationMessages = await _formulaShareService.CheckFormulaAndSubformulasForSharing(formulaId);

            if(validationMessages.Count != 0)
            {
                return BadRequest(String.Join(Environment.NewLine, validationMessages.ToArray()));
            }

            await _formulaShareService.CopyFormulaToUser(formulaId, UserGuid);
            return Ok();
        }

        [HttpGet]
        [Route("{formulaId:int}/status")]
        public async Task<IActionResult> ShareStatus([FromRoute] int formulaId)
        {
            try
            {
                return Json(await _formulaShareService.GetFormulaShareStatus(formulaId) == FormulaShareType.PublicLink);
            }
            catch (Exception)
            {
                return Forbid("Cannot reshare formula");
            }
        }

        [HttpPut]
        [Route("{formulaId:int}/status")]
        public async Task<IActionResult> UpdateShareStatus(
            [FromRoute] int formulaId,
            [FromBody] FormulaShareStatusDto shareStatus)
        {
            return Ok(await _formulaShareService.UpdateFormulaShareStatus(formulaId, shareStatus));
        }

        #region User Based Sharing

        [HttpGet]
        [Route("{formulaId:int}/list")]
        public async Task<IActionResult> GetFormulaHolders([FromRoute] int formulaId)
        {
            var result = await _formulaShareService.GetFormulaShareList(formulaId);
            return Ok(result.Select(x => x.UserId));
        }

        [HttpPost]
        [Route("{formulaId:int}/list")]
        public async Task<IActionResult> ShareFormulaWithUser([FromRoute] int formulaId, Guid userId)
        {
            await _formulaShareService.ShareFormulaToUser(new FormulaUserShareDto
            {
                FormulaProjectId = formulaId,
                UserId = userId
            });
            return Ok();
        }

        [HttpDelete]
        [Route("{formulaId:int}/list/{userId:guid}")]
        public async Task<IActionResult> LockAccessForUser([FromRoute] int formulaId, [FromRoute] Guid userId)
        {
            await _formulaShareService.RemoveUserFromShareList(new FormulaUserShareDto
            {
                FormulaProjectId = formulaId,
                UserId = userId
            });
            return Ok();
        }

        #endregion
    }
}
