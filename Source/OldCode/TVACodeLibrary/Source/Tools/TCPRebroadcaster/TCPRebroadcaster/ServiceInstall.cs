using System.ComponentModel;
using System.Configuration.Install;


namespace TCPRebroadcaster
{
    [RunInstaller(true)]
    public partial class ServiceInstall : Installer
    {
        public ServiceInstall()
        {
            InitializeComponent();
        }
    }
}
