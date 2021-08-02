using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IAutoM8.Domain.Models.User;
using IAutoM8.Service.Users.Dto;

namespace IAutoM8.Service.Users.Interfaces
{
    public interface IUserService
    {
        Task AddRole(AddRoleDto data);
        Task SignUp(SignUpDto data, string[] roles = null);
        Task<ClaimsPrincipal> SignIn(SignInDto data);
        Task<ProfileDto> GetUserProfile(Guid userId);
        Task<ClaimsPrincipal> UpdateUserProfile(Guid userId, ProfileDto profile);
        Task<ClaimsPrincipal> RefreshUser();
        Task<List<UserDropdownItemDto>> GetUsers();
        Task<List<UserDropdownItemDto>> GetOwnerUsers(Guid ownerGuid);
        Task ChangeRoleTo(AssigneeUserDto user, string roleTo);
        Task CreateAssigneeUser(string ownerUser, AssigneeUserDto user);
        Task ResendConfirmationEmail(Guid userGuid);
        Task<List<CompanyUserDto>> GetOwnerUsersWithRoles(string currentUser);
        Task SendForgotPasswordMessage(ForgotPasswordDto model);
        Task<ClaimsPrincipal> ChangeForgotPasswordMessage(ForgotPasswordSubmitDto model);
        Task<ClaimsPrincipal> ConfirmUser(ConfirmUserDto model);
        Task<Guid> SetLockStatusForUser(Guid userGuid, bool @lock);
        Task DeleteUser(Guid userGuid);
        Task<List<UserFilterItemDto>> GetUsersForAllProjects(IEnumerable<int> projectIds);
        Task SaveSignUpInfusionData(InfusionSignUpDto data);

        #region   For Super Admin
        Task<List<UserProfileWithRoleDto>> GetAllUserWithRole();
        Task<User> ChangeEmail(ChangeEmail changeEmail);
        Task ResendConfirmationEmailByAdmin(string eMail);
        Task<ClaimsPrincipal> UserSignInByAdmin(SignInDto data);

        Task<ClaimsPrincipal> RefreshNewUser(ClaimsPrincipal claims);
        #endregion
    }
}
