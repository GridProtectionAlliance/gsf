using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Drawing;

// James Ritchie Carroll - 2003


// This FTP library is based on a similar C# library found on "The Code Project" web site written by
// Uwe Keim of Germany.  It was translated into VB with most of classes being renamed (removed Ftp prefix)
// and the namespace was changed to TVA.Ftp. Many bug fixes, additions and modifications have been made to
// this code as well as extensive testing.  Note worthy changes:  converted the C# delegates to standard
// .NET events for ease of use, made the library work with IIS based FTP servers that were in Unix mode,
// added detailed file system information for FTP files and directories (size, timestamp, etc), coverted
// FTP session into a component that could be dragged onto a design surface, created an FTP FileWatcher
// component and an FTP file system crawler based on this library - JRC
namespace TVA
{
	namespace Net
	{
		namespace Ftp
		{
			
			
			internal interface ISessionState
			{
				
				string Server{
					get;
					set;
				}
				int Port{
					get;
					set;
				}
				Directory CurrentDirectory{
					get;
					set;
				}
				Directory RootDirectory{
					get;
				}
				ControlChannel ControlChannel{
					get;
				}
				bool IsBusy{
					get;
				}
				void AbortTransfer();
				void Connect(string UserName, string Password);
				void Close();
				
			}
			
			[ToolboxBitmap(typeof(Session)), DefaultProperty("Server"), DefaultEvent("FileTransferProgress"), Description("Creates a client connection to an FTP server")]public class Session : Component
			{
				
				
				
				public delegate void BeginFileTransferEventHandler(string LocalFileName, string RemoteFileName, TransferDirection TransferDirection);
				private BeginFileTransferEventHandler BeginFileTransferEvent;
				
				public event BeginFileTransferEventHandler BeginFileTransfer
				{
					add
					{
						BeginFileTransferEvent = (BeginFileTransferEventHandler) System.Delegate.Combine(BeginFileTransferEvent, value);
					}
					remove
					{
						BeginFileTransferEvent = (BeginFileTransferEventHandler) System.Delegate.Remove(BeginFileTransferEvent, value);
					}
				}
				
				public delegate void EndFileTransferEventHandler(string LocalFileName, string RemoteFileName, TransferDirection TransferDirection, AsyncResult TransferResult);
				private EndFileTransferEventHandler EndFileTransferEvent;
				
				public event EndFileTransferEventHandler EndFileTransfer
				{
					add
					{
						EndFileTransferEvent = (EndFileTransferEventHandler) System.Delegate.Combine(EndFileTransferEvent, value);
					}
					remove
					{
						EndFileTransferEvent = (EndFileTransferEventHandler) System.Delegate.Remove(EndFileTransferEvent, value);
					}
				}
				
				public delegate void FileTransferProgressEventHandler(long TotalBytes, long TotalBytesTransfered, TransferDirection TransferDirection);
				private FileTransferProgressEventHandler FileTransferProgressEvent;
				
				public event FileTransferProgressEventHandler FileTransferProgress
				{
					add
					{
						FileTransferProgressEvent = (FileTransferProgressEventHandler) System.Delegate.Combine(FileTransferProgressEvent, value);
					}
					remove
					{
						FileTransferProgressEvent = (FileTransferProgressEventHandler) System.Delegate.Remove(FileTransferProgressEvent, value);
					}
				}
				
				public delegate void FileTransferNotificationEventHandler(AsyncResult TransferResult);
				private FileTransferNotificationEventHandler FileTransferNotificationEvent;
				
				public event FileTransferNotificationEventHandler FileTransferNotification
				{
					add
					{
						FileTransferNotificationEvent = (FileTransferNotificationEventHandler) System.Delegate.Combine(FileTransferNotificationEvent, value);
					}
					remove
					{
						FileTransferNotificationEvent = (FileTransferNotificationEventHandler) System.Delegate.Remove(FileTransferNotificationEvent, value);
					}
				}
				
				public delegate void ResponseReceivedEventHandler(string Response);
				private ResponseReceivedEventHandler ResponseReceivedEvent;
				
				public event ResponseReceivedEventHandler ResponseReceived
				{
					add
					{
						ResponseReceivedEvent = (ResponseReceivedEventHandler) System.Delegate.Combine(ResponseReceivedEvent, value);
					}
					remove
					{
						ResponseReceivedEvent = (ResponseReceivedEventHandler) System.Delegate.Remove(ResponseReceivedEvent, value);
					}
				}
				
				public delegate void CommandSentEventHandler(string Command);
				private CommandSentEventHandler CommandSentEvent;
				
				public event CommandSentEventHandler CommandSent
				{
					add
					{
						CommandSentEvent = (CommandSentEventHandler) System.Delegate.Combine(CommandSentEvent, value);
					}
					remove
					{
						CommandSentEvent = (CommandSentEventHandler) System.Delegate.Remove(CommandSentEvent, value);
					}
				}
				
				
				private bool m_caseInsensitive;
				private ISessionState m_currentState;
				private int m_waitLockTimeOut;
				
				public Session() : this(false)
				{
					
					
				}
				
				public Session(bool CaseInsensitive)
				{
					
					m_caseInsensitive = CaseInsensitive;
					m_waitLockTimeOut = 10;
					m_currentState = new SessionDisconnected(this, m_caseInsensitive);
					
				}
				
				[Browsable(true), Category("Configuration"), Description("Specify FTP server name (do not prefix with FTP://).")]public string Server
				{
					get
					{
						return m_currentState.Server;
					}
					set
					{
						m_currentState.Server = value;
					}
				}
				
				[Browsable(true), Category("Configuration"), Description("Set to True to not be case sensitive with FTP file names."), DefaultValue(false)]public bool CaseInsensitive
				{
					get
					{
						return m_caseInsensitive;
					}
					set
					{
						m_caseInsensitive = value;
					}
				}
				
				[Browsable(true), Category("Configuration"), Description("Specify FTP server post if needed."), DefaultValue(21)]public int Port
				{
					get
					{
						return m_currentState.Port;
					}
					set
					{
						m_currentState.Port = value;
					}
				}
				
				[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]public Directory CurrentDirectory
				{
					get
					{
						return m_currentState.CurrentDirectory;
					}
					set
					{
						m_currentState.CurrentDirectory = value;
					}
				}
				
				[Browsable(false)]public Directory RootDirectory
				{
					get
					{
						return m_currentState.RootDirectory;
					}
				}
				
				[Browsable(true), Category("Configuration"), Description("Specify the maximum number of seconds to wait for read lock for files to be uploaded."), DefaultValue(10)]public int WaitLockTimeout
				{
					get
					{
						return m_waitLockTimeOut;
					}
					set
					{
						m_waitLockTimeOut = value;
					}
				}
				
				public void SetCurrentDirectory(string DirectoryPath)
				{
					
					if (! IsConnected)
					{
						throw (new InvalidOperationException("You must be connected to the FTP server before you can set the current directory."));
					}
					
					if (DirectoryPath.Length > 0)
					{
						m_currentState.CurrentDirectory = new Directory(m_currentState, CaseInsensitive, DirectoryPath);
						m_currentState.CurrentDirectory.Refresh();
					}
					
				}
				
				[Browsable(false)]public ControlChannel ControlChannel
				{
					get
					{
						return m_currentState.ControlChannel;
					}
				}
				
				[Browsable(false)]public bool IsConnected
				{
					get
					{
						return (m_currentState is SessionConnected);
					}
				}
				
				[Browsable(false)]public bool IsBusy
				{
					get
					{
						return m_currentState.IsBusy;
					}
				}
				
				public void AbortTransfer()
				{
					
					m_currentState.AbortTransfer();
					
				}
				
				public void Connect(string UserName, string Password)
				{
					
					m_currentState.Connect(UserName, Password);
					
				}
				
				public void Close()
				{
					
					m_currentState.Close();
					
				}
				
				internal ISessionState State
				{
					get
					{
						return m_currentState;
					}
					set
					{
						m_currentState = value;
					}
				}
				
				internal void RaiseResponse(string response)
				{
					
					if (ResponseReceivedEvent != null)
						ResponseReceivedEvent(response);
					
				}
				
				internal void RaiseCommand(string command)
				{
					
					if (CommandSentEvent != null)
						CommandSentEvent(command);
					
				}
				
				internal void RaiseBeginFileTransfer(string LocalFileName, string RemoteFileName, TransferDirection TransferDirection)
				{
					
					if (BeginFileTransferEvent != null)
						BeginFileTransferEvent(LocalFileName, RemoteFileName, TransferDirection);
					
				}
				
				internal void RaiseEndFileTransfer(string LocalFileName, string RemoteFileName, TransferDirection TransferDirection, AsyncResult TransferResult)
				{
					
					if (EndFileTransferEvent != null)
						EndFileTransferEvent(LocalFileName, RemoteFileName, TransferDirection, TransferResult);
					
				}
				
				internal void RaiseFileTransferProgress(long TotalBytes, long TotalBytesTransfered, TransferDirection TransferDirection)
				{
					
					if (FileTransferProgressEvent != null)
						FileTransferProgressEvent(TotalBytes, TotalBytesTransfered, TransferDirection);
					
				}
				
				internal void RaiseFileTranferNotification(AsyncResult TransferResult)
				{
					
					if (FileTransferNotificationEvent != null)
						FileTransferNotificationEvent(TransferResult);
					
				}
				
			}
			
		}
	}}
