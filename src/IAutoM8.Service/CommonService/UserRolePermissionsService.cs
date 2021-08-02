using IAutoM8.Global;
using IAutoM8.Global.Options;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using System;
using System.Security.Claims;

namespace IAutoM8.Service.CommonService
{
    public class UserRolePermissionsService: IUserRolePermissionsService
    {
        private readonly ClaimsPrincipal _principal;

        public UserRolePermissionsService(ClaimsPrincipal principal)
        {
            _principal = principal;
        }

        public RolePermissions GetPermissions()
        {
            switch (_principal.GetUserRole())
            {
                case UserRoles.Owner:
                    return UserRolePermissions.Owner;
                case UserRoles.Manager:
                    return UserRolePermissions.Manager;
                case UserRoles.Worker:
                    return UserRolePermissions.Worker;
                case UserRoles.Vendor:
                    return UserRolePermissions.Vendor;
                case UserRoles.Admin:
                    return UserRolePermissions.Admin;
                case UserRoles.CompanyWorker: // added new for company worker (WRT Sprint 10B)
                    return UserRolePermissions.CompanyWoker;
                case UserRoles.Company: // added new for company (WRT Sprint 10B)
                    return UserRolePermissions.Company;
                default:
                    throw new ArgumentException("Unknown or empty role");
            }
        }
    }
}
