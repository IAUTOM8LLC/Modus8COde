using IAutoM8.Controllers.Abstract;
using IAutoM8.Global;
using IAutoM8.Infrastructure.JWT;
using IAutoM8.Service.Users.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IAutoM8.Service.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using IAutoM8.Service.Resources.Interface;
using System;

namespace IAutoM8.Controllers.api
{
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IInfusionSoftDataService _infusionSoftDataService;
        private readonly IResourceService _resourceService;//Commented

        public AccountController(IUserService userService, ITokenProvider tokenProvider,
            IInfusionSoftDataService infusionSoftDataService, IResourceService resourceService)
        {
            _userService = userService;
            _tokenProvider = tokenProvider;
            _infusionSoftDataService = infusionSoftDataService;
            _resourceService = resourceService;
        }

        [Route("infusiondata")]
        [HttpGet]
        public async Task<InfusionSoftDataDto> GetInfusionsoftData()
        {
            return await _infusionSoftDataService.GetDataAsync();
        }

        [Route("createsuperadmin")]
        [HttpPost]
        public async Task<IActionResult> CreateSuperAdmin([FromBody] SignUpDto data)
        {
            await _userService.SignUp(data, new[] { UserRoles.Owner });
            return Ok();
        }

        [Route("addrole")]
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] AddRoleDto data)
        {
            await _userService.AddRole(data);
            return Ok();
        }

        [Authorize]
        [Route("profile")]
        [HttpGet]
        public async Task<ProfileDto> GetProfile()
        {
            return await _userService.GetUserProfile(UserGuid);
        }

        [Authorize]
        [Route("profile/{userGuid:Guid}")]
        [HttpGet]
        public async Task<ProfileDto> GetProfile(Guid userGuid)
        {
            return await _userService.GetUserProfile(userGuid);
        }

        [Authorize]
        [Route("profile")]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto profile)
        {
            return Json(_tokenProvider.GenerateToken(await _userService.UpdateUserProfile(UserGuid, profile)));
        }

        [Route("confirmemail")]
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmUserDto user)
        {
            var principal = await _userService.ConfirmUser(user);
            return Json(_tokenProvider.GenerateToken(principal));
        }

        [Route("restorepassword")]
        [HttpPost]
        public async Task<IActionResult> RestorePassword([FromBody] ForgotPasswordSubmitDto user)
        {
            var principal = await _userService.ChangeForgotPasswordMessage(user);
            return Json(_tokenProvider.GenerateToken(principal));
        }

        [Route("forgotpassword")]
        [HttpPut]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto user)
        {
            await _userService.SendForgotPasswordMessage(user);
            return Ok();
        }

        //[HttpPost]
        //[Authorize(Roles = VendorOrBussinessMember)]
        //[Authorize(Roles = VendorOrBussinessOrCompanyWorkerMember)]
        //[Authorize(Roles = VendorOrBussinessOrCompanyWorkerOrCompanyMember)]
        //[Route("upload-file")]
        //public async Task<IActionResult> UploadFile(IFormFile qqfile)
        //{
        //    await _resourceService.UploadProfilePic(qqfile, UserGuid);
        //    return Ok(_userService.GetUserProfile(UserGuid));
        //}
    }
}
