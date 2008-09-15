using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;

namespace TVA.Scheduling
{
    public partial class ScheduleManager : Component
    {
        [DebuggerNonUserCode()]
        public ScheduleManager(IContainer Container)
            : this()
        {
            //Required for Windows.Forms Class Composition Designer support
            Container.Add(this);
        }

        [DebuggerNonUserCode()]
        public ScheduleManager()
        {
            //This call is required by the Component Designer.
            InitializeComponent();

            m_enabled = DefaultEnabled;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategoryName = DefaultSettingsCategoryName;

            m_timer = new System.Timers.Timer(60000);
            m_timer.Elapsed += m_timer_Elapsed;
            m_schedules = new List<Schedule>();
        }

        //Component overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            Stop(); // Stop the schedule manager.
            SaveSettings(); // Saves settings to the config file.

            if (disposing)
            {
                if (m_timer != null)
                {
                    m_timer.Elapsed -= m_timer_Elapsed;
                    m_timer.Dispose();
                }

                m_timer = null;

                if (components != null)
                    components.Dispose();

                components = null;
            }
            base.Dispose(disposing);
        }

        //Required by the Component Designer
        private Container components = null;

        //NOTE: The following procedure is required by the Component Designer
        //It can be modified using the Component Designer.
        //Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new Container();
        }
    }
}