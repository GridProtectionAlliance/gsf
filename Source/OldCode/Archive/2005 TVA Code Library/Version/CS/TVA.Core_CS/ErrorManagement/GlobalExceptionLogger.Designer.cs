using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

namespace TVA
{
    namespace ErrorManagement
    {

        public partial class GlobalExceptionLogger : System.ComponentModel.Component
        {


            [System.Diagnostics.DebuggerNonUserCode()]
            public GlobalExceptionLogger(System.ComponentModel.IContainer container)
                : this()
            {

                //Required for Windows.Forms Class Composition Designer support
                if (container != null)
                {
                    container.Add(this);
                }

            }

            [System.Diagnostics.DebuggerNonUserCode()]
            public GlobalExceptionLogger()
            {

                //This call is required by the Component Designer.
                InitializeComponent();

                m_autoRegister = DefaultAutoRegister;
                m_exitOnUnhandledException = DefaultExitOnUnhandledException;
                m_logToUI = DefaultLogToUI;
                m_logToFile = DefaultLogToFile;
                m_logToEmail = DefaultLogToEmail;
                m_logToEventLog = DefaultLogToEventLog;
                m_logToScreenshot = DefaultLogToScreenshot;
                m_smtpServer = DefaultSmtpServer;
                m_contactName = DefaultContactName;
                m_contactEmail = DefaultContactEmail;
                m_contactPhone = DefaultContactPhone;
                m_persistSettings = DefaultPersistSettings;
                m_settingsCategoryName = DefaultSettingsCategoryName;

                m_errorTextMethod = new System.EventHandler(GetErrorText);
                m_scopeTextMethod = new System.EventHandler(GetScopeText);
                m_actionTextMethod = new System.EventHandler(GetActionText);
                m_moreInfoTextMethod = new System.EventHandler(GetMoreInfoText);
                m_loggers = new List<LoggerMethodSignature>();

            }

            //Component overrides dispose to clean up the component list.
            [System.Diagnostics.DebuggerNonUserCode()]
            protected override void Dispose(bool disposing)
            {
                try
                {
                    Unregister();
                    SaveSettings();
                    if (disposing && (components != null))
                    {
                        components.Dispose();
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            //Required by the Component Designer
            private System.ComponentModel.Container components = null;

            //NOTE: The following procedure is required by the Component Designer
            //It can be modified using the Component Designer.
            //Do not modify it using the code editor.
            [System.Diagnostics.DebuggerStepThrough()]
            private void InitializeComponent()
            {
                this.components = new System.ComponentModel.Container();
                this._LogFile = new TVA.IO.LogFile(this.components);
                ((System.ComponentModel.ISupportInitialize)this._LogFile).BeginInit();
                //
                //_LogFile
                //
                this._LogFile.Name = "LogFile.txt";
                ((System.ComponentModel.ISupportInitialize)this._LogFile).EndInit();

            }
            internal TVA.IO.LogFile _LogFile;

        }

    }
}
