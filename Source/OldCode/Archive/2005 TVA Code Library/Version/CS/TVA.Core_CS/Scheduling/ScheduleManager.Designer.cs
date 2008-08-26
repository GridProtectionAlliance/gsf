using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

namespace TVA
{
	namespace Scheduling
	{
		
		public partial class ScheduleManager : System.ComponentModel.Component
		{
			
			
			[System.Diagnostics.DebuggerNonUserCode()]public ScheduleManager(System.ComponentModel.IContainer Container) : this()
			{
				
				//Required for Windows.Forms Class Composition Designer support
				Container.Add(this);
				
			}
			
			[System.Diagnostics.DebuggerNonUserCode()]public ScheduleManager()
			{
				
				//This call is required by the Component Designer.
				InitializeComponent();
				
				m_enabled = DefaultEnabled;
				m_persistSettings = DefaultPersistSettings;
				m_settingsCategoryName = DefaultSettingsCategoryName;
				
				m_timer = new System.Timers.Timer(60000);
				m_timer.Elapsed += new System.Timers.ElapsedEventHandler(m_timer_Elapsed);
				m_schedules = new List<Schedule>();
				m_scheduleDueEventHandlerList = new List<EventHandler<ScheduleEventArgs>>();
				
			}
			
			//Component overrides dispose to clean up the component list.
			[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
			{
				@Stop(); // Stop the schedule manager.
				SaveSettings(); // Saves settings to the config file.
				if (disposing && (components != null))
				{
					components.Dispose();
				}
				base.Dispose(disposing);
			}
			
			//Required by the Component Designer
			private System.ComponentModel.Container components = null;
			
			//NOTE: The following procedure is required by the Component Designer
			//It can be modified using the Component Designer.
			//Do not modify it using the code editor.
			[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
			{
				components = new System.ComponentModel.Container();
			}
			
		}
		
	}
}
