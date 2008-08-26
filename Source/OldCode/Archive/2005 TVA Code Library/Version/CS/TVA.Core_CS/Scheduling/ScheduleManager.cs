using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using TVA.Services;
using TVA.Configuration;
using TVA.Threading;

//*******************************************************************************************************
//  TVA.Scheduling.ScheduleManager.vb - Monitors multiples schedules defined as TVA.Scheduling.Schedule
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/01/2006 - Pinal C. Patel
//       Original version of source code generated
//  04/23/2007 - Pinal C. Patel
//       Made the schedules dictionary case-insensitive
//  04/24/2007 - Pinal C. Patel
//       Implemented the IPersistSettings and ISupportInitialize interfaces
//  05/02/2007 - Pinal C. Patel
//       Converted schedules to a list instead of a dictionary
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//       Modified the Stop() method to avoid a  null reference exception that was most likely causing
//       the IDE of the component's consumer to crash in design-mode
//
//*******************************************************************************************************



namespace TVA
{
	namespace Scheduling
	{
		
		[ToolboxBitmap(typeof(ScheduleManager)), DefaultEvent("ScheduleDue")]public partial class ScheduleManager : TVA.Services.IServiceComponent, IPersistSettings, ISupportInitialize
		{
			
			
			#region " Variables "
			
			private bool m_enabled;
			private List<Schedule> m_schedules;
			private bool m_persistSettings;
			private string m_settingsCategoryName;
			private bool m_previouslyEnabled;
			#if ThreadTracking
			private ManagedThread m_startTimerThread;
			#else
			private Thread m_startTimerThread;
			#endif
			private List<EventHandler<ScheduleEventArgs>> m_scheduleDueEventHandlerList;
			
			private System.Timers.Timer m_timer;
			
			#endregion
			
			#region " Constants "
			
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
			
			#endregion
			
			#region " Events "
			
			/// <summary>
			/// Occurs while the schedule manager is waiting to start at top of the minute.
			/// </summary>
			[Category("State")]private EventHandler StartingEvent;
			public event EventHandler Starting
			{
				add
				{
					StartingEvent = (EventHandler) System.Delegate.Combine(StartingEvent, value);
				}
				remove
				{
					StartingEvent = (EventHandler) System.Delegate.Remove(StartingEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when the schedule manager has started.
			/// </summary>
			[Category("State")]private EventHandler StartedEvent;
			public event EventHandler Started
			{
				add
				{
					StartedEvent = (EventHandler) System.Delegate.Combine(StartedEvent, value);
				}
				remove
				{
					StartedEvent = (EventHandler) System.Delegate.Remove(StartedEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when the schedule manager has stopped.
			/// </summary>
			[Category("State")]private EventHandler StoppedEvent;
			public event EventHandler Stopped
			{
				add
				{
					StoppedEvent = (EventHandler) System.Delegate.Combine(StoppedEvent, value);
				}
				remove
				{
					StoppedEvent = (EventHandler) System.Delegate.Remove(StoppedEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when the a particular schedule is being checked to see if it is due.
			/// </summary>
			[Category("Schedules")]public delegate void CheckingScheduleEventHandler(object Of);
			private CheckingScheduleEventHandler CheckingScheduleEvent;
			
			public event CheckingScheduleEventHandler CheckingSchedule
			{
				add
				{
					CheckingScheduleEvent = (CheckingScheduleEventHandler) System.Delegate.Combine(CheckingScheduleEvent, value);
				}
				remove
				{
					CheckingScheduleEvent = (CheckingScheduleEventHandler) System.Delegate.Remove(CheckingScheduleEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when a schedule is due according to the rule specified for the schedule.
			/// </summary>
			[Category("Schedules")]
			{
				add
				{
					m_scheduleDueEventHandlerList.Add(value);
				}
				
				remove
				{
					m_scheduleDueEventHandlerList.Remove(value);
				}
				
//				Warning - Custom RaiseEvent handlers not supported in C#
//				RaiseEvent(ByVal sender As Object, ByVal e As ScheduleEventArgs)
//				{
//					foreach (EventHandler<ScheduleEventArgs> handler in m_scheduleDueEventHandlerList)
//					{
//						handler.BeginInvoke(Sender, e, null, null);
//						}
						// }
					}
					
					#endregion
					
					#region " Properties "
					
					/// <summary>
					/// Gets or sets a boolean value indicating whether the schedule manager is enabled.
					/// </summary>
					/// <value></value>
					/// <returns>True if the schedule manager is enabled; otherwise False.</returns>
					[Category("Behavior"), DefaultValue(DefaultEnabled)]public bool Enabled
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
					[Category("Persistance"), DefaultValue(DefaultPersistSettings), Description("Indicates whether the component settings are to be persisted to the config file.")]public bool PersistSettings
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
					[Category("Persistance"), DefaultValue(DefaultSettingsCategoryName), Description("The category name under which the component settings are to be saved in the config file.")]public string SettingsCategoryName
					{
						get
						{
							return m_settingsCategoryName;
						}
						set
						{
							if (! string.IsNullOrEmpty(value))
							{
								m_settingsCategoryName = value;
							}
							else
							{
								throw (new ArgumentNullException("SettingsCategoryName"));
							}
						}
					}
					
					/// <summary>
					/// Gets a boolean value indicating whether the schedule manager is running.
					/// </summary>
					/// <value></value>
					/// <returns>True if the schedule manager is running; otherwise False.</returns>
					[Browsable(false)]public bool IsRunning
					{
						get
						{
							return m_timer.Enabled;
						}
					}
					
					/// <summary>
					/// Gets a list of all the schedules.
					/// </summary>
					/// <value></value>
					/// <returns>A list of the schedules.</returns>
					[Browsable(false)]public List<Schedule> Schedules
					{
						get
						{
							return m_schedules;
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
					
					[Browsable(false)]public string Name
					{
						get
						{
							return this.GetType().Name;
						}
					}
					
					[Browsable(false)]public string Status
					{
						get
						{
							System.Text.StringBuilder with_1 = new System.Text.StringBuilder();
							with_1.Append("        Number of schedules: ");
							with_1.Append(m_schedules.Count);
							with_1.AppendLine();
							with_1.AppendLine();
							foreach (Schedule schedule in m_schedules)
							{
								with_1.Append(schedule.Status);
								with_1.AppendLine();
							}
							
							return with_1.ToString();
						}
					}
					
					#endregion
					
					#region " Methods "
					
					/// <summary>
					/// Starts the schedule manager asynchronously.
					/// </summary>
					public void Start()
					{
						
						if (m_enabled && ! m_timer.Enabled)
						{
							#if ThreadTracking
							m_startTimerThread = new ManagedThread(StartTimer);
							m_startTimerThread.Name = "TVA.Scheduling.ScheduleManager.StartTimer()";
							#else
							m_startTimerThread = new Thread(new System.Threading.ThreadStart(StartTimer));
							#endif
							m_startTimerThread.Start();
						}
						
					}
					
					/// <summary>
					/// Stops the schedule manager.
					/// </summary>
					public void @Stop()
					{
						
						if (m_enabled)
						{
							if ((m_startTimerThread != null)&& m_startTimerThread.IsAlive)
							{
								m_startTimerThread.Abort();
							}
							if (m_timer.Enabled)
							{
								m_timer.Stop();
								if (StoppedEvent != null)
									StoppedEvent(this, EventArgs.Empty);
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
								if (CheckingScheduleEvent != null)
									CheckingScheduleEvent(this, new ScheduleEventArgs(schedule));
								if (schedule.IsDue())
								{
									if (ScheduleDueEvent != null) // Event raised asynchronously.
										ScheduleDueEvent(this, new ScheduleEventArgs(schedule));
								}
							}
						}
						
					}
					
					public void ProcessStateChanged(string processName, Services.ProcessState newState)
					{
						
					}
					
					public void ServiceStateChanged(Services.ServiceState newState)
					{
						
						if (newState == ServiceState.Started)
						{
							this.Start();
						}
						else if ((newState == ServiceState.Stopped) || (newState == ServiceState.Shutdown))
						{
							this.Stop();
						}
						else if (newState == ServiceState.Paused)
						{
							m_previouslyEnabled = Enabled;
							this.Enabled = false;
						}
						else if (newState == ServiceState.Resumed)
						{
							this.Enabled = m_previouslyEnabled;
						}
						else if (newState == ServiceState.Shutdown)
						{
							this.Dispose();
						}
						
					}
					
					/// <summary>
					/// Loads previously saved schedules from the config file.
					/// </summary>
					public void LoadSettings()
					{
						
						try
						{
							foreach (CategorizedSettingsElement schedule in TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName))
							{
								// Add the schedule if it doesn't exist or update it otherwise with data from the config file.
								Schedule existingSchedule = Schedules(schedule.Name);
								if (existingSchedule == null)
								{
									// Schedule doesn't exist, so we'll add it.
									m_schedules.Add(new schedule(schedule.Name, schedule.Value, schedule.Description));
								}
								else
								{
									// Schedule does exist, so we'll update it.
									existingSchedule.Name = schedule.Name;
									existingSchedule.Rule = schedule.Value;
									existingSchedule.Description = schedule.Description;
								}
							}
						}
						catch (Exception)
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
								CategorizedSettingsElementCollection with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
								with_1.Clear();
								foreach (Schedule schedule in m_schedules)
								{
									with_1.Add(schedule.Name, schedule.Rule, schedule.Description);
								}
								TVA.Configuration.Common.SaveSettings();
							}
							catch (Exception)
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
						{
							LoadSettings(); // Load settings from the config file.
						}
						
					}
					
					private void StartTimer()
					{
						
						while (true)
						{
							if (StartingEvent != null)
								StartingEvent(this, EventArgs.Empty);
							if (System.DateTime.Now.Second == 0)
							{
								// We'll start the timer that will check the schedules at top of the minute.
								m_timer.Start();
								if (StartedEvent != null)
									StartedEvent(this, EventArgs.Empty);
								CheckAllSchedules();
								
								break;
							}
							else
							{
								System.Threading.Thread.Sleep(500);
							}
						}
						
					}
					
					#endregion
					
					#region " Handlers "
					
					private void m_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
					{
						
						CheckAllSchedules();
						
					}
					
					#endregion
					
				}
				
			}
		}
