using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

// James Ritchie Carroll - 2003


namespace TVA
{
	namespace Net
	{
		namespace Ftp
		{
			
			
			[ToolboxBitmap(typeof(FileWatcher)), DefaultProperty("Server"), DefaultEvent("FileAdded"), Description("Monitors for file changes over an FTP session")]public class FileWatcher : Component
			{
				
				
				
				protected Session FtpSession;
				protected string FtpUserName;
				protected string FtpPassword;
				protected string WatchDirectory;
				protected System.Timers.Timer WatchTimer = new System.Timers.Timer();
				protected System.Timers.Timer RestartTimer = new System.Timers.Timer();
				protected ArrayList DirFiles = new ArrayList();
				protected ArrayList NewFiles = new ArrayList();
				private bool flgEnabled; // Determines if file watching is enabled
				private bool flgNotifyOnComplete; // Sets flag for notification time: set to True to only notify when a file is finished uploading, set to False to get an immediate notification when a new file is detected
				
				public delegate void FileAddedEventHandler(File FileReference);
				private FileAddedEventHandler FileAddedEvent;
				
				public event FileAddedEventHandler FileAdded
				{
					add
					{
						FileAddedEvent = (FileAddedEventHandler) System.Delegate.Combine(FileAddedEvent, value);
					}
					remove
					{
						FileAddedEvent = (FileAddedEventHandler) System.Delegate.Remove(FileAddedEvent, value);
					}
				}
				
				public delegate void FileDeletedEventHandler(File FileReference);
				private FileDeletedEventHandler FileDeletedEvent;
				
				public event FileDeletedEventHandler FileDeleted
				{
					add
					{
						FileDeletedEvent = (FileDeletedEventHandler) System.Delegate.Combine(FileDeletedEvent, value);
					}
					remove
					{
						FileDeletedEvent = (FileDeletedEventHandler) System.Delegate.Remove(FileDeletedEvent, value);
					}
				}
				
				public delegate void StatusEventHandler(string StatusText);
				private StatusEventHandler StatusEvent;
				
				public event StatusEventHandler Status
				{
					add
					{
						StatusEvent = (StatusEventHandler) System.Delegate.Combine(StatusEvent, value);
					}
					remove
					{
						StatusEvent = (StatusEventHandler) System.Delegate.Remove(StatusEvent, value);
					}
				}
				
				
				public delegate void InternalSessionCommandEventHandler(string Command);
				private InternalSessionCommandEventHandler InternalSessionCommandEvent;
				
				public event InternalSessionCommandEventHandler InternalSessionCommand
				{
					add
					{
						InternalSessionCommandEvent = (InternalSessionCommandEventHandler) System.Delegate.Combine(InternalSessionCommandEvent, value);
					}
					remove
					{
						InternalSessionCommandEvent = (InternalSessionCommandEventHandler) System.Delegate.Remove(InternalSessionCommandEvent, value);
					}
				}
				
				public delegate void InternalSessionResponseEventHandler(string Response);
				private InternalSessionResponseEventHandler InternalSessionResponseEvent;
				
				public event InternalSessionResponseEventHandler InternalSessionResponse
				{
					add
					{
						InternalSessionResponseEvent = (InternalSessionResponseEventHandler) System.Delegate.Combine(InternalSessionResponseEvent, value);
					}
					remove
					{
						InternalSessionResponseEvent = (InternalSessionResponseEventHandler) System.Delegate.Remove(InternalSessionResponseEvent, value);
					}
				}
				
				
				public FileWatcher()
				{
					
					flgEnabled = true;
					flgNotifyOnComplete = true;
					FtpSession = new Session(false);
					FtpSession.CommandSent += new TVA.Session.CommandSentEventHandler(Session_CommandSent);
					FtpSession.ResponseReceived += new TVA.Session.ResponseReceivedEventHandler(Session_ResponseReceived);
					
					// Define a timer to watch for new files
					WatchTimer.AutoReset = false;
					WatchTimer.Interval = 5000;
					WatchTimer.Enabled = false;
					
					// Define a timer for FTP connection in case of availability failures
					RestartTimer.AutoReset = false;
					RestartTimer.Interval = 10000;
					RestartTimer.Enabled = false;
					
				}
				
				public FileWatcher(bool CaseInsensitive, bool NotifyOnComplete) : this()
				{

					FtpSession.CaseInsensitive = CaseInsensitive;
					flgNotifyOnComplete = NotifyOnComplete;
					
				}
				
				~FileWatcher()
				{
					
					Dispose(true);
					
				}
				
				protected virtual void Dispose(bool disposing)
				{
					
					Close();
					
				}
				
				public virtual void Close()
				{
					
					DirFiles.Clear();
					NewFiles.Clear();
					WatchTimer.Enabled = false;
					RestartTimer.Enabled = false;
					CloseSession();
					GC.SuppressFinalize(this);
					
				}
				
				[Browsable(true), Category("Configuration"), Description("Specify FTP server name (do not prefix with ftp://).")]public virtual string Server
				{
					get
					{
						return FtpSession.Server;
					}
					set
					{
						FtpSession.Server = value;
					}
				}
				
				[Browsable(true), Category("Configuration"), Description("Set to True to not be case sensitive with FTP file names."), DefaultValue(false)]public bool CaseInsensitive
				{
					get
					{
						return FtpSession.CaseInsensitive;
					}
					set
					{
						FtpSession.CaseInsensitive = value;
					}
				}
				
				[Browsable(true), Category("Configuration"), Description("Specify interval in seconds to poll FTP directory for file changes."), DefaultValue(5)]public virtual int WatchInterval
				{
					get
					{
						return WatchTimer.Interval / 1000;
					}
					set
					{
						WatchTimer.Enabled = false;
						WatchTimer.Interval = value * 1000;
						WatchTimer.Enabled = flgEnabled;
					}
				}
				
				[Browsable(true), Category("Configuration"), Description("Specify FTP directory to monitor.  Leave blank to monitor initial FTP session directory."), DefaultValue("")]public virtual string Directory
				{
					get
					{
						return WatchDirectory;
					}
					set
					{
						WatchDirectory = value;
						ConnectToWatchDirectory();
						Reset();
					}
				}
				
				[Browsable(true), Category("Configuration"), Description("Set to True to only be notified of new FTP files when upload is complete.  This monitors file size changes at each WatchInterval."), DefaultValue(true)]public virtual bool NotifyOnComplete
				{
					get
					{
						return flgNotifyOnComplete;
					}
					set
					{
						flgNotifyOnComplete = value;
						Reset();
					}
				}
				
				[Browsable(true), Category("Configuration"), Description("Determines if FTP file watcher is enabled."), DefaultValue(true)]public virtual bool Enabled
				{
					get
					{
						return flgEnabled;
					}
					set
					{
						flgEnabled = value;
						Reset();
					}
				}
				
				[Browsable(false)]public virtual bool IsConnected
				{
					get
					{
						return FtpSession.IsConnected;
					}
				}
				
				public virtual void Connect(string UserName, string Password)
				{
					
					if (UserName.Length > 0)
					{
						FtpUserName = UserName;
					}
					if (Password.Length > 0)
					{
						FtpPassword = Password;
					}
					
					try
					{
						// Attempt to connect to FTP server
						FtpSession.Connect(FtpUserName, FtpPassword);
						if (StatusEvent != null)
							StatusEvent("[" + DateTime.Now+ "] FTP file watcher connected to \"ftp://" + FtpUserName + "@" + FtpSession.Server + "\"");
						ConnectToWatchDirectory();
						WatchTimer.Enabled = flgEnabled;
						
						// FTP servers can be fickle creatues, so after a successful connection we setup the
						// restart timer to reconnect every thirty minutes whether we need to or not :)
						RestartTimer.Interval = 1800000;
						RestartTimer.Enabled = true;
					}
					catch (Exception ex)
					{
						// If this fails, we'll try again in a moment.  The FTP server may be down...
						if (StatusEvent != null)
							StatusEvent("[" + DateTime.Now+ "] FTP file watcher failed to connect to \"ftp://" + FtpUserName + "@" + FtpSession.Server + "\" - trying again in 10 seconds..." + "\r\n" + "\t" + "Exception: " + ex.Message);
						RestartConnectCycle();
					}
					
				}
				
				public virtual Session NewDirectorySession()
				{
					
					// This method is just for convenience.  We can't allow the end user to use the
					// actual internal directory for sending files or other work because it is
					// constantly being refreshed/used etc., so we instead create a new FTP Session
					// based on the current internal session and watch directory information
					Session DirectorySession = new Session(FtpSession.CaseInsensitive);
					
					DirectorySession.Server = FtpSession.Server;
					DirectorySession.Connect(FtpUserName, FtpPassword);
					DirectorySession.SetCurrentDirectory(WatchDirectory);
					
					return DirectorySession;
					
				}
				
				public virtual void Reset()
				{
					
					RestartTimer.Enabled = false;
					WatchTimer.Enabled = false;
					DirFiles.Clear();
					NewFiles.Clear();
					WatchTimer.Enabled = flgEnabled;
					if (! FtpSession.IsConnected)
					{
						RestartConnectCycle();
					}
					
				}
				
				private void ConnectToWatchDirectory()
				{
					
					if (FtpSession.IsConnected)
					{
						FtpSession.SetCurrentDirectory(WatchDirectory);
						
						if (WatchDirectory.Length > 0)
						{
							if (StatusEvent != null)
								StatusEvent("[" + DateTime.Now+ "] FTP file watcher monitoring directory \"" + WatchDirectory + "\"");
						}
						else
						{
							if (StatusEvent != null)
								StatusEvent("[" + DateTime.Now+ "] No FTP file watcher directory specified - monitoring initial folder");
						}
					}
					
				}
				
				// This method is synchronized in case user sets watch interval too tight...
				[MethodImpl(MethodImplOptions.Synchronized)]private void WatchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
				{
					
					// We attempt to access the FTP Session and refresh the current directory, if this fails
					// we are going to restart the connect cycle
					try
					{
						// Refresh the file listing for the current directory
						FtpSession.CurrentDirectory.Refresh();
					}
					catch (Exception ex)
					{
						RestartConnectCycle();
						if (StatusEvent != null)
							StatusEvent("[" + DateTime.Now+ "] FTP file watcher is no longer connected to server \"" + FtpSession.Server + "\" - restarting connect cycle." + "\r\n" + "\t" + "Exception: " + ex.Message);
					}
					
					if (FtpSession != null)
					{
						if (FtpSession.IsConnected)
						{
							File File;
							File NewFile;
							Dictionary<string, File> Files = FtpSession.CurrentDirectory.Files.GetEnumerator();
							ArrayList RemovedFiles = new ArrayList();
							int intIndex;
							int x;
							
							// Check for new files
							while (Files.MoveNext())
							{
								File = Files.Current;
								
								if (flgNotifyOnComplete)
								{
									// See if any new files are finished downloading
									intIndex = NewFiles.BinarySearch(File);
									
									if (intIndex >= 0)
									{
										NewFile = (File) (NewFiles[intIndex]);
										if (NewFile.Size == File.Size)
										{
											// File size has not changed since last directory refresh, so we will
											// notify user of new file...
											DirFiles.Add(File);
											DirFiles.Sort();
											NewFiles.RemoveAt(intIndex);
											if (FileAddedEvent != null)
												FileAddedEvent(File);
										}
										else
										{
											NewFile.Size = File.Size;
										}
									}
									else if (DirFiles.BinarySearch(File) < 0)
									{
										NewFiles.Add(File);
										NewFiles.Sort();
									}
								}
								else if (DirFiles.BinarySearch(File) < 0)
								{
									// If user wants an immediate notification of new files, we'll give it to them...
									DirFiles.Add(File);
									DirFiles.Sort();
									if (FileAddedEvent != null)
										FileAddedEvent(File);
								}
							}
							
							// Check for removed files
							for (x = 0; x <= DirFiles.Count - 1; x++)
							{
								File = (File) (DirFiles[x]);
								if (FtpSession.CurrentDirectory.FindFile(File.Name) == null)
								{
									RemovedFiles.Add(x);
									if (FileDeletedEvent != null)
										FileDeletedEvent(File);
								}
							}
							
							// Remove files that have been deleted
							if (RemovedFiles.Count > 0)
							{
								RemovedFiles.Sort();
								
								// We remove items in desc order to maintain index integrity
								for (x = RemovedFiles.Count - 1; x >= 0; x--)
								{
									DirFiles.RemoveAt(RemovedFiles[x]);
								}
								
								RemovedFiles.Clear();
							}
							
							WatchTimer.Enabled = flgEnabled;
						}
						else
						{
							RestartConnectCycle();
							if (StatusEvent != null)
								StatusEvent("[" + DateTime.Now+ "] FTP file watcher is no longer connected to server \"" + FtpSession.Server + "\" - restarting connect cycle.");
						}
					}
					
				}
				
				private void CloseSession()
				{
					
					try
					{
						FtpSession.Close();
					}
					catch
					{
					}
					
				}
				
				private void RestartConnectCycle()
				{
					
					RestartTimer.Enabled = false;
					RestartTimer.Interval = 10000;
					RestartTimer.Enabled = true;
					
				}
				
				private void RestartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
				{
					
					// Attempt to close the FTP Session if it is open...
					CloseSession();
					
					// Try to reestablish connection
					WatchTimer.Enabled = false;
					Connect("", "");
					
				}
				
				private void Session_CommandSent(string Command)
				{
					
					if (InternalSessionCommandEvent != null)
						InternalSessionCommandEvent(Command);
					
				}
				
				private void Session_ResponseReceived(string Response)
				{
					
					if (InternalSessionResponseEvent != null)
						InternalSessionResponseEvent(Response);
					
				}
				
			}
			
		}
	}
}
