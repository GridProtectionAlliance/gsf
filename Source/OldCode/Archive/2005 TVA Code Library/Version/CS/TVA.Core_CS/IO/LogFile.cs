using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using TVA.Collections;
using TVA.IO.FilePath;
using TVA.Configuration;

//*******************************************************************************************************
//  TVA.IO.LogFile.vb - Log file that can be used for logging purpose
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
//  04-09-2007 - Pinal C. Patel
//       Original version of source code generated
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//
//*******************************************************************************************************



namespace TVA
{
	namespace IO
	{
		
		[ToolboxBitmap(typeof(LogFile))]public partial class LogFile : IPersistSettings, ISupportInitialize
		{


			#region " Variables "
			
			private string m_name;
			private int m_size;
			private bool m_autoOpen;
			private LogFileFullOperation m_fileFullOperation;
			private bool m_persistSettings;
			private string m_settingsCategoryName;
			private FileStream m_fileStream;
			private ManualResetEvent m_operationWaitHandle;
			private bool m_disposed;
			
			private ProcessQueue<string> m_logEntryQueue;
			
			#endregion
			
			#region " Constants "
			
			/// <summary>
			/// The minimum size for a log file.
			/// </summary>
			public const int MinimumFileSize = 1;
			
			/// <summary>
			/// The maximum size for a log file.
			/// </summary>
			public const int MaximumFileSize = 10;
			
			/// <summary>
			/// Default value for Name property.
			/// </summary>
			public const string DefaultName = "LogFile.txt";
			
			/// <summary>
			/// Default value for Size property.
			/// </summary>
			public const int DefaultSize = 3;
			
			/// <summary>
			/// Default value for AutoOpen property.
			/// </summary>
			public const bool DefaultAutoOpen = false;
			
			/// <summary>
			/// Default value for FileFullOperation property.
			/// </summary>
			public const LogFileFullOperation DefaultFileFullOperation = LogFileFullOperation.Truncate;
			
			/// <summary>
			/// Default value for PersistSettings property.
			/// </summary>
			public const bool DefaultPersistSettings = false;
			
			/// <summary>
			/// Default value for SettingsCategoryName property.
			/// </summary>
			public const string DefaultSettingsCategoryName = "LogFile";
			
			#endregion
			
			#region " Events "
			
			/// <summary>
			/// Occurs when the log file is being opened.
			/// </summary>
			[Description("Occurs when the log file is being opened.")]private EventHandler FileOpeningEvent;
			public event EventHandler FileOpening
			{
				add
				{
					FileOpeningEvent = (EventHandler) System.Delegate.Combine(FileOpeningEvent, value);
				}
				remove
				{
					FileOpeningEvent = (EventHandler) System.Delegate.Remove(FileOpeningEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when the log file has been opened.
			/// </summary>
			[Description("Occurs when the log file has been opened.")]private EventHandler FileOpenedEvent;
			public event EventHandler FileOpened
			{
				add
				{
					FileOpenedEvent = (EventHandler) System.Delegate.Combine(FileOpenedEvent, value);
				}
				remove
				{
					FileOpenedEvent = (EventHandler) System.Delegate.Remove(FileOpenedEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when the log file is being closed.
			/// </summary>
			[Description("Occurs when the log file is being closed.")]private EventHandler FileClosingEvent;
			public event EventHandler FileClosing
			{
				add
				{
					FileClosingEvent = (EventHandler) System.Delegate.Combine(FileClosingEvent, value);
				}
				remove
				{
					FileClosingEvent = (EventHandler) System.Delegate.Remove(FileClosingEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when the log file has been closed.
			/// </summary>
			[Description("Occurs when the log file has been closed.")]private EventHandler FileClosedEvent;
			public event EventHandler FileClosed
			{
				add
				{
					FileClosedEvent = (EventHandler) System.Delegate.Combine(FileClosedEvent, value);
				}
				remove
				{
					FileClosedEvent = (EventHandler) System.Delegate.Remove(FileClosedEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when the log file is full.
			/// </summary>
			[Description("Occurs when the log file is full.")]private EventHandler FileFullEvent;
			public event EventHandler FileFull
			{
				add
				{
					FileFullEvent = (EventHandler) System.Delegate.Combine(FileFullEvent, value);
				}
				remove
				{
					FileFullEvent = (EventHandler) System.Delegate.Remove(FileFullEvent, value);
				}
			}
			
			
			/// <summary>
			/// Occurs when an exception is encountered while writing entries to the log file.
			/// </summary>
			[Description("Occurs when an exception is encountered while writing entries to the log file.")]public delegate void LogExceptionEventHandler(object Of);
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
			
			
			#endregion
			
			#region " Properties "
			
			/// <summary>
			/// Gets or sets the name of the log file, including the file extension.
			/// </summary>
			/// <returns>The name of the log file, including the file extension.</returns>
			[Category("Settings"), DefaultValue(DefaultName), Description("The name of the log file, including the file extension.")]public string Name
			{
				get
				{
					return m_name;
				}
				set
				{
					if (! string.IsNullOrEmpty(value))
					{
						m_name = value;
						if (IsOpen)
						{
							Close();
							Open();
						}
					}
					else
					{
						throw (new ArgumentNullException("Name"));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets the size of the log file in MB.
			/// </summary>
			/// <returns>The size of the log file in MB.</returns>
			[Category("Settings"), DefaultValue(DefaultSize), Description("The size of the log file in MB.")]public int Size
			{
				get
				{
					return m_size;
				}
				set
				{
					if (value >= MinimumFileSize && value <= MaximumFileSize)
					{
						m_size = value;
					}
					else
					{
						throw (new ArgumentOutOfRangeException("Size", string.Format("Value must be between {0} and {1}", MinimumFileSize, MaximumFileSize)));
					}
				}
			}
			
			/// <summary>
			/// Gets or sets a boolean value indicating whether the log file is to be opened automatically after the
			/// component has finished initializing.
			/// </summary>
			/// <returns>True, if the log file is to be opened after the component has finished initializing; otherwise,
			/// false.</returns>
			[Category("Behavior"), DefaultValue(DefaultAutoOpen), Description("Indicates whether the log file is to be opened automatically after the component has finished initializing.")]public bool AutoOpen
			{
				get
				{
					return m_autoOpen;
				}
				set
				{
					m_autoOpen = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the type of operation to be performed when the log file is full.
			/// </summary>
			/// <returns>One of the TVA.IO.LogFileFullOperation values.</returns>
			[Category("Behavior"), DefaultValue(DefaultFileFullOperation), Description("The type of operation to be performed when the log file is full.")]public LogFileFullOperation FileFullOperation
			{
				get
				{
					return m_fileFullOperation;
				}
				set
				{
					m_fileFullOperation = value;
				}
			}
			
			/// <summary>
			/// Gets or sets a boolean value indicating whether the component settings are to be persisted to the config
			/// file.
			/// </summary>
			/// <returns>True, if the component settings are to be persisted to the config file; otherwise, false.</returns>
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
			/// Gets a boolean value indicating whether the log file is open.
			/// </summary>
			/// <returns>True, if the log file is open; otherwise, false.</returns>
			[Browsable(false)]public bool IsOpen
			{
				get
				{
					return (m_fileStream != null);
				}
			}
			
			#endregion
			
			#region " Methods "
			
			/// <summary>
			/// Opens the log file if it is closed.
			/// </summary>
			public void Open()
			{
				
				if (! IsOpen)
				{
					if (FileOpeningEvent != null)
						FileOpeningEvent(this, EventArgs.Empty);
					
					// Gets the absolute file path if a relative path is specified.
					m_name = AbsolutePath(m_name);
					// Creates the folder in which the log file will reside it, if it does not exist.
					if (! Directory.Exists(JustPath(m_name)))
					{
						Directory.CreateDirectory(JustPath(m_name));
					}
					// Opens the log file (if it exists) or creates it (if it does not exist).
					m_fileStream = new FileStream(m_name, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
					// Scrolls to the end of the file so that existing data is not overwritten.
					m_fileStream.Seek(0, SeekOrigin.End);
					// If this is a new log file, set its creation date to current date. This is done to prevent historic
					// log files (when FileFullOperation = Rollover) from having the same start time in their filename.
					if (m_fileStream.Length == 0)
					{
						System.IO.FileInfo with_1 = new FileInfo(m_name);
						with_1.CreationTime = DateTime.Now;
					}
					
					// Starts the queue to which log entries are going to be added.
					m_logEntryQueue.Start();
					
					if (FileOpenedEvent != null)
						FileOpenedEvent(this, EventArgs.Empty);
				}
				
			}
			
			/// <summary>
			/// Closes the log file if it is open.
			/// </summary>
			public void Close()
			{
				
				Close(true);
				
			}
			
			/// <summary>
			/// Closes the log file if it is open.
			/// </summary>
			/// <param name="flushQueuedEntries">True, if queued log entries are to be written to the log file; otherwise,
			/// false.</param>
			public void Close(bool flushQueuedEntries)
			{
				
				if (IsOpen)
				{
					if (FileClosingEvent != null)
						FileClosingEvent(this, EventArgs.Empty);
					
					if (flushQueuedEntries)
					{
						// Writes all queued log entries to the file.
						m_logEntryQueue.Flush();
					}
					else
					{
						// Stops processing the queued log entries.
						m_logEntryQueue.Stop();
					}
					
					if (m_fileStream != null)
					{
						// Closes the log file.
						m_fileStream.Dispose();
						m_fileStream = null;
					}
					
					if (FileClosedEvent != null)
						FileClosedEvent(this, EventArgs.Empty);
				}
				
			}
			
			/// <summary>
			/// Queues the text for writing to the log file.
			/// </summary>
			/// <param name="text">The text to be written to the log file.</param>
			public void Write(string text)
			{
				
				// Yields to the "file full operation" to complete, if in progress.
				m_operationWaitHandle.WaitOne();
				
				if (IsOpen)
				{
					// Queues the text for writing to the log file.
					m_logEntryQueue.Add(text);
				}
				else
				{
					throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
				}
				
			}
			
			/// <summary>
			/// Queues the text for writing to the log file.
			/// </summary>
			/// <param name="text">The text to be written to the log file.</param>
			/// <remarks>A "newline" character will automatically be appended to the text.</remarks>
			public void WriteLine(string text)
			{
				
				Write(text + Environment.NewLine);
				
			}
			
			/// <summary>
			/// Queues the text for writing to the log file.
			/// </summary>
			/// <param name="text">The text to be written to the log file.</param>
			/// <remarks>
			/// <para>A timestamp will automatically be preprended to the text.</para>
			/// <para>A "newline" character will automatically be appended to the text.</para>
			/// </remarks>
			public void WriteTimestampedLine(string text)
			{
				
				Write("[" + System.DateTime.Now.ToString() + "] " + text + Environment.NewLine);
				
			}
			
			/// <summary>
			/// Loads the component settings from the config file, if present.
			/// </summary>
			public void LoadSettings()
			{
				
				try
				{
					CategorizedSettingsElementCollection with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
					if (with_1.Count > 0)
					{
						Name = with_1.Item("Name").GetTypedValue(m_name);
						Size = with_1.Item("Size").GetTypedValue(m_size);
						AutoOpen = with_1.Item("AutoOpen").GetTypedValue(m_autoOpen);
						FileFullOperation = with_1.Item("FileFullOperation").GetTypedValue(m_fileFullOperation);
					}
				}
				catch (Exception)
				{
					// Exceptions will occur if the settings are not present in the config file.
				}
				
			}
			
			/// <summary>
			/// Saves the component settings to the config file.
			/// </summary>
			public void SaveSettings()
			{
				
				if (m_persistSettings)
				{
					try
					{
						CategorizedSettingsElementCollection with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
						with_1.Clear();
						object with_2 = with_1.Item("Name", true);
						with_2.Value = m_name;
						with_2.Description = "Name of the log file including its path.";
						object with_3 = with_1.Item("Size", true);
						with_3.Value = m_size.ToString();
						with_3.Description = "Maximum size of the log file in MB.";
						object with_4 = with_1.Item("AutoOpen", true);
						with_4.Value = m_autoOpen.ToString();
						with_4.Description = "True if the log file is to be open automatically after initialization is complete; otherwise False.";
						object with_5 = with_1.Item("FileFullOperation", true);
						with_5.Value = m_fileFullOperation.ToString();
						with_5.Description = "Operation (Truncate; Rollover) that is to be performed on the file when it is full.";
						TVA.Configuration.Common.SaveSettings();
					}
					catch (Exception)
					{
						// Exceptions may occur if the settings cannot be saved to the config file.
					}
				}
				
			}
			
			public void BeginInit()
			{
				
				// No prerequisites before the component is initialized.
				
			}
			
			public void EndInit()
			{
				
				if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
				{
					LoadSettings(); // Loads settings from the config file.
					if (m_autoOpen)
					{
						Open(); // Opens the file automatically, if specified.
					}
				}
				
			}
			
			/// <summary>
			/// Reads and returns the text from the log file.
			/// </summary>
			/// <returns>The text read from the log file.</returns>
			public string ReadText()
			{
				
				// Yields to the "file full operation" to complete, if in progress.
				m_operationWaitHandle.WaitOne();
				
				if (IsOpen)
				{
					byte[] buffer = null;
					lock(m_fileStream)
					{
						buffer = TVA.Common.CreateArray<byte>(Convert.ToInt32(m_fileStream.Length));
						m_fileStream.Seek(0, SeekOrigin.Begin);
						m_fileStream.Read(buffer, 0, buffer.Length);
					}
					
					return Encoding.Default.GetString(buffer);
				}
				else
				{
					throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
				}
				
			}
			
			/// <summary>
			/// Reads text from the log file and returns a list of lines created by seperating the text by the "newline"
			/// characters if and where present.
			/// </summary>
			/// <returns>A list of lines from the text read from the log file.</returns>
			public List<string> ReadLines()
			{
				
				return new List<string>(ReadText().Split(new string[] {Environment.NewLine}, StringSplitOptions.None));
				
			}
			
			#endregion
			
			#region " Handlers "
			
			private void WriteLogEntries(string[] items)
			{
				
				long currentFileSize = 0;
				long maximumFileSize = Convert.ToInt64(m_size * (Math.Pow(1024, 2)));
				lock(m_fileStream)
				{
					currentFileSize = m_fileStream.Length;
				}
				
				for (int i = 0; i <= items.Length - 1; i++)
				{
					if (! string.IsNullOrEmpty(items[i]))
					{
						// Write entries with text.
						byte[] buffer = Encoding.Default.GetBytes(items[i]);
						
						if (currentFileSize + buffer.Length <= maximumFileSize)
						{
							// Writes the entry.
							lock(m_fileStream)
							{
								m_fileStream.Write(buffer, 0, buffer.Length);
								m_fileStream.Flush();
							}
							currentFileSize += buffer.Length;
						}
						else
						{
							// Either truncates the file or rolls over to a new file because the current file is full.
							// Prior to acting, it requeues the entries that have not been written to the file.
							for (int j = items.Length - 1; j >= i; j--)
							{
								m_logEntryQueue.Insert(0, items[j]);
							}
							
							// Truncates file or roll over to new file.
							if (FileFullEvent != null)
								FileFullEvent(this, EventArgs.Empty);
							
							return;
						}
					}
				}
				
			}
			
			private void LogFile_FileFull(object sender, System.EventArgs e)
			{
				
				// Signals that the "file full operation" is in progress.
				m_operationWaitHandle.Reset();
				
				switch (m_fileFullOperation)
				{
					case LogFileFullOperation.Truncate:
						// Deletes the existing log entries, and makes way from new ones.
						try
						{
							Close(false);
							File.Delete(m_name);
						}
						catch (Exception)
						{
							throw;
						}
						finally
						{
							Open();
						}
						break;
					case LogFileFullOperation.Rollover:
						string historyFileName = JustPath(m_name) + NoFileExtension(m_name) + "_" + File.GetCreationTime(m_name).ToString("yyyy-MM-dd hh!mm!ss") + "_to_" + File.GetLastWriteTime(m_name).ToString("yyyy-MM-dd hh!mm!ss") + JustFileExtension(m_name);
						
						// Rolls over to a new log file, and keeps the current file for history.
						try
						{
							Close(false);
							File.Move(m_name, historyFileName);
						}
						catch (Exception)
						{
							throw;
						}
						finally
						{
							Open();
						}
						break;
				}
				
				// Signals that the "file full operation" is complete.
				m_operationWaitHandle.Set();
				
			}
			
			private void m_logEntryQueue_ProcessException(System.Exception ex)
			{
				
				if (LogExceptionEvent != null)
					LogExceptionEvent(this, new GenericEventArgs<Exception>(ex));
				
			}
			
			#endregion
			
		}
		
	}
}
