using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.Formula.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.ShareSubFormulas
{
    public class Worker : IWorker
    {
        private readonly IRepo _repo;
        private readonly IFormulaShareService _formulaShareService;
        private readonly IFormulaNeo4jRepository _formulaNeo4JRepository;
        private readonly IServiceCollection _services;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public Worker(
            IRepo repo,
            IFormulaShareService formulaShareService,
            IFormulaNeo4jRepository formulaNeo4JRepository,
            IServiceCollection services,
            ClaimsPrincipal claimsPrincipal)
        {
            _repo = repo;
            _formulaShareService = formulaShareService;
            _formulaNeo4JRepository = formulaNeo4JRepository;
            _services = services;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task Do()
        {
            var list = (await _repo.Read<FormulaProject>()
                .Include(i => i.FormulaTasks)
                .Where(w => w.OriginalFormulaProjectId.HasValue)
                .SelectMany(smf => smf.FormulaTasks
                    .Where(w => w.InternalFormulaProjectId.HasValue)
                    .Select(s => new { formulaId = s.InternalFormulaProjectId.Value, OwnerGuid = smf.OwnerGuid }))
                .ToListAsync())
                    .GroupBy(g => new { g.OwnerGuid, g.formulaId })
                    .Select(s => new { s.Key.formulaId, s.Key.OwnerGuid })
                    .GroupBy(g => g.OwnerGuid)
                    .Select(s => new { OwnerGuid = s.Key, FormulaIds = s.Select(sf => sf.formulaId).ToList() })
                    .ToList();
            foreach (var sharedSubformula in list)
            {
                var identity = (ClaimsIdentity)_claimsPrincipal.Identity;
                identity.RemoveClaim(identity.Claims.First());
                identity.AddClaim(new Claim(ClaimTypes.PrimarySid, sharedSubformula.OwnerGuid.ToString()));
                foreach (var sharedSubformulaId in sharedSubformula.FormulaIds)
                {
                    var originalFormula = await _repo.Read<FormulaProject>()
                        .FirstOrDefaultAsync(w => (w.OriginalFormulaProjectId == sharedSubformulaId && w.OwnerGuid == sharedSubformula.OwnerGuid));
                    if (!await _repo.Read<FormulaProject>()
                        .AnyAsync(w => w.Id == sharedSubformulaId && w.OwnerGuid == sharedSubformula.OwnerGuid))
                    {
                        if (originalFormula == null)
                        {
                            await _formulaShareService.CopyFormulaToUser(sharedSubformulaId, sharedSubformula.OwnerGuid);
                            originalFormula = await _repo.Read<FormulaProject>()
                            .FirstOrDefaultAsync(w => w.OriginalFormulaProjectId == sharedSubformulaId && w.OwnerGuid == sharedSubformula.OwnerGuid);
                        }
                        using (var trx = _repo.Transaction())
                        {
                            var oldFormulaTasks = await trx.Track<FormulaTask>()
                                .Include(i => i.FormulaProject)
                                .Where(w => w.InternalFormulaProjectId == sharedSubformulaId && w.FormulaProject.OwnerGuid == sharedSubformula.OwnerGuid)
                                .ToListAsync();
                            foreach (var subFormulaTask in oldFormulaTasks)
                            {
                                subFormulaTask.InternalFormulaProjectId = originalFormula.Id;
                                await _formulaNeo4JRepository.AddRelationAsync(subFormulaTask.FormulaProject.Id, originalFormula.Id);
                            }
                            await trx.SaveAndCommitAsync();
                        }
                    }
                }
            }
        }
    }
}
