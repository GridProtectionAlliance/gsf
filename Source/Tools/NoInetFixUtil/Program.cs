using System;
using System.Windows.Forms;

// ReSharper disable InconsistentNaming
namespace NoInetFixUtil
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(args.Length > 0 && args[0].ToLower().Trim() == "--checkall"));
        }
    }
}
