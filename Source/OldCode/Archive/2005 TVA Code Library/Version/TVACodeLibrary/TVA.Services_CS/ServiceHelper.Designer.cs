using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.ComponentModel;


namespace TVA.Services
{
	public partial class ServiceHelper : System.ComponentModel.Component
	{
		
		
		[System.Diagnostics.DebuggerNonUserCode()]public ServiceHelper(System.ComponentModel.IContainer Container) : this()
		{
			
			//Required for Windows.Forms Class Composition Designer support
			Container.Add(this);
			
		}
		
		[System.Diagnostics.DebuggerNonUserCode()]public ServiceHelper()
		{
			
			//This call is required by the Component Designer.
			InitializeComponent();
			
			m_logStatusUpdates = DefaultLogStatusUpdates;
			m_monitorServiceHealth = DefaultMonitorServiceHealth;
			m_requestHistoryLimit = DefaultRequestHistoryLimit;
			m_queryableSettingsCategories = DefaultQueryableSettingsCategories;
			m_persistSettings = DefaultPersistSettings;
			m_settingsCategoryName = DefaultSettingsCategoryName;
			m_pursip = "s3cur3";
			m_processes = new List<ServiceProcess>();
			m_connectedClients = new List<ClientInfo>();
			m_clientRequestHistory = new List<ClientRequestInfo>();
			m_serviceComponents = new List<IServiceComponent>();
			m_clientRequestHandlers = new List<ClientRequestHandlerInfo>();
			// Components
			m_statusLog = new TVA.IO.LogFile();
			m_statusLog.LogException += new System.EventHandler`1[[TVA.GenericEventArgs`1[[System.Exception, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_statusLog_LogException);
			m_scheduler = new TVA.Scheduling.ScheduleManager();
			m_scheduler.ScheduleDue += new System.EventHandler`1[[TVA.Scheduling.ScheduleEventArgs, TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_scheduler_ScheduleDue);
			m_exceptionLogger = new TVA.ErrorManagement.GlobalExceptionLogger();
			m_statusLog.Name = "StatusLog.txt";
			m_statusLog.PersistSettings = true;
			m_statusLog.SettingsCategoryName = "StatusLog";
			m_scheduler.PersistSettings = true;
			m_scheduler.SettingsCategoryName = "Scheduler";
			m_exceptionLogger.ExitOnUnhandledException = true;
			m_exceptionLogger.PersistSettings = true;
			m_exceptionLogger.SettingsCategoryName = "ExceptionLogger";
			
		}
		
		//Component overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				SaveSettings(); // Saves settings to the config file.
				m_statusLog.Dispose();
				m_scheduler.Dispose();
				m_exceptionLogger.Dispose();
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
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			
		}
		
	}
}
