//*******************************************************************************************************
//  ScheduleManager.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
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
//  09/19/2008 - J. Ritchie Carroll
//       Convert to C#.
//  11/04/2008 - Pinal C. Patel
//       Edited code comments.
//  06/18/2009 - Pinal C. Patel
//       Fixed the implementation of Enabled property.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using TVA.Configuration;

namespace TVA.Scheduling
{
    /// <summary>
    /// Monitors multiple <see cref="Schedule"/> at an interval of one minute to check if they are due.
    /// </summary>
    /// <seealso cref="Schedule"/>
    /// <example>
    /// This example shows how to use the <see cref="ScheduleManager"/> component:
    /// <code>
    /// using System;
    /// using TVA;
    /// using TVA.Scheduling;
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
        private List<Schedule> m_schedules;
        private System.Timers.Timer m_timer;
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
            : base()
        {
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_schedules = new List<Schedule>();
            m_timer = new System.Timers.Timer(60000);
            m_timer.Elapsed += m_timer_Elapsed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleManager"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ScheduleManager"/>.</param>
        public ScheduleManager(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="ScheduleManager"/> object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
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
        [Category("Persistance"),
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
                    throw (new ArgumentNullException("value"));

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
                return m_timer.Enabled;
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
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Clear();
                lock (m_schedules)
                {
                    foreach (Schedule schedule in m_schedules)
                    {
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
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                foreach (CategorizedSettingsElement setting in settings)
                {
                    // Add the schedule if it doesn't exist or update it otherwise with data from the config file.
                    Schedule existingSchedule = FindSchedule(setting.Name);

                    if (existingSchedule == null)
                    {
                        // Schedule doesn't exist, so we'll add it.
                        lock (m_schedules)
                        {
                            m_schedules.Add(new Schedule(setting.Name, setting.Value, setting.Description));
                        }
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
        }

        /// <summary>
        /// Starts the <see cref="ScheduleManager"/> asynchronously if not running.
        /// </summary>
        public void Start()
        {
            if (m_startTimerThread == null && !m_timer.Enabled)
            {
                // Initialize if uninitialized.
                Initialize();

                // Schedule manager is not running and no active attempt to start it is in progress.
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
        /// Stops the <see cref="ScheduleManager"/> if running.
        /// </summary>
        public void Stop()
        {
            if (m_timer.Enabled)
                m_timer.Stop();

            if (m_startTimerThread != null)
            {
                m_startTimerThread.Abort();
                m_startTimerThread = null;
            }
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
            if (existingSchedule == null)
            {
                // Schedule doesn't exist, so we'll add it.
                lock (m_schedules)
                {
                    m_schedules.Add(new Schedule(scheduleName, scheduleRule, scheduleDescription));
                }
                return true;
            }
            else
            {
                // Schedule exists, we'll update if specified.
                if (updateExisting)
                {
                    // Update existing schedule.
                    existingSchedule.Name = scheduleName;
                    existingSchedule.Rule = scheduleRule;
                    existingSchedule.Description = scheduleDescription;

                    return true;
                }
                else
                {
                    // Leave existing schedule alone.
                    return false;
                }
            }
        }

        /// <summary>
        /// Attempts to remove a <see cref="Schedule"/> with the specified name if one exists.
        /// </summary>
        /// <param name="scheduleName">Name of the <see cref="Schedule"/> to be removed.</param>
        /// <returns>true if the <see cref="Schedule"/> was removed; otherwise false.</returns>
        public bool RemoveSchedule(string scheduleName)
        {
            Schedule scheduleToRemove = FindSchedule(scheduleName);
            if (scheduleToRemove != null)
            {
                // Schedule exists, so remove it.
                lock (m_schedules)
                {
                    m_schedules.Remove(scheduleToRemove);
                }
                return true;
            }
            else
            {
                // Can't remove schedule, since it doesn't exist.
                return false;
            }
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
                    if (string.Compare(schedule.Name, scheduleName, true) == 0)
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
            if (Starting != null)
                Starting(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="Started"/> event.
        /// </summary>
        protected virtual void OnStarted()
        {           
            if (Started != null)
                Started(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ScheduleDue"/> event.
        /// </summary>
        /// <param name="schedule"><see cref="Schedule"/> to send to <see cref="ScheduleDue"/> event.</param>
        protected virtual void OnScheduleDue(Schedule schedule)
        {
            if (ScheduleDue != null)
            {
                EventArgs<Schedule> args = new EventArgs<Schedule>(schedule);
                foreach (EventHandler<EventArgs<Schedule>> handler in ScheduleDue.GetInvocationList())
                {
                    // Asynchrnonously invoke handlers...
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
            if (ScheduleDueCheck != null)
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
                        SaveSettings();

                        if (m_startTimerThread != null)
                            m_startTimerThread.Abort();

                        if (m_timer != null)
                        {
                            m_timer.Elapsed -= m_timer_Elapsed;
                            m_timer.Dispose();
                        }
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        private void StartTimer()
        {
            while (true)
            {
                OnStarting();

                if (DateTime.Now.Second == 0)
                {
                    // We'll start the timer that will check the schedules at top of the minute.
                    m_timer.Start();
                    m_startTimerThread = null;
                    OnStarted();
                    
                    CheckAllSchedules();
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
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