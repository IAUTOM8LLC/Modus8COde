using IAutoM8.Service.Users.Dto;
using System.Collections.Generic;

namespace IAutoM8.Service.Infrastructure
{
    class UserItemDtoComparer : IEqualityComparer<UserItemDto>
    {
        public bool Equals(UserItemDto x, UserItemDto y)
        {
            return x.Email.Equals(y.Email);
        }

        public int GetHashCode(UserItemDto obj)
        {
            return obj.Email.GetHashCode();
        }
    }
}
