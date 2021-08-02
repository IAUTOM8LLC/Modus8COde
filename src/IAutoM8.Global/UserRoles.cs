using System;
using System.Collections.Generic;
using System.Linq;

namespace IAutoM8.Global
{
    public static class UserRoles
    {
        public const string Owner = "Owner";
        public const string Manager = "Manager";
        public const string Worker = "Worker";
        public const string Vendor = "Vendor";
        public const string Admin = "Admin";
        public const string CompanyWorker = "CompanyWorker";
        public const string Company = "Company";

        public static IEnumerable<string> Roles =>
            typeof(UserRoles).GetFields().Select(p => p.Name);

        public static bool IsRoleDefined(string role) =>
            Roles.Contains(role);

        public static string RoleName(Guid roleId) //Remember to Update the case id's(RoleId),RoleName in case of change in roleId and roleName from DB.
        {
            var roleName = string.Empty;

            switch (roleId.ToString().ToUpper())
            {
                case "66184769-E6EB-4F54-06D8-08D538D647A9":
                    roleName = Owner;
                    break;
                case "C6C9DE9A-3F19-4A35-6578-08D53C14C887":
                    roleName = Manager;
                    break;
                case "AB6F4BDA-56F8-4BCC-6579-08D53C14C887":
                    roleName = Worker;
                    break;
                case "5CBD430E-8D96-470D-2028-08D60200F923":
                    roleName = Vendor;
                    break;
                case "6CBD430E-8D96-470D-2028-08D60200F923":
                    roleName = CompanyWorker;
                    break;
                case "7CBD430E-8D96-470D-2028-08D60200F923":
                    roleName = Company;
                    break;
                case "181B961B-CB3E-470F-E2DB-08D7A6650570":
                    roleName = Admin;
                    break;
            }
            return roleName;
        }
    }
}
