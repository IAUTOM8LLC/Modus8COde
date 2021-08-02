using IAutoM8.Controllers.Abstract;
using IAutoM8.Service.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersList()
        {
            var users = await _userService.GetUsers();
            return Ok(users);
        }
    }
}
