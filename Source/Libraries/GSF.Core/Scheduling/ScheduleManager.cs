//******************************************************************************************************
//  ScheduleManager.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
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
//  09/19/2008 - J. Ritchie Carroll
//       Convert to C#.
//  11/04/2008 - Pinal C. Patel
//       Edited code comments.
//  06/18/2009 - Pinal C. Patel
//       Fixed the implementation of Enabled property.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  06/21/2010 - Stephen C. Wills
//       Modified code to avoid calls to abort the m_startTimerThread.
//  07/02/2010 - J. Ritchie Carroll
//       Fixed an issue related to accessing a disposed timer.
//  08/04/2010 - Pinal C. Patel
//       Fixed an issue with NullReferenceException being encountered when ScheduleManager object is 
//       disposed before it successfully starts up (StartTimer() method).
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Timers;
using GSF.Collections;
using GSF.Configuration;
using Timer = System.Timers.Timer;

namespace GSF.Scheduling
{
    /// <summary>
    /// Monitors multiple <see cref="Schedule"/> at an interval of one minute to check if they are due.
    /// </summary>
    /// <example>
    /// This example shows how to use the <see cref="ScheduleManager"/> component:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Scheduling;
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         ScheduleManager scheduler = new ScheduleManager();
    ///         scheduler.Initialize();
    ///         // Add event handlers.
    ///         scheduler.Starting += scheduler_Starting;
    ///         scheduler.Started += scheduler_Started;
    ///         scheduler.ScheduleDue += scheduler_ScheduleDue;
    ///         // Add test schedules.
    ///         scheduler.AddSchedule("Run.Notepad", "* * * * *");
    ///         scheduler.AddSchedule("Run.Explorer", "* * * * *");
    ///         // Start the scheduler.
    ///         scheduler.Start();
    /// 
    ///         Console.ReadLine();
    ///     }
    /// 
    ///     static void scheduler_Started(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Scheduler has started successfully.");
    ///     }
    /// 
    ///     static void scheduler_Starting(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Scheduler is waiting to be started.");
    ///     }
    /// 
    ///     static void scheduler_ScheduleDue(object sender, EventArgs<![CDATA[<]]>Schedule<![CDATA[>]]> e)
    ///     {
    ///         Console.WriteLine(string.Format("{0} schedule is due for processing.", e.Argument.Name));
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Schedule"/>
    [ToolboxBitmap(typeof(ScheduleManager))]
    public class ScheduleManager : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ScheduleManager";

        /// <summary>
        /// Number of milliseconds between timer ticks.
        /// </summary>
        private const int TimerInterval = 60000;

        // Events

        /// <summary>
        /// Occurs while the <see cref="ScheduleManager"/> is waiting to start at the top of the minute.
        /// </summary>
        [Category("State"),
        Description("Occurs while the ScheduleManager is waiting to start at the top of the minute.")]
        public event EventHandler Starting;

        /// <summary>
        /// Occurs when the <see cref="ScheduleManager"/> has started at the top of the minute.
        /// </summary>
        [Category("State"),
        Description("Occurs when the ScheduleManager has started at the top of the minute.")]
        public event EventHandler Started;

        /// <summary>
        /// Occurs asynchronously when a <see cref="Schedule"/> is due according to the rule specified for it.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Schedule"/> that is due.
        /// </remarks>
        [Category("Schedules"),
        Description("Occurs asynchronously when a Schedule is due according to the rule specified for it.")]
        public event EventHandler<EventArgs<Schedule>> ScheduleDue;

        /// <summary>
        /// Occurs when the a particular <see cref="Schedule"/> is being checked to see if it is due.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Schedule"/> that is being checked to see if it is due.
        /// </remarks>
        [Category("Schedules"),
        Description("Occurs when the a particular Schedule is being checked to see if it is due.")]
        public event EventHandler<EventArgs<Schedule>> ScheduleDueCheck;

        // Fields
        private bool m_persistSettings;
        private string m_settingsCategory;
        private readonly List<Schedule> m_schedules;
        private Timer m_timer;
#if ThreadTracking
        private ManagedThread m_startTimerThread;
#else
        private Thread m_startTimerThread;
#endif
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleManager"/> class.
        /// </summary>
        public ScheduleManager()
        {
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_schedules = new List<Schedule>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleManager"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ScheduleManager"/>.</param>
        public ScheduleManager(IContainer container)
            : this()
        {
            if ((object)container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="ScheduleManager"/> object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistence"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of ScheduleManager object are to be saved to the config file.")]
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
        /// Gets or sets the category under which the settings of <see cref="ScheduleManager"/> object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        [Category("Persistence"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of ScheduleManager object are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="Schedule"/> monitored by the <see cref="ScheduleManager"/> object.
        /// </summary>
        /// <remarks>
        /// Thread-safety Warning: Due to the asynchronous nature of <see cref="ScheduleManager"/>, a lock must be 
        /// obtained on <see cref="Schedules"/> before accessing it.
        /// </remarks>
        [Category("Settings"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Description("Gets a list of all Schedule monitored by the ScheduleManager object.")]
        public List<Schedule> Schedules
        {
            get
            {
                return m_schedules;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="ScheduleManager"/> is running.
        /// </summary>
        [Browsable(false)]
        public bool IsRunning
        {
            get
            {
                if ((object)m_timer != null)
                    return m_timer.Enabled;

                return false;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ScheduleManager"/> object is currently enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="Enabled"/> property is not be set by user-code directly.
        /// </remarks>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return IsRunning;
            }
            set
            {
                if (value && !Enabled)
                    Start();
                else if (!value && Enabled)
                    Stop();
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="ScheduleManager"/> object.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return m_settingsCategory;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="ScheduleManager"/> object.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append("       Number of schedules: ");
                status.Append(m_schedules.Count);
                status.AppendLine();
                lock (m_schedules)
                {
                    foreach (Schedule schedule in m_schedules)
                    {
                        status.AppendLine();
                        status.Append(schedule.Status);
                    }
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ScheduleManager"/> object.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="ScheduleManager"/> 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Performs necessary operations before the <see cref="ScheduleManager"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ScheduleManager"/> object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            if (!DesignMode)
            {
                try
                {
                    // Nothing needs to be done before component is initialized.
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Performs necessary operations after the <see cref="ScheduleManager"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ScheduleManager"/> object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                try
                {
                    Initialize();
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="ScheduleManager"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                settings.Clear();

                lock (m_schedules)
                {
                    foreach (Schedule schedule in m_schedules)
                    {
                        if (schedule.UseLocalTime)
                            settings[schedule.Name, true].Update(string.Format("rule={0}; useLocalTime={1}", schedule.Rule, schedule.UseLocalTime), schedule.Description);
                        else
                            settings[schedule.Name, true].Update(schedule.Rule, schedule.Description);
                    }
                }

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="ScheduleManager"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                foreach (CategorizedSettingsElement setting in settings)
                {
                    if ((object)setting != null && !string.IsNullOrWhiteSpace(setting.Name))
                    {
                        string value = setting.Value.ToNonNullNorWhiteSpace("* * * * *");
                        Dictionary<string, string> lookup = value.ParseKeyValuePairs();

                        // Add the schedule if it doesn't exist or update it otherwise with data from the config file.
                        Schedule existingSchedule = FindSchedule(setting.Name);

                        if ((object)existingSchedule == null)
                        {
                            // Schedule doesn't exist, so we'll add it.
                            lock (m_schedules)
                            {
                                m_schedules.Add(new Schedule(setting.Name, lookup.GetOrDefault("rule", key => value), setting.Description.ToNonNullString(), lookup.GetOrDefault("useLocalTime").ParseBoolean()));
                            }
                        }
                        else
                        {
                            // Schedule exists, so we'll update it.
                            existingSchedule.Name = setting.Name;
                            existingSchedule.Rule = lookup.GetOrDefault("rule", key => value);
                            existingSchedule.Description = setting.Description.ToNonNullString();
                            existingSchedule.UseLocalTime = lookup.GetOrDefault("useLocalTime").ParseBoolean();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="ScheduleManager"/> asynchronously if not running.
        /// </summary>
        public void Start()
        {
            if (!IsRunning && (object)m_startTimerThread == null)
            {
                // Initialize if uninitialized.
                Initialize();

                // Initialize timer that checks schedules.
                m_timer = new Timer(TimerInterval);
                m_timer.Elapsed += m_timer_Elapsed;

                // Spawn new thread to start timer at top of the minute.
#if ThreadTracking
                m_startTimerThread = new ManagedThread(StartTimer);
                m_startTimerThread.Name = "GSF.Scheduling.ScheduleManager.StartTimer()";
#else
                m_startTimerThread = new Thread(StartTimer);
                m_startTimerThread.IsBackground = true;
#endif
                m_startTimerThread.Start();
            }
        }

        /// <summary>
        /// Stops the <see cref="ScheduleManager"/> if running.
        /// </summary>
        public void Stop()
        {
            if ((object)m_timer != null)
            {
                m_timer.Elapsed -= m_timer_Elapsed;
                m_timer.Dispose();
            }
            m_timer = null;

            m_startTimerThread = null;
        }

        /// <summary>
        /// Checks all of the <see cref="Schedules"/> to determine if they are due.
        /// </summary>
        public void CheckAllSchedules()
        {
            lock (m_schedules)
            {
                foreach (Schedule schedule in m_schedules)
                {
                    OnScheduleDueCheck(new EventArgs<Schedule>(schedule));

                    // Schedule is due so raise the event.
                    if (schedule.IsDue())
                        OnScheduleDue(schedule);
                }
            }
        }

        /// <summary>
        /// Attempts to add a new <see cref="Schedule"/>.
        /// </summary>
        /// <param name="scheduleName">Name of the new <see cref="Schedule"/>.</param>
        /// <param name="scheduleRule">Rule of the new <see cref="Schedule"/>.</param>
        /// <returns>true if a new <see cref="Schedule"/> was added or an existing one was updated; otherwise false.</returns>
        public bool AddSchedule(string scheduleName, string scheduleRule)
        {
            return AddSchedule(scheduleName, scheduleRule, false);
        }

        /// <summary>
        /// Attempts to add a new <see cref="Schedule"/>.
        /// </summary>
        /// <param name="scheduleName">Name of the new <see cref="Schedule"/>.</param>
        /// <param name="scheduleRule">Rule of the new <see cref="Schedule"/>.</param>
        /// <param name="updateExisting">true to update existing <see cref="Schedule"/> with the specified <paramref name="scheduleName"/>; otherwise false.</param>
        /// <returns>true if a new <see cref="Schedule"/> was added or an existing one was updated; otherwise false.</returns>
        public bool AddSchedule(string scheduleName, string scheduleRule, bool updateExisting)
        {
            return AddSchedule(scheduleName, scheduleRule, string.Empty, updateExisting);
        }

        /// <summary>
        /// Attempts to add a new <see cref="Schedule"/>.
        /// </summary>
        /// <param name="scheduleName">Name of the new <see cref="Schedule"/>.</param>
        /// <param name="scheduleRule">Rule of the new <see cref="Schedule"/>.</param>
        /// <param name="scheduleDescription">Description of the new <see cref="Schedule"/>.</param>
        /// <returns>true if a new <see cref="Schedule"/> was added; otherwise false.</returns>
        public bool AddSchedule(string scheduleName, string scheduleRule, string scheduleDescription)
        {
            return AddSchedule(scheduleName, scheduleRule, scheduleDescription, false);
        }

        /// <summary>
        /// Attempts to add a new <see cref="Schedule"/>.
        /// </summary>
        /// <param name="scheduleName">Name of the new <see cref="Schedule"/>.</param>
        /// <param name="scheduleRule">Rule of the new <see cref="Schedule"/>.</param>
        /// <param name="scheduleDescription">Description of the new <see cref="Schedule"/>.</param>
        /// <param name="updateExisting">true to update existing <see cref="Schedule"/> with the specified <paramref name="scheduleName"/>; otherwise false.</param>
        /// <returns>true if a new <see cref="Schedule"/> was added or an existing one was updated; otherwise false.</returns>
        public bool AddSchedule(string scheduleName, string scheduleRule, string scheduleDescription, bool updateExisting)
        {
            Schedule existingSchedule = FindSchedule(scheduleName);

            if ((object)existingSchedule == null)
            {
                // Schedule doesn't exist, so we'll add it.
                lock (m_schedules)
                {
                    m_schedules.Add(new Schedule(scheduleName, scheduleRule, scheduleDescription));
                }

                return true;
            }

            // Schedule exists, we'll update if specified.
            if (updateExisting)
            {
                // Update existing schedule.
                existingSchedule.Name = scheduleName;
                existingSchedule.Rule = scheduleRule;
                existingSchedule.Description = scheduleDescription;

                return true;
            }

            // Leave existing schedule alone.
            return false;
        }

        /// <summary>
        /// Attempts to remove a <see cref="Schedule"/> with the specified name if one exists.
        /// </summary>
        /// <param name="scheduleName">Name of the <see cref="Schedule"/> to be removed.</param>
        /// <returns>true if the <see cref="Schedule"/> was removed; otherwise false.</returns>
        public bool RemoveSchedule(string scheduleName)
        {
            Schedule scheduleToRemove = FindSchedule(scheduleName);
            if ((object)scheduleToRemove != null)
            {
                // Schedule exists, so remove it.
                lock (m_schedules)
                {
                    m_schedules.Remove(scheduleToRemove);
                }
                return true;
            }

            // Can't remove schedule, since it doesn't exist.
            return false;
        }

        /// <summary>
        /// Searches for the <see cref="Schedule"/> with the specified name.
        /// </summary>
        /// <param name="scheduleName">Name of the <see cref="Schedule"/> to be obtained.</param>
        /// <returns><see cref="Schedule"/> object if a match is found; otherwise null.</returns>
        public Schedule FindSchedule(string scheduleName)
        {
            Schedule match = null;
            lock (m_schedules)
            {
                foreach (Schedule schedule in m_schedules)
                {
                    if (string.Compare(schedule.Name, scheduleName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        match = schedule;
                        break;
                    }
                }
            }

            return match;
        }

        /// <summary>
        /// Raises the <see cref="Starting"/> event.
        /// </summary>
        protected virtual void OnStarting()
        {
            if ((object)Starting != null)
                Starting(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="Started"/> event.
        /// </summary>
        protected virtual void OnStarted()
        {
            if ((object)Started != null)
                Started(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ScheduleDue"/> event.
        /// </summary>
        /// <param name="schedule"><see cref="Schedule"/> to send to <see cref="ScheduleDue"/> event.</param>
        protected virtual void OnScheduleDue(Schedule schedule)
        {
            if ((object)ScheduleDue != null)
            {
                EventArgs<Schedule> args = new EventArgs<Schedule>(schedule);

                foreach (Delegate invocationFunction in ScheduleDue.GetInvocationList())
                {
                    EventHandler<EventArgs<Schedule>> handler = invocationFunction as EventHandler<EventArgs<Schedule>>;

                    // Asynchronously invoke handlers...
                    if ((object)handler != null)
                        handler.BeginInvoke(this, args, null, null);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ScheduleDueCheck"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnScheduleDueCheck(EventArgs<Schedule> e)
        {
            if ((object)ScheduleDueCheck != null)
                ScheduleDueCheck(this, e);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ScheduleManager"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        Stop();
                        SaveSettings();
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        private void StartTimer()
        {
            while (!IsRunning && (object)m_timer != null)
            {
                OnStarting();

                if (DateTime.UtcNow.Second == 0)
                {
                    // We'll start the timer that will check the schedules at top of the minute.
                    m_timer.Start();
                    OnStarted();

                    CheckAllSchedules();
                    break;
                }

                Thread.Sleep(1000);
            }

            m_startTimerThread = null;
        }

        private void m_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckAllSchedules();
        }

        #endregion
    }
}