using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.ComponentModel;
//using TVA.Common;
//using TVA.IO.Common;
using TVA.ErrorManagement;
using TVA.Threading;

//*******************************************************************************************************
//  TVA.Communication.FileClient.vb - File-based communication client
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
//  07/24/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed file data access
//
//*******************************************************************************************************


/// <summary>
/// Represents a File-based communication client.
/// </summary>
namespace TVA.Communication
{
	public partial class FileClient
	{
		
		
		#region " Member Declaration "
		
		private bool m_autoRepeat;
		private bool m_receiveOnDemand;
		private double m_receiveInterval;
		private long m_startingOffset;
		private StateInfo<FileStream> m_fileClient;
		#if ThreadTracking
		private ManagedThread m_receivingThread;
		private ManagedThread m_connectionThread;
		#else
		private Thread m_receivingThread;
		private Thread m_connectionThread;
		#endif
		private Dictionary<string, string> m_connectionData;
		private System.Timers.Timer m_receiveDataTimer;
		
		#endregion
		
		#region " Code Scope: Public "
		
		/// <summary>
		/// Initializes a instance of TVA.Communication.FileClient with the specified data.
		/// </summary>
		/// <param name="connectionString">The data that is required by the client to initialize.</param>
		public FileClient(string connectionString) : this()
		{
			
			base.ConnectionString = connectionString;
			
		}
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether receiving (reading) of data is to be repeated endlessly.
		/// </summary>
		/// <value></value>
		/// <returns>True if receiving (reading) of data is to be repeated endlessly; otherwise False.</returns>
		[Description("Indicates whether receiving (reading) of data is to be repeated endlessly."), Category("Data"), DefaultValue(typeof(bool), "False")]public bool AutoRepeat
		{
			get
			{
				return m_autoRepeat;
			}
			set
			{
				m_autoRepeat = value;
			}
		}
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether receiving (reading) of data will be initiated manually
		/// by calling ReceiveData().
		/// </summary>
		/// <value></value>
		/// <returns>True if receiving (reading) of data will be initiated manually; otherwise False.</returns>
		[Description("Indicates whether receiving (reading) of data will be initiated manually by calling ReceiveData()."), Category("Data"), DefaultValue(typeof(bool), "False")]public bool ReceiveOnDemand
		{
			get
			{
				return m_receiveOnDemand;
			}
			set
			{
				m_receiveOnDemand = value;
				if (m_receiveOnDemand)
				{
					// We'll disable receiving data at a set interval if user wants to receive data on demand.
					m_receiveInterval = - 1;
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the time in milliseconds to pause before receiving (reading) the next available set of data.
		/// </summary>
		/// <value></value>
		/// <returns>Time in milliseconds to pause before receiving (reading) the next available set of data.</returns>
		/// <remarks>Set ReceiveInterval = -1 to receive (read) data continuously without pausing.</remarks>
		[Description("Time in milliseconds to pause before receiving (reading) the next available set of data. Set ReceiveInterval = -1 to receive data continuously without pausing."), Category("Data"), DefaultValue(typeof(double), "-1")]public double ReceiveInterval
		{
			get
			{
				return m_receiveInterval;
			}
			set
			{
				if (value == - 1 || value > 0)
				{
					m_receiveInterval = value;
					if (m_receiveInterval > 0)
					{
						// We'll disable the ReceiveOnDemand feature if the user specifies an interval for
						// automatically receiving data.
						m_receiveOnDemand = false;
					}
				}
				else
				{
					throw (new ArgumentOutOfRangeException("value"));
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the starting point relative to the beginning of the file from where the data is to be received (read).
		/// </summary>
		/// <value></value>
		/// <returns>The starting point relative to the beginning of the file from where the data is to be received (read).</returns>
		[Description("The starting point relative to the beginning of the file from where the data is to be received (read)."), Category("Data"), DefaultValue(typeof(long), "0")]public long StartingOffset
		{
			get
			{
				return m_startingOffset;
			}
			set
			{
				if (value >= 0)
				{
					m_startingOffset = value;
				}
				else
				{
					throw (new ArgumentOutOfRangeException("value"));
				}
			}
		}
		
		/// <summary>
		/// Initiates receiving to data from the file.
		/// </summary>
		/// <remarks>This method is functional only when ReceiveOnDemand is enabled.</remarks>
		public void ReceiveData()
		{
			
			if (base.Enabled && base.IsConnected && m_receiveOnDemand && ! m_receivingThread.IsAlive)
			{
				#if ThreadTracking
				m_receivingThread = new ManagedThread(ReceiveFileData);
				m_receivingThread.Name = "TVA.Communication.FileClient.ReceiveFileData()";
				#else
				m_receivingThread = new Thread(new System.Threading.ThreadStart(ReceiveFileData));
				#endif
				m_receivingThread.Start();
			}
			
		}
		
		/// <summary>
		/// Cancels any active attempts of connecting to the file.
		/// </summary>
		public override void CancelConnect()
		{
			
			if (base.Enabled && m_connectionThread.IsAlive)
			{
				m_connectionThread.Abort();
			}
			
		}
		
		/// <summary>
		/// Connects to the file asynchronously.
		/// </summary>
		public override void Connect()
		{
			
			if (base.Enabled && ! base.IsConnected && ValidConnectionString(ConnectionString))
			{
				if (File.Exists(m_connectionData("file")))
				{
					#if ThreadTracking
					m_connectionThread = new ManagedThread(ConnectToFile);
					m_connectionThread.Name = "TVA.Communication.FileClient.ConnectToFile()";
					#else
					m_connectionThread = new Thread(new System.Threading.ThreadStart(ConnectToFile));
					#endif
					m_connectionThread.Start();
				}
				else
				{
					throw (new FileNotFoundException(m_connectionData("file") + " does not exist."));
				}
			}
			
		}
		
		/// <summary>
		/// Disconnects from the file (i.e., closes the file stream).
		/// </summary>
		public override void Disconnect(int timeout)
		{
			
			CancelConnect();
			
			if (base.Enabled && base.IsConnected)
			{
				m_receiveDataTimer.Stop();
				m_fileClient.Client.Close();
				OnDisconnected(EventArgs.Empty);
			}
			
		}
		
		public override void LoadSettings()
		{
			
			base.LoadSettings();
			
			if (PersistSettings)
			{
				try
				{
					TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName);
					ReceiveOnDemand = with_1.Item("ReceiveOnDemand").GetTypedValue(m_receiveOnDemand);
					ReceiveInterval = with_1.Item("ReceiveInterval").GetTypedValue(m_receiveInterval);
					StartingOffset = with_1.Item("StartingOffset").GetTypedValue(m_startingOffset);
				}
				catch (Exception)
				{
					// We'll encounter exceptions if the settings are not present in the config file.
				}
			}
			
		}
		
		public override void SaveSettings()
		{
			
			base.SaveSettings();
			
			if (PersistSettings)
			{
				try
				{
					TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName);
					object with_2 = with_1.Item("ReceiveOnDemand", true);
					with_2.Value = m_receiveOnDemand.ToString();
					with_2.Description = "True if receiving (reading) of data will be initiated manually; otherwise False.";
					object with_3 = with_1.Item("ReceiveInterval", true);
					with_3.Value = m_receiveInterval.ToString();
					with_3.Description = "Time in milliseconds to pause before receiving (reading) the next available set of data.";
					object with_4 = with_1.Item("StartingOffset", true);
					with_4.Value = m_startingOffset.ToString();
					with_4.Description = "The starting point relative to the beginning of the file from where the data is to be received (read).";
					TVA.Configuration.Common.SaveSettings();
				}
				catch (Exception)
				{
					// We might encounter an exception if for some reason the settings cannot be saved to the config file.
				}
			}
		}
		
		#endregion
		
		#region " Code Scope: Protected "
		
		[EditorBrowsable(EditorBrowsableState.Never)]protected override void SendPreparedData(byte[] data)
		{
			
			throw (new NotSupportedException());
			
		}
		
		/// <summary>
		/// Determines whether specified connection string required for connecting to the file is valid.
		/// </summary>
		/// <param name="connectionString">The connection string to be validated.</param>
		/// <returns>True is the connection string is valid; otherwise False.</returns>
		protected override bool ValidConnectionString(string connectionString)
		{
			
			if (! string.IsNullOrEmpty(connectionString))
			{
				m_connectionData = TVA.Text.Common.ParseKeyValuePairs(connectionString);
				if (m_connectionData.ContainsKey("file"))
				{
					return true;
				}
				else
				{
					// Connection string is not in the expected format.
					System.Text.StringBuilder with_1 = new StringBuilder();
					with_1.Append("Connection string must be in the following format:");
					with_1.AppendLine();
					with_1.Append("   File=[Name of the file]");
					throw (new ArgumentException(with_1.ToString()));
				}
			}
			else
			{
				throw (new ArgumentNullException());
			}
			
		}
		
		#endregion
		
		#region " Code Scope: Private "
		
		/// <summary>
		/// Connects to the file.
		/// </summary>
		/// <remarks>This method is meant to be executed on a seperate thread.</remarks>
		private void ConnectToFile()
		{
			
			int connectionAttempts = 0;
			
			while (MaximumConnectionAttempts == - 1 || connectionAttempts < MaximumConnectionAttempts)
			{
				try
				{
					OnConnecting(EventArgs.Empty);
					m_fileClient.Client = new FileStream(m_connectionData("file"), FileMode.Open);
					m_fileClient.Client.Seek(m_startingOffset, SeekOrigin.Begin); // Move to the specified offset.
					OnConnected(EventArgs.Empty);
					if (! m_receiveOnDemand)
					{
						if (m_receiveInterval > 0)
						{
							// We need to start receivng data at the specified interval.
							m_receiveDataTimer.Interval = m_receiveInterval;
							m_receiveDataTimer.Start();
						}
						else
						{
							// We need to start receiving data continuously.
							#if ThreadTracking
							m_connectionThread = new ManagedThread(ConnectToFile);
							m_connectionThread.Name = "TVA.Communication.FileClient.ConnectToFile()";
							#else
							m_connectionThread = new Thread(new System.Threading.ThreadStart(ConnectToFile));
							#endif
							m_receivingThread.Start();
						}
					}
					
					break; // We've successfully connected to the file.
				}
				catch (ThreadAbortException)
				{
					OnConnectingCancelled(EventArgs.Empty);
					break; // We must abort connecting to the file.
				}
				catch (Exception ex)
				{
					connectionAttempts++;
					OnConnectingException(ex);
				}
			}
			
		}
		
		/// <summary>
		/// Receive data from the file.
		/// </summary>
		/// <remarks>This method is meant to be executed on a seperate thread.</remarks>
		private void ReceiveFileData()
		{
			
			try
			{
				int received;
				
				// Process the entire file content
				while (m_fileClient.Client.Position < m_fileClient.Client.Length)
				{
					// Retrieve data from the file stream
					received = m_fileClient.Client.Read(m_buffer, 0, m_buffer.Length);
					
					// Post raw data to real-time function delegate if defined - this bypasses all other activity
					if (m_receiveRawDataFunction != null)
					{
						m_receiveRawDataFunction(m_buffer, 0, received);
						m_totalBytesReceived += received;
					}
					else
					{
						// Unpack data and make available via event
						OnReceivedData(new IdentifiableItem<Guid, byte>(ServerID, TVA.IO.Common.CopyBuffer(m_buffer, 0, received)));
					}
					
					// We'll re-read the file if the user wants to repeat when we're done reading the file.
					if (m_autoRepeat && m_fileClient.Client.Position == m_fileClient.Client.Length)
					{
						m_fileClient.Client.Seek(0, SeekOrigin.Begin);
					}
					
					// We must stop processing the file if user has either opted to receive data on
					// demand or receive data at a predefined interval.
					if (m_receiveOnDemand || m_receiveInterval > 0)
					{
						break;
					}
				}
			}
			catch (Exception)
			{
				// Exit gracefully when an exception is encountered while receiving data.
			}
			
		}
		
		private void m_receiveDataTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			
			if (base.Enabled && base.IsConnected && m_receiveInterval > 0 && ! m_receivingThread.IsAlive)
			{
				#if ThreadTracking
				m_receivingThread = new ManagedThread(ReceiveFileData);
				m_receivingThread.Name = "TVA.Communication.FileClient.ReceiveFileData()";
				#else
				m_receivingThread = new Thread(new System.Threading.ThreadStart(ReceiveFileData));
				#endif
				m_receivingThread.Start();
			}
			
		}
		
		#endregion
		
	}
	
}
