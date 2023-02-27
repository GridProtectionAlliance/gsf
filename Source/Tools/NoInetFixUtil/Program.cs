using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ReSharper disable InconsistentNaming
namespace NoInetFixUtil
{
    static class Program
    {
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetConnectedState(out int lpdwFlags, int dwReserved);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            static bool InternetConnectionDetected()
            {
                try
                {
                    return InternetGetConnectedState(out int _, 0);
                }
                catch
                {
                    return false;
                }
            }

            bool checkAll = args.Length > 0 && args[0].ToLower().Trim() == "--checkall" && !InternetConnectionDetected();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(checkAll));
        }
    }
}
