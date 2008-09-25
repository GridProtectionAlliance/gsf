using System.Text;
using System.Threading;
using TVA.Collections;

namespace TVA.IO
{
    public partial class LogFile : System.ComponentModel.Component
    {
        [System.Diagnostics.DebuggerNonUserCode()]
        public LogFile(System.ComponentModel.IContainer container)
            : this()
        {
            //Required for Windows.Forms Class Composition Designer support.
            if (container != null)
                container.Add(this);
        }

        [System.Diagnostics.DebuggerNonUserCode()]
        public LogFile()
        {
            //Required by the Component Designer.
            InitializeComponent();

            m_name = DefaultName;
            m_size = DefaultSize;
            m_autoOpen = DefaultAutoOpen;
            m_fileFullOperation = DefaultFileFullOperation;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategoryName = DefaultSettingsCategoryName;

            m_operationWaitHandle = new ManualResetEvent(true);

            m_logEntryQueue = ProcessQueue<string>.CreateSynchronousQueue(WriteLogEntries);
            m_logEntryQueue.ProcessException += m_logEntryQueue_ProcessException;

            m_textEncoding = Encoding.Default;
        }

        //Overrides dispose to clean up the component list.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!m_disposed)
                {
                    Close(); // Closes the file.
                    SaveSettings(); // Saves settings to the config file.

                    if (disposing && (components != null))
                        components.Dispose();
                }

                m_disposed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //Required by the Component Designer.
        private System.ComponentModel.Container components = null;

        //NOTE: Required by the Component Designer
        //Can be modified using the Component Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
    }
}