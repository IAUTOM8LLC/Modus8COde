using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IAutoM8.Controllers.Abstract;
using IAutoM8.Domain.Models.User;
using IAutoM8.Infrastructure.JWT;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IAutoM8.Controllers.api
{
    [Route("api/[controller]")]
    //[Authorize(Roles = Admin )]
    [Authorize(Roles = AdminOrCompany)]
    public class SuperAdminController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITokenProvider _tokenProvider;
        public SuperAdminController(IUserService userService, ITokenProvider tokenProvider)
        {
            _userService = userService;
            _tokenProvider = tokenProvider;
        }

        [Route("all-users")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(string filterSearch)
        {
            var users = await _userService.GetAllUserWithRole();
            if (!string.IsNullOrEmpty(filterSearch))
            {
                filterSearch = filterSearch.ToLower();
                users = users.Where(w =>(w.FullName.ToLower().Contains(filterSearch)) ||
                    (w.Email.ToLower().Contains(filterSearch)) ||
                    (w.Role.ToLower().Contains(filterSearch))).ToList();
            }
            return Ok(users);
        }
        [Route("change-email")]
        [HttpPost]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmail changeEmail)
        {
            User user = await _userService.ChangeEmail(changeEmail);
            return Ok(user);
        }

        [Route("resend-email")]
        [HttpPut]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ChangeEmail email)
        {
            await _userService.ResendConfirmationEmailByAdmin(email.eMail);
            return Ok("Email Confirmed");
        }

        [Route("signin-user")]
        [HttpPost]
        public async Task<IActionResult> SignInAsUser([FromBody] SignInDto data)
        {
            var user = await _userService.UserSignInByAdmin(data);
            return Json(_tokenProvider.GenerateToken(user, data.Remember));
        }
    }
}
