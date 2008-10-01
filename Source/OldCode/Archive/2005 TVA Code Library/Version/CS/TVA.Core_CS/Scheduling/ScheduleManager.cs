//*******************************************************************************************************
//  ScheduleManager.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/01/2006 - Pinal C. Patel
//       Original version of source code generated.
//  04/23/2007 - Pinal C. Patel
//       Made the schedules dictionary case-insensitive.
//  04/24/2007 - Pinal C. Patel
//       Implemented the IPersistSettings and ISupportInitialize interfaces.
//  05/02/2007 - Pinal C. Patel
//       Converted schedules to a list instead of a dictionary.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//       Modified the Stop() method to avoid a  null reference exception that was most likely causing
//       the IDE of the component's consumer to crash in design-mode.
//  09/19/2008 - James R Carroll
//       Convert to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using TVA.Configuration;
using TVA.Services;
using TVA.Threading;

namespace TVA.Scheduling
{
    /// <summary>Manages multiple schedules defined as scheduling objects.</summary>
    /// <remarks>
    /// <para>Note that schedules are based on UNIX crontab syntax.</para>
    /// <para>
    /// Operators:
    /// </para>
    /// <para>
    /// There are several ways of specifying multiple date/time values in a field:
    /// <list type="bullet">
    /// <item>
    ///     <description>
    ///         The comma (',') operator specifies a list of values, for example: "1,3,4,7,8"
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         The dash ('-') operator specifies a range of values, for example: "1-6",
    ///         which is equivalent to "1,2,3,4,5,6"
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         The asterisk ('*') operator specifies all possible values for a field.
    ///         For example, an asterisk in the hour time field would be equivalent to
    ///         'every hour' (subject to matching other specified fields).
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         The slash ('/') operator (called "step"), which can be used to skip a given
    ///         number of values. For example, "*/3" in the hour time field is equivalent
    ///         to "0,3,6,9,12,15,18,21". So "*" specifies 'every hour' but the "*/3" means
    ///         only those hours divisible by 3.
    ///     </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Fields:
    /// </para>
    /// <para>
    /// <code>
    ///     +---------------- minute (0 - 59)
    ///     |  +------------- hour (0 - 23)
    ///     |  |  +---------- day of month (1 - 31)
    ///     |  |  |  +------- month (1 - 12)
    ///     |  |  |  |  +---- day of week (0 - 7) (Sunday=0 or 7)
    ///     |  |  |  |  |
    ///     *  *  *  *  *
    /// </code>
    /// </para>
    /// <para>
    /// Each of the patterns from the first five fields may be either * (an asterisk), which matches all legal values,
    /// or a list of elements separated by commas. 
    /// </para>
    /// <para>
    /// See http://en.wikipedia.org/wiki/Cron for more information.
    /// </para>
    /// </remarks>
    [ToolboxBitmap(typeof(ScheduleManager)), DefaultEvent("ScheduleDue")]
    public partial class ScheduleManager : Component, IServiceComponent, IPersistSettings, ISupportInitialize
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for Enabled property.
        /// </summary>
        public const bool DefaultEnabled = true;

        /// <summary>
        /// Default value for PersistSettings property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Default value for SettingsCategoryName property.
        /// </summary>
        public const string DefaultSettingsCategoryName = "ScheduleManager";

        // Events

        /// <summary>
        /// Occurs while the schedule manager is waiting to start at top of the minute.
        /// </summary>
        [Category("State")]
        public event EventHandler Starting;

        /// <summary>
        /// Occurs when the schedule manager has started.
        /// </summary>
        [Category("State")]
        public event EventHandler Started;

        /// <summary>
        /// Occurs when the schedule manager has stopped.
        /// </summary>
        [Category("State")]
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when the a particular schedule is being checked to see if it is due.
        /// </summary>
        [Category("Schedules")]
        public event EventHandler<ScheduleEventArgs> CheckingSchedule;

        /// <summary>
        /// Occurs when a schedule is due according to the rule specified for the schedule.
        /// </summary>
        [Category("Schedules")]
        public event EventHandler<ScheduleEventArgs> ScheduleDue;

        // Fields
        private bool m_enabled;
        private List<Schedule> m_schedules;
        private bool m_persistSettings;
        private string m_settingsCategoryName;
        private bool m_previouslyEnabled;
        private System.Timers.Timer m_timer;
#if ThreadTracking
        private ManagedThread m_startTimerThread;
#else
        private Thread m_startTimerThread;
#endif
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public ScheduleManager()
        {
            m_enabled = DefaultEnabled;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategoryName = DefaultSettingsCategoryName;
            m_schedules = new List<Schedule>();
            m_timer = new System.Timers.Timer(60000);
            m_timer.Elapsed += m_timer_Elapsed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a list of all the schedules.
        /// </summary>
        /// <value></value>
        /// <returns>A list of the schedules.</returns>
        public List<Schedule> Schedules
        {
            get
            {
                return m_schedules;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the schedule manager is enabled.
        /// </summary>
        /// <value></value>
        /// <returns>True if the schedule manager is enabled; otherwise False.</returns>
        [Category("Behavior"), DefaultValue(DefaultEnabled)]
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the component settings are to be persisted to the config file.
        /// </summary>
        /// <value></value>
        /// <returns>True if the component settings are to be persisted to the config file; otherwise False.</returns>
        [Category("Persistance"), DefaultValue(DefaultPersistSettings), Description("Indicates whether the component settings are to be persisted to the config file.")]
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category name under which the component settings are to be saved in the config file.
        /// </summary>
        /// <value></value>
        /// <returns>The category name under which the component settings are to be saved in the config file.</returns>
        [Category("Persistance"), DefaultValue(DefaultSettingsCategoryName), Description("The category name under which the component settings are to be saved in the config file.")]
        public string SettingsCategoryName
        {
            get
            {
                return m_settingsCategoryName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_settingsCategoryName = value;
                else
                    throw new ArgumentNullException("SettingsCategoryName");
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the schedule manager is running.
        /// </summary>
        /// <value></value>
        /// <returns>True if the schedule manager is running; otherwise False.</returns>
        [Browsable(false)]
        public bool IsRunning
        {
            get
            {
                return m_timer.Enabled;
            }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("        Number of schedules: ");
                status.Append(m_schedules.Count);
                status.AppendLine();
                status.AppendLine();

                foreach (Schedule schedule in m_schedules)
                {
                    status.Append(schedule.Status);
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="ScheduleManager" /> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        Stop();         // Stop the schedule manager.
                        SaveSettings(); // Saves settings to the config file.

                        if (m_timer != null)
                        {
                            m_timer.Elapsed -= m_timer_Elapsed;
                            m_timer.Dispose();
                        }

                        m_timer = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Gets the schedule with the specified schedule name.
        /// </summary>
        /// <param name="scheduleName">Name of the schedule that is to be found.</param>
        /// <value></value>
        /// <returns>The TVA.Scheduling.Schedule instance for the specified schedule name if found; otherwise Nothing.</returns>
        public Schedule Schedules(string scheduleName)
        {
            Schedule match = null;

            foreach (Schedule schedule in m_schedules)
            {
                if (string.Compare(schedule.Name, scheduleName, true) == 0)
                {
                    match = schedule;
                    break;
                }
            }

            return match;
        }

        /// <summary>
        /// Starts the schedule manager asynchronously.
        /// </summary>
        public void Start()
        {

            if (m_enabled && !m_timer.Enabled)
            {
#if ThreadTracking
                m_startTimerThread = new ManagedThread(StartTimer);
                m_startTimerThread.Name = "TVA.Scheduling.ScheduleManager.StartTimer()";
#else
                m_startTimerThread = new Thread(StartTimer);
#endif
                m_startTimerThread.Start();
            }

        }

        /// <summary>
        /// Stops the schedule manager.
        /// </summary>
        public void Stop()
        {
            if (m_enabled)
            {
                if ((m_startTimerThread != null) && m_startTimerThread.IsAlive)
                    m_startTimerThread.Abort();

                if (m_timer.Enabled)
                {
                    m_timer.Stop();

                    if (Stopped != null)
                        Stopped(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Checks all of the schedules to determine if they are due.
        /// </summary>
        public void CheckAllSchedules()
        {
            if (m_enabled)
            {
                foreach (Schedule schedule in m_schedules)
                {
                    if (CheckingSchedule != null)
                        CheckingSchedule(this, new ScheduleEventArgs(schedule));

                    if (schedule.IsDue())
                        OnScheduleDue(this, new ScheduleEventArgs(schedule)); // Event raised asynchronously.
                }
            }
        }

        public void ProcessStateChanged(string processName, ProcessState newState)
        {
        }

        public void ServiceStateChanged(ServiceState newState)
        {
            switch (newState)
            {
                case ServiceState.Started:
                    Start();
                    break;
                case ServiceState.Stopped:
                    Stop();
                    break;
                case ServiceState.Paused:
                    m_previouslyEnabled = Enabled;
                    Enabled = false;
                    break;
                case ServiceState.Resumed:
                    Enabled = m_previouslyEnabled;
                    break;
                case ServiceState.Shutdown:
                    Dispose();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Loads previously saved schedules from the config file.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                foreach (CategorizedSettingsElement setting in ConfigurationFile.Current.Settings[m_settingsCategoryName])
                {
                    // Add the schedule if it doesn't exist or update it otherwise with data from the config file.
                    Schedule existingSchedule = Schedules(setting.Name);

                    if (existingSchedule == null)
                    {
                        // Schedule doesn't exist, so we'll add it.
                        m_schedules.Add(new Schedule(setting.Name, setting.Value, setting.Description));
                    }
                    else
                    {
                        // Schedule exists, so we'll update it.
                        existingSchedule.Name = setting.Name;
                        existingSchedule.Rule = setting.Value;
                        existingSchedule.Description = setting.Description;
                    }
                }
            }
            catch
            {
                // We'll encounter exceptions if the settings are not present in the config file.
            }
        }

        /// <summary>
        /// Saves all schedules to the config file.
        /// </summary>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];

                    settings.Clear();
                    foreach (Schedule schedule in m_schedules)
                    {
                        settings.Add(schedule.Name, schedule.Rule, schedule.Description);
                    }

                    ConfigurationFile.Current.Save();
                }
                catch
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        public void BeginInit()
        {
            // We don't need to do anything before the component is initialized.
        }

        public void EndInit()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                LoadSettings(); // Load settings from the config file.
        }

        protected void OnScheduleDue(object sender, ScheduleEventArgs e)
        {
            if (ScheduleDue != null)
            {
                foreach (EventHandler<ScheduleEventArgs> handler in ScheduleDue.GetInvocationList())
                {
                    // Asynchrnonously invoke handlers...
                    handler.BeginInvoke(sender, e, null, null);
                }
            }
        }

        private void StartTimer()
        {
            while (true)
            {
                if (Starting != null)
                    Starting(this, EventArgs.Empty);

                if (DateTime.Now.Second == 0)
                {
                    // We'll start the timer that will check the schedules at top of the minute.
                    m_timer.Start();

                    if (Started != null)
                        Started(this, EventArgs.Empty);

                    CheckAllSchedules();

                    break;
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
        }

        private void m_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckAllSchedules();
        }

        #endregion
   }
}