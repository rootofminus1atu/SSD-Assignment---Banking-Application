using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;


namespace Banking_Application
{
    public static class ActiveDirectoryHelper
    {
        private const string DOMAIN = "ITSLIGO.LAN";
        private const string TELLER_GROUP = "Bank Teller";
        private const string ADMIN_GROUP = "Bank Teller Administrator";

        public static bool IsUserInTellerGroup()
        {
            return IsUserInGroup(TELLER_GROUP);
        }

        public static bool IsUserInAdminGroup()
        {
            return IsUserInGroup(ADMIN_GROUP);
        }

        private static bool IsUserInGroup(string groupName)
        {
            using var context = new PrincipalContext(ContextType.Domain, DOMAIN);
            using var user = UserPrincipal.FindByIdentity(
                context,
                WindowsIdentity.GetCurrent().Name
            );

            if (user == null)
                return false;

            return user.IsMemberOf(
                GroupPrincipal.FindByIdentity(context, groupName)
            );
        }
    }

}
