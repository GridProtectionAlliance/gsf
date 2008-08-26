using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using TVA.Services;
using TVA.Collections;

//*******************************************************************************************************
//  TVA.TRO.Ssam.SsamLogger.vb - SSAM Logger
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
//  04/24/2006 - Pinal C. Patel
//       Original version of source code generated
//  08/25/2006 - Pinal C. Patel
//       Changed property ApiInstance to SsamApi and made its content serializable in the designer.
//       Removed properties Server and KeepConnectionOpen that can now be modified through the SsamApi
//       property.
//*******************************************************************************************************


namespace TVA.TRO
{
	namespace Ssam
	{
		
		/// <summary>
		/// Defines a component for logging events to the SSAM server.
		/// </summary>
		/// <remarks></remarks>
		[ToolboxBitmap(typeof(SsamLogger))]public partial class SsamLogger : TVA.Services.IServiceComponent
		{
			
			
			private bool m_enabled;
			private SsamApi m_ssamApi;
			private ProcessQueue<SsamEvent> m_eventQueue;
			
			/// <summary>
			/// Occurs when an exception is encountered when logging an event to the SSAM server.
			/// </summary>
			[Description("Occurs when an exception is encountered when logging an event to the SSAM server.")]public delegate void LogExceptionEventHandler(object Of);
			private LogExceptionEventHandler LogExceptionEvent;
			
			public event LogExceptionEventHandler LogException
			{
				add
				{
					LogExceptionEvent = (LogExceptionEventHandler) System.Delegate.Combine(LogExceptionEvent, value);
				}
				remove
				{
					LogExceptionEvent = (LogExceptionEventHandler) System.Delegate.Remove(LogExceptionEvent, value);
				}
			}
			
			
			/// <summary>
			/// Initializes a instance of TVA.TRO.Ssam.SsamLogger with the specified information.
			/// </summary>
			/// <param name="server">One of the TVA.TRO.Ssam.SsamApi.SsamServer values.</param>
			/// <param name="keepConnectionOpen">
			/// True if connection with the SSAM server is to be kept open after the first event is loggged for
			/// any consecutive events that will follow; otherwise False.
			/// </param>
			/// <remarks></remarks>
			public SsamLogger(SsamServer server, bool keepConnectionOpen) : this(new SsamApi(server, keepConnectionOpen))
			{
			}
			
			public SsamLogger(SsamApi ssamApi)
			{
				
				m_ssamApi = ssamApi;
				m_enabled = true;
				m_eventQueue = ProcessQueue<SsamEvent>.CreateSynchronousQueue(ProcessEvent);
				m_eventQueue.ProcessException += new System.EventHandler(this.m_eventQueue_ProcessException);
				m_eventQueue.RequeueOnException = true;
				m_eventQueue.Start();
				
				//This call is required by the Component Designer.
				InitializeComponent();
				
			}
			
			/// <summary>
			/// Gets or sets a boolean value indicating whether the logging of SSAM events is enabled.
			/// </summary>
			/// <value></value>
			/// <returns>True if logging of SSAM events is enabled; otherwise False.</returns>
			/// <remarks></remarks>
			[Description("Determines whether the logging of SSAM events is enabled."), Category("Behavior"), DefaultValue(typeof(bool), "True")]public bool Enabled
			{
				get
				{
					return m_enabled;
				}
				set
				{
					if (value)
					{
						m_eventQueue.Start(); // Start processing any queued events when the logger is enabled.
					}
					else
					{
						m_eventQueue.Stop(); // Stop processing any queued events when the logger is disabled.
					}
					m_enabled = value;
				}
			}
			
			/// <summary>
			/// Gets the TVA.TRO.Ssam.SsamApi inistance used for logging events to the SSAM server.
			/// </summary>
			/// <value></value>
			/// <returns>The TVA.TRO.Ssam.SsamApi inistance used for logging events to the SSAM server.</returns>
			/// <remarks></remarks>
			[Description("The TVA.TRO.Ssam.SsamApi inistance used for logging events to the SSAM server."), Category("Configuration"), TypeConverter(typeof(ExpandableObjectConverter)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]public SsamApi SsamApi
			{
				get
				{
					return m_ssamApi;
				}
			}
			
			/// <summary>
			/// Gets the TVA.Collections.ProcessQueue(Of SsamEvent) in which events are queued for logging to the
			/// SSAM server.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The TVA.Collections.ProcessQueue(Of SsamEvent) in which events are queued for logging to the
			/// SSAM server.
			/// </returns>
			/// <remarks></remarks>
			[Browsable(false)]public ProcessQueue<SsamEvent> EventQueue
			{
				get
				{
					return m_eventQueue;
				}
			}
			
			/// <summary>
			/// Creates an event with the specified information and queues it for logging to the SSAM server.
			/// </summary>
			/// <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
			/// <param name="entityType">One of the TVA.TRO.Ssam.SsamEntityType values.</param>
			/// <param name="eventType">One of the TVA.TRO.Ssam.SsamEvent.SsamEventType values.</param>
			public void LogEvent(string entityID, SsamEntityType entityType, SsamEventType eventType)
			{
				
				LogEvent(entityID, entityType, eventType, "", "", "");
				
			}
			
			/// <summary>
			/// Creates an event with the specified information and queues it for logging to the SSAM server.
			/// </summary>
			/// <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
			/// <param name="entityType">One of the TVA.TRO.Ssam.SsamEntityType values.</param>
			/// <param name="eventType">One of the TVA.TRO.Ssam.SsamEvent.SsamEventType values.</param>
			/// <param name="message">A brief description of the event (max 120 characters).</param>
			public void LogEvent(string entityID, SsamEntityType entityType, SsamEventType eventType, string message)
			{
				
				LogEvent(entityID, entityType, eventType, "", message, "");
				
			}
			
			/// <summary>
			/// Creates an event with the specified information and queues it for logging to the SSAM server.
			/// </summary>
			/// <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
			/// <param name="entityType">One of the TVA.TRO.Ssam.SsamEntityType values.</param>
			/// <param name="eventType">One of the TVA.TRO.Ssam.SsamEvent.SsamEventType values.</param>
			/// <param name="errorNumber">The error number encountered, if any, for which the event is being logged.</param>
			/// <param name="message">A brief description of the event (max 120 characters).</param>
			/// <param name="description">A detailed description of the event (max 2GB).</param>
			public void LogEvent(string entityID, SsamEntityType entityType, SsamEventType eventType, string errorNumber, string message, string description)
			{
				
				LogEvent(new SsamEvent(entityID, entityType, eventType, errorNumber, message, description));
				
			}
			
			/// <summary>
			/// Queues the specified event for logging it to the SSAM server.
			/// </summary>
			/// <param name="newEvent">The event that is to be logged to the SSAM server.</param>
			/// <remarks></remarks>
			public void LogEvent(SsamEvent newEvent)
			{
				
				// Discard the event if SSAM Logger has been disabled.
				if (m_enabled)
				{
					// SSAM Logger is enabled so queue the event for logging.
					m_eventQueue.Add(newEvent);
				}
				
			}
			
			/// <summary>
			/// This is delagate that will be invoked by the queue for processing an event in the queue.
			/// </summary>
			/// <param name="item">The event that is to be processed (logged to the SSAM server).</param>
			/// <remarks></remarks>
			private void ProcessEvent(SsamEvent item)
			{
				
				if (m_enabled)
				{
					// Process the queued events only when the SSAM Logger is enabled.
					m_ssamApi.LogEvent(item);
				}
				
			}
			
			private void m_eventQueue_ProcessException(Exception ex)
			{
				
				if (LogExceptionEvent != null)
					LogExceptionEvent(this, new GenericEventArgs<Exception>(ex));
				
			}
			
			#region " IServiceComponent Implementation "
			
			private bool m_previouslyEnabled = false;
			
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
					System.Text.StringBuilder with_1 = new StringBuilder();
					with_1.Append("                    Logger: ");
					if (this.Enabled == true)
					{
						with_1.Append("Enabled");
					}
					else if (this.Enabled == false)
					{
						with_1.Append("Disabled");
					}
					with_1.Append(Environment.NewLine);
					with_1.Append(m_eventQueue.Status[]);
					return with_1.ToString();
				}
			}
			
			public void ProcessStateChanged(string processName, Services.ProcessState newState)
			{
				
				// Ssam logger, when used as a service component, doesn't need to respond to changes in process state.
				
			}
			
			public void ServiceStateChanged(Services.ServiceState newState)
			{
				
				if (newState == TVA.Services.ServiceState.Started)
				{
					// No action required when the service is started.
				}
				else if (newState == TVA.Services.ServiceState.Stopped)
				{
					// No action required when the service is stopped.
				}
				else if (newState == TVA.Services.ServiceState.Paused)
				{
					m_previouslyEnabled = this.Enabled;
					this.Enabled = false;
				}
				else if (newState == TVA.Services.ServiceState.Resumed)
				{
					this.Enabled = m_previouslyEnabled;
				}
				else if (newState == TVA.Services.ServiceState.Shutdown)
				{
					this.Dispose();
				}
				
			}
			
			#endregion
			
		}
		
	}
	
}
