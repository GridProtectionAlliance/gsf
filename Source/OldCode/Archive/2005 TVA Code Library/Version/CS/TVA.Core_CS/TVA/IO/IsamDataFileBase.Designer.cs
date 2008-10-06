using System.IO;

namespace TVA.IO
{
    public partial class IsamDataFileBase<T> : System.ComponentModel.Component where T : TVA.Parsing.IBinaryDataProvider
    {
        [System.Diagnostics.DebuggerNonUserCode()]
        public IsamDataFileBase(System.ComponentModel.IContainer container)
            : this()
        {
            //Required for Windows.Forms Class Composition Designer support.
            if (container != null)
                container.Add(this);
        }

        [System.Diagnostics.DebuggerNonUserCode()]
        public IsamDataFileBase()
        {
            //Required by the Component Designer.
            InitializeComponent();

            m_name = this.GetType().Name + Extension;
            m_minimumRecordCount = 0;
            m_loadOnOpen = true;
            m_reloadOnModify = true;
            m_autoSaveInterval = -1;
            m_settingsCategoryName = this.GetType().Name;

            m_autoSaveTimer = new System.Timers.Timer();
            m_autoSaveTimer.Elapsed += m_autoSaveTimer_Elapsed;

            m_loadWaitHandle = new System.Threading.ManualResetEvent(true);
            m_saveWaitHandle = new System.Threading.ManualResetEvent(true);

        }

        //Overrides dispose to clean up the component list.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Close();        // Closes the file.
                    SaveSettings(); // Saves settings to the config file.

                    if (FileSystemWatcher != null)
                    {
                        FileSystemWatcher.Changed -= FileSystemWatcher_Changed;
                        FileSystemWatcher.Dispose();
                    }
                    FileSystemWatcher = null;

                    if (m_loadWaitHandle != null) m_loadWaitHandle.Close();
                    m_loadWaitHandle = null;

                    if (m_saveWaitHandle != null) m_saveWaitHandle.Close();
                    m_saveWaitHandle = null;

                    if (m_autoSaveTimer != null) m_autoSaveTimer.Dispose();
                    m_autoSaveTimer = null;

                    if (components != null)
                        components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //Required by the Component Designer.
        private System.ComponentModel.Container components = null;

        //NOTE: Required by the Component Designer.
        //Can be modified using the Component Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.FileSystemWatcher = new FileSystemWatcher();
            this.FileSystemWatcher.Changed += FileSystemWatcher_Changed;
            ((System.ComponentModel.ISupportInitialize)this.FileSystemWatcher).BeginInit();
            //
            //FileSystemWatcher
            //
            this.FileSystemWatcher.EnableRaisingEvents = true;
            ((System.ComponentModel.ISupportInitialize)this.FileSystemWatcher).EndInit();

        }

        internal FileSystemWatcher FileSystemWatcher;
    }
}