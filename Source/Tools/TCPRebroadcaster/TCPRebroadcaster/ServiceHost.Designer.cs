namespace TCPRebroadcaster
{
    partial class ServiceHost
    {
        #region [ Service Binding ]

        internal void StartDebugging(string[] args)
        {
            OnStart(args);
        }

        internal void StopDebugging()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            m_serviceHelper.OnStart(args);
        }

        protected override void OnStop()
        {
            m_serviceHelper.OnStop();
        }

        protected override void OnPause()
        {
            m_serviceHelper.OnPause();
        }

        protected override void OnContinue()
        {
            m_serviceHelper.OnResume();
        }

        protected override void OnShutdown()
        {
            m_serviceHelper.OnShutdown();
        }

        #endregion

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_serviceHelper = new GSF.ServiceProcess.ServiceHelper(this.components);
            this.m_remotingServer = new GSF.Communication.TcpServer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_remotingServer)).BeginInit();
            // 
            // m_serviceHelper
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            this.m_serviceHelper.ErrorLogger.ErrorLog.FileName = "TCPRebroadcaster.ErrorLog.txt";
            this.m_serviceHelper.ErrorLogger.ErrorLog.PersistSettings = true;
            this.m_serviceHelper.ErrorLogger.ErrorLog.SettingsCategory = "ErrorLog";
            this.m_serviceHelper.ErrorLogger.PersistSettings = true;
            this.m_serviceHelper.ParentService = this;
            this.m_serviceHelper.PersistSettings = true;
            // 
            // 
            // 
            this.m_serviceHelper.ProcessScheduler.PersistSettings = true;
            this.m_serviceHelper.ProcessScheduler.SettingsCategory = "ProcessScheduler";
            this.m_serviceHelper.RemotingServer = this.m_remotingServer;
            // 
            // 
            // 
            this.m_serviceHelper.StatusLog.FileName = "TCPRebroadcaster.StatusLog.txt";
            this.m_serviceHelper.StatusLog.PersistSettings = true;
            this.m_serviceHelper.StatusLog.SettingsCategory = "StatusLog";
            // 
            // m_remotingServer
            // 
            this.m_remotingServer.ConfigurationString = "Port=4343";
            this.m_remotingServer.IntegratedSecurity = true;
            this.m_remotingServer.PayloadAware = true;
            this.m_remotingServer.PersistSettings = true;
            this.m_remotingServer.SettingsCategory = "RemotingServer";
            // 
            // ServiceHost
            // 
            this.ServiceName = "TCPRebroadcaster";
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_remotingServer)).EndInit();

        }

        #endregion

        private GSF.ServiceProcess.ServiceHelper m_serviceHelper;
        private GSF.Communication.TcpServer m_remotingServer;
    }
}
