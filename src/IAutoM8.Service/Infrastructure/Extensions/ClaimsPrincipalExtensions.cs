using IAutoM8.Global;
using System;
using System.Linq;
using System.Security.Claims;

namespace IAutoM8.Service.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user) =>
            new Guid(user.Claims.First(w => w.Type == ClaimTypes.NameIdentifier).Value);

        public static Guid GetOwnerId(this ClaimsPrincipal user) =>
            new Guid(user.Claims.First(w => w.Type == ClaimTypes.PrimarySid).Value);

        public static bool IsOwner(this ClaimsPrincipal user) =>
             user.IsInRole(UserRoles.Owner);

        public static bool IsAdmin(this ClaimsPrincipal user) =>
             user.IsInRole(UserRoles.Admin);

        public static bool IsManager(this ClaimsPrincipal user) =>
             user.IsInRole(UserRoles.Manager);


        //public static bool IsVendor(this ClaimsPrincipal user) =>
        //     user.IsInRole(UserRoles.Vendor) || user.IsInRole(UserRoles.CompanyWorker); // added logic for new role CompanyWorker WRT Sprint 10B

        public static bool IsVendor(this ClaimsPrincipal user) =>
            user.IsInRole(UserRoles.Vendor) || user.IsInRole(UserRoles.CompanyWorker) || user.IsInRole(UserRoles.Company); // added logic for new role CompanyWorker and Company WRT Sprint 10B

        public static bool CanBeOnlyWorker(this ClaimsPrincipal user) =>
            !user.IsOwner() && !user.IsManager();

        public static bool IsWorker(this ClaimsPrincipal user) =>
             user.IsInRole(UserRoles.Worker);

        public static string GetUserRole(this ClaimsPrincipal user) => user.Claims.Single(x => x.Type == ClaimTypes.Role).Value;

        public static string GetFullName(this ClaimsPrincipal user) =>
            user?.Claims.FirstOrDefault(w => w.Type == CustomClaimType.fullName)?.Value;

        public static Guid? GetOptionalUserId(this ClaimsPrincipal user)
        {
            var identity = user?.Claims.FirstOrDefault(w => w.Type == ClaimTypes.NameIdentifier);
            if (identity == null)
                return null;
            return new Guid(identity.Value);
        }
    }
}
