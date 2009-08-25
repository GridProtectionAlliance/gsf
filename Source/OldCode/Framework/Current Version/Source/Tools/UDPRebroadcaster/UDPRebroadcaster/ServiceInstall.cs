using System.ComponentModel;
using System.Configuration.Install;


namespace UDPRebroadcaster
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
