using System;
using System.Security.Principal;
using GSF.Collections;
using GSF.Identity;

namespace UserInfoTest
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Current user = " + WindowsIdentity.GetCurrent().Name);
            string userName, password, errorMessage;

            Console.Write("Enter user name: ");
            userName = Console.ReadLine();

            Console.Write("Enter password: ");

            ConsoleKeyInfo consoleKey = Console.ReadKey(true);
            password = "";

            while (consoleKey.Key != ConsoleKey.Enter)
            {
                Console.Write('*');
                password += consoleKey.KeyChar;
                consoleKey = Console.ReadKey(true);
            }

            Console.WriteLine();

            IPrincipal principal = UserInfo.AuthenticateUser("", userName, password, out errorMessage);

            if ((object)principal == null)
            {
                if (string.IsNullOrEmpty(errorMessage))
                    Console.WriteLine("Authentication failed without error message.");
                else
                    Console.WriteLine("Authentication failed: " + errorMessage);
            }
            else
            {
                Console.WriteLine("Authentication succeeded!");
            }

            Console.WriteLine();

            // Show info for root user
            Console.WriteLine("\nRunning user \"{0}\" information:\n", UserInfo.CurrentUserInfo.LoginID);
            ShowUserInfo();

            Console.WriteLine("\nAttempting impersonation of \"{0}\"...", userName);

            WindowsImpersonationContext context = UserInfo.ImpersonateUser("", userName, password);

            if ((object)context != null)
            {
                try
                {
                    Console.WriteLine("Impersonation of \"{0}\" succeeded, information:\n", UserInfo.CurrentUserInfo.LoginID);
                    ShowUserInfo();
                }
                finally
                {
                    UserInfo.EndImpersonation(context);
                }
            }
            else
            {
                Console.WriteLine("Impersonation of \"{0}\" failed.\n", userName);
            }

            return 0;
        }

        private static void ShowUserInfo()
        {
            UserInfo info = UserInfo.CurrentUserInfo;

            Console.WriteLine("Current user: " + info.LoginID);

            Console.WriteLine("Attempting initialization...");

            info.Initialize();

            Console.WriteLine("Groups: " + info.Groups.ToDelimitedString());

            Console.WriteLine("Next password change date: " + info.NextPasswordChangeDate);

            Console.WriteLine("{0} is in group {0} = {1}", info.UserName, UserInfo.UserIsInLocalGroup(info.UserName, info.UserName));

            Console.WriteLine("{0} group members: {1}", info.UserName, UserInfo.GetLocalGroupUserList(info.UserName).ToDelimitedString());

            Console.WriteLine("{0} account disabled = {1}", info.UserName, info.AccountIsDisabled);

            Console.WriteLine("{0} account locked-out = {1}", info.UserName, info.AccountIsLockedOut);

            Console.WriteLine("{0} account password cannot change = {1}", info.UserName, info.PasswordCannotChange);

            Console.WriteLine("{0} account password does not expire = {1}", info.UserName, info.PasswordDoesNotExpire);

            Console.WriteLine("{0} account last logon time = {1}", info.UserName, info.LastLogon);

            Console.WriteLine("{0} account creation date = {1}", info.UserName, info.AccountCreationDate);

            Console.WriteLine();
        }
    }
}
