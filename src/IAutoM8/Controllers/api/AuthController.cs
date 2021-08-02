using IAutoM8.Controllers.Abstract;
using IAutoM8.Global;
using IAutoM8.Infrastructure.JWT;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IAutoM8.Service.CommonService.Interfaces;

namespace IAutoM8.Controllers.api
{
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IUserRolePermissionsService _permissionsService;

        public AuthController(IUserService userService,
            ITokenProvider tokenProvider,
            IUserRolePermissionsService permissionsService)
        {
            _userService = userService;
            _tokenProvider = tokenProvider;
            _permissionsService = permissionsService;
        }

        [Route("signup")]
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto data)
        {
            await _userService.SignUp(data, new[] { UserRoles.Owner });
            return Ok();
        }

        [Route("saveInfusionUrl")]
        [HttpPost]
        public async Task<IActionResult> SaveInfusionUrl([FromBody] InfusionSignUpDto data)
        {
            await _userService.SaveSignUpInfusionData(data);
            return Ok();
        }

        [Route("vendor-signup")]
        [HttpPost]
        public async Task<IActionResult> VendorSignUp([FromBody] SignUpDto data)
        {
            await _userService.SignUp(data, new[] { UserRoles.Vendor });
            return Ok();
        }

        [Route("cvendor-signup")]
        [HttpPost]
        public async Task<IActionResult> CVendorSignUp([FromBody] SignUpDto data)
        {
            await _userService.SignUp(data, new[] { UserRoles.CompanyWorker });
            return Ok();
        }

        [Route("signin")]
        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody] SignInDto data)
        {
            var user = await _userService.SignIn(data);
            return Json(_tokenProvider.GenerateToken(user, data.Remember));
        }

        [Authorize]
        [Route("refresh-token")]
        [HttpGet]
        public async Task<object> RefreshToken()
        {
            var user = await _userService.RefreshUser();
            return Json(_tokenProvider.GenerateToken(user));
        }

        [Authorize]
        [HttpGet("permissions")]
        public IActionResult GetPermissions()
        {
            return Ok(_permissionsService.GetPermissions());
        }
    }
}
