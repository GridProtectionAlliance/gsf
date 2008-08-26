using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.ComponentModel;
//using TVA.Data.Common;
//using TVA.Configuration.Common;

//*******************************************************************************************************
//  TVA.TRO.Ssam.SsamApi.vb - SSAM API
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
//       Moved SsamServer and SsamConnectionState enumerations to Enumerations.vb.
//*******************************************************************************************************


namespace TVA.TRO
{
	namespace Ssam
	{
		
		/// <summary>
		/// Defines the mechanism for communicating with SSAM programatically.
		/// </summary>
		public class SsamApi : IDisposable
		{
			
			
			private SsamServer m_server;
			private bool m_keepConnectionOpen;
			private SsamConnectionState m_connectionState;
			private string m_devConnectionString;
			private string m_prdConnectionString;
			private SqlConnection m_connection;
			
			private const string ConfigurationElement = "Ssam";
			
			/// <summary>
			/// Initializes a default instance of TVA.TRO.Ssam.SsamApi.
			/// </summary>
			public SsamApi() : this(SsamServer.Development)
			{
			}
			
			/// <summary>
			/// Initializes a instance of TVA.TRO.Ssam.SsamApi with the specified information.
			/// </summary>
			/// <param name="server">One of the TVA.TRO.Ssam.SsamApi.SsamServer values.</param>
			public SsamApi(SsamServer server) : this(server, false)
			{
			}
			
			/// <summary>
			/// Initializes a instance of TVA.TRO.Ssam.SsamApi with the specified information.
			/// </summary>
			/// <param name="server">One of the TVA.TRO.Ssam.SsamApi.SsamServer values.</param>
			/// <param name="keepConnectionOpen">
			/// True if connection with the SSAM server is to be kept open after the first event is logged for
			/// any consecutive events that will follow; otherwise False.
			/// </param>
			public SsamApi(SsamServer server, bool keepConnectionOpen)
			{
				m_server = server;
				m_keepConnectionOpen = keepConnectionOpen;
				m_connectionState = SsamConnectionState.Closed;
				m_connection = new SqlConnection();
				m_devConnectionString = "Server=RGOCSQLD;Database=Ssam;Trusted_Connection=True;";
				m_prdConnectionString = "Server=OPSSAMSQL;Database=Ssam;Trusted_Connection=True;";
				// We'll try to load the connection string from the config file if can, or else use the default ones.
				try
				{
					m_devConnectionString = TVA.Configuration.Common.get_CategorizedSettings("Development").Value;
					m_prdConnectionString = TVA.Configuration.Common.get_CategorizedSettings("Production").Value;
				}
				catch (Exception)
				{
					// We can safely ignore any exception encountered while retrieving connection string from the config file.
				}
			}
			
			/// <summary>
			/// Gets or sets the SSAM server with which we are connected.
			/// </summary>
			/// <value></value>
			/// <returns>The SSAM server with which we are connected.</returns>
			[Category("Configuration"), Description("The SSAM server to which event are to be logged."), DefaultValue(typeof(SsamServer), "Development")]public SsamServer Server
			{
				get
				{
					return m_server;
				}
				set
				{
					// If connection to a SSAM server is open, disconnect from it and connect to the selected server.
					if (m_connectionState != SsamConnectionState.Closed)
					{
						Disconnect();
						Connect();
					}
					m_server = value;
				}
			}
			
			/// <summary>
			/// Gets or sets a boolean value indicating whether connection with the SSAM server is to be kept open after
			/// the first event is logged for any consecutive events that will follow.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// True if connection with the SSAM server is to be kept open after the first event is logged for any
			/// consecutive events that will follow; otherwise False.
			/// </returns>
			[Category("Configuration"), Description("Determines whether the connection with SSAM server is to be kept open after logging an event."), DefaultValue(typeof(bool), "False")]public bool KeepConnectionOpen
			{
				get
				{
					return m_keepConnectionOpen;
				}
				set
				{
					// We will only close the connection with SSAM if it is already open, but is not to be
					// kept open after logging an event. However, if the connection is to be kept open after
					// logging an event, but is closed at present, we will only open it when the first event
					// is logged and keep if open until it is explicitly closed.
					if (value == false && m_connectionState != SsamConnectionState.Closed)
					{
						Disconnect();
					}
					m_keepConnectionOpen = value;
				}
			}
			
			/// <summary>
			/// Gets the current state of the connection with SSAM server.
			/// </summary>
			/// <value></value>
			/// <returns>The current state of the connection with SSAM server.</returns>
			[Browsable(false)]public SsamConnectionState ConnectionState
			{
				get
				{
					if (m_connection.State != System.Data.ConnectionState.Closed)
					{
						try
						{
							// Verify that connection with the SSAM database is active by switching to
							// the SSAM database. This will raise an exception if the operation cannot
							// be performed due to network or database server issues.
							m_connection.ChangeDatabase(m_connection.Database);
						}
						catch (Exception)
						{
							m_connectionState = SsamConnectionState.Closed;
						}
					}
					return m_connectionState;
				}
				private set
				{
					m_connectionState = value;
				}
			}
			
			/// <summary>
			/// Connects with the selected SSAM server.
			/// </summary>
			public void Connect()
			{
				
				CheckDisposed();
				
				if (m_connection.State != System.Data.ConnectionState.Closed)
				{
					Disconnect();
				}
				m_connection.ConnectionString = this.ConnectionString;
				m_connection.Open();
				m_connectionState = SsamConnectionState.OpenAndInactive;
				
			}
			
			/// <summary>
			/// Disconnects from the connected SSAM server.
			/// </summary>
			public void Disconnect()
			{
				
				CheckDisposed();
				
				if (m_connection.State != System.Data.ConnectionState.Closed)
				{
					m_connection.Close();
				}
				
				try
				{
					// Save SSAM connection strings to the config file.
					TVA.Configuration.CategorizedSettingsElement with_1 = object@);TVA.Configuration.Common.get_CategorizedSettings(ConfigurationElement);
					with_1.Add("Development", m_devConnectionString, "Connection string for connecting to development SSAM server.", true);
					with_1.Add("Production", m_prdConnectionString, "Connection string for connecting to production SSAM server.", true);
					TVA.Configuration.Common.SaveSettings();
				}
				catch (Exception)
				{
					// We can safely ignore any exceptions encountered while saving connections strings to the config file.
				}
				
				m_connectionState = SsamConnectionState.Closed;
				
			}
			
			/// <summary>
			/// Logs an event with the specified information and queues it for logging to the SSAM server.
			/// </summary>
			/// <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
			/// <param name="entityType">One of the TVA.TRO.Ssam.SsamEntityType values.</param>
			/// <param name="eventType">One of the TVA.TRO.Ssam.SsamEvent.SsamEventType values.</param>
			public void LogEvent(string entityID, SsamEntityType entityType, SsamEventType eventType)
			{
				
				LogEvent(entityID, entityType, eventType, "", "", "");
				
			}
			
			/// <summary>
			/// Logs an event with the specified information and queues it for logging to the SSAM server.
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
			/// Logs an event with the specified information and queues it for logging to the SSAM server.
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
			/// Logs the specified TVA.TRO.Ssam.SsamEvent to the current SSAM server.
			/// </summary>
			/// <param name="newEvent">The TVA.TRO.Ssam.SsamEvent to log.</param>
			/// <returns>True if the event is logged successfully; otherwise False.</returns>
			public bool LogEvent(SsamEvent newEvent)
			{
				
				CheckDisposed();
				
				if (! string.IsNullOrEmpty(newEvent.EntityId))
				{
					// We have a valid SSAM entity ID, so we'll go ahead and log the event.
					try
					{
						if (this.ConnectionState == SsamConnectionState.Closed)
						{
							Connect();
						}
						m_connectionState = SsamConnectionState.OpenAndActive;
						
						// Log the event to SSAM.
						TVA.Data.Common.ExecuteScalar("sp_LogSsamEvent", m_connection, new object[] {newEvent.EventType, newEvent.EntityType, newEvent.EntityId, newEvent.ErrorNumber, newEvent.Message, newEvent.Description});
						
						m_connectionState = SsamConnectionState.OpenAndInactive;
						return true;
					}
					catch (Exception)
					{
						throw;
					}
					finally
					{
						if (! m_keepConnectionOpen)
						{
							// Connection with SSAM is not to be kept open after logging an event, so close it.
							Disconnect();
						}
					}
				}
				
				return false;
			}
			
			/// <summary>
			/// Gets the connection string used for connecting with the current SSAM server.
			/// </summary>
			/// <value></value>
			/// <returns>The connection string used for connecting with the current SSAM server.</returns>
			/// <remarks>
			/// This property must be accessible only by components inheriting from this component that exists in this
			/// assembly. This is done so that the database connection string is not exposed even when a component that
			/// exists outside of this assembly inherits from this component.
			/// </remarks>
			protected internal string ConnectionString
			{
				get
				{
					// Return the connection string for connecting to the selected SSAM server.
					switch (m_server)
					{
						case SsamServer.Development:
							return m_devConnectionString;
						case SsamServer.Production:
							return m_prdConnectionString;
						default:
							return "";
					}
				}
			}
			
			/// <summary>
			/// Attempts to free resources and perform other cleanup operations before the TVA.TRO.Ssam.SsamApi
			/// is reclaimed by garbage collection.
			/// </summary>
			~SsamApi()
			{
				
				Dispose(false);
				
			}
			
			#region " IDisposable Implementation "
			
			private bool m_disposed = false;
			
			/// <summary>
			/// This is a helper method that check whether the object is disposed and raises a System.ObjectDisposedException if it is.
			/// </summary>
			/// <remarks></remarks>
			protected void CheckDisposed()
			{
				
				if (m_disposed)
				{
					throw (new ObjectDisposedException(this.GetType().FullName));
				}
				
			}
			
			/// <summary>
			/// Releases the unmanaged resources used by the TVA.TRO.Ssam.SsamApi and and optionally releases the
			/// managed resources.
			/// </summary>
			/// <param name="disposing">
			/// True to release both managed and unmanaged resources; False to release only unmanaged resources.
			/// </param>
			/// <remarks></remarks>
			protected virtual void Dispose(bool disposing)
			{
				if (! m_disposed) // Object jas not been disposed yet.
				{
					if (disposing)
					{
						Disconnect(); // Close connection with the SSAM server.
					}
				}
				m_disposed = true; // Mark the object as been disposed.
			}
			
			#region " IDisposable Support "
			/// <summary>
			/// Releases all resources used by the TVA.TRO.Ssam.SsamApi.
			/// </summary>
			/// <remarks></remarks>
			public void Dispose()
			{
				// Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			#endregion
			
			#endregion
			
		}
		
	}
}
