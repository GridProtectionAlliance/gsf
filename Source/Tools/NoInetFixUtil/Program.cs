using System;
using System.Windows.Forms;

namespace NoInetFixUtil
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool checkAll = false ;

            if (args.Length > 0)
                if (args[0].ToLower().Trim() == "--checkall")
                    checkAll = true;
            
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(checkAll));
        }
    }
}
