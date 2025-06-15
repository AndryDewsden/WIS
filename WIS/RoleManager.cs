using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIS
{
    public static class RoleManager
    {
        public enum UserRole
        {
            Administrator = 1,
            ITSpecialist = 5,
            Manager = 2,
            Accountant = 4,
            User = 3
        }

        public enum AccessLevel
        {
            User = 1,
            Accountant = 2,
            Manager = 3,
            ITSpecialist = 4,
            Administrator = 5
        }

        public static readonly Dictionary<UserRole, int> RoleHierarchy = new Dictionary<UserRole, int>
        {
            { UserRole.Administrator, 5 },
            { UserRole.ITSpecialist, 4 },
            { UserRole.Manager, 3 },
            { UserRole.Accountant, 2 },
            { UserRole.User, 1 }
        };

        public static int GetRoleLevel(UserRole role)
        {
            return RoleHierarchy.TryGetValue(role, out int level) ? level : 0;
        }

        public static bool HasAccess(UserRole userRole, AccessLevel requiredLevel)
        {
            return GetRoleLevel(userRole) >= (int)requiredLevel;
        }
    }
}
