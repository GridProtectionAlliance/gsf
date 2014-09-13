using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GSF.Collections;
using GSF.Identity;

namespace UserInfoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string userName, password, errorMessage;

            Console.Write("Enter user name: ");
            userName = Console.ReadLine();

            Console.Write("Enter password: ");
            password = Console.ReadLine();

            IPrincipal principal = UserInfo.AuthenticateUser("", userName, password, out errorMessage);

            if ((object)principal == null)
            {
                if (string.IsNullOrEmpty(errorMessage))
                    Console.WriteLine("Authentication failed with error message.");
                else
                    Console.WriteLine("Authentication failed: " + errorMessage);
            }
            else
            {
                Console.WriteLine("Authentication succeeded!");
            }

            UserInfo info = UserInfo.CurrentUserInfo;

            Console.WriteLine("Current user: " + info.LoginID);

            Console.WriteLine("Attempting initialization...");
            
            info.Initialize();

            Console.WriteLine("Groups: " + info.LocalGroups.ToDelimitedString());

            Console.WriteLine("Next password change date: " + info.NextPasswordChangeDate);

            Console.ReadLine();
        }
    }
}
