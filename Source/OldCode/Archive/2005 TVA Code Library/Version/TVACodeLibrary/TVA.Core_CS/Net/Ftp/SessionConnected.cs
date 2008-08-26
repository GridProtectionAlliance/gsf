using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;

// James Ritchie Carroll - 2003


namespace TVA
{
	namespace Net
	{
		namespace Ftp
		{
			
			
			internal class SessionConnected : ISessionState
			{
				
				
				
				private Session m_host;
				private ControlChannel m_ctrlChannel;
				private Directory m_root;
				private Directory m_current;
				private DataStream m_dataStream;
				private bool m_caseInsensitive;
				
				internal SessionConnected(Session h, ControlChannel ctrl, bool CaseInsensitive)
				{
					
					m_host = h;
					m_ctrlChannel = ctrl;
					m_ctrlChannel.Session = this;
					m_caseInsensitive = CaseInsensitive;
					
				}
				
				internal void InitRootDirectory()
				{
					
					m_root = new Directory(this, m_caseInsensitive, m_ctrlChannel.PWD());
					m_current = m_root;
					
				}
				
				public string Server
				{
					get
					{
						return m_ctrlChannel.Server;
					}
					set
					{
						throw (new InvalidOperationException);
					}
				}
				
				public int Port
				{
					get
					{
						return m_ctrlChannel.Port;
					}
					set
					{
						throw (new InvalidOperationException);
					}
				}
				
				public Directory RootDirectory
				{
					get
					{
						return m_root;
					}
				}
				
				public Directory CurrentDirectory
				{
					get
					{
						return m_current;
					}
					set
					{
						m_ctrlChannel.CWD(value.FullPath);
						m_current = value;
						m_current.ClearItems();
					}
				}
				
				public bool IsBusy
				{
					get
					{
						return !(m_dataStream == null);
					}
				}
				
				internal Session Host
				{
					get
					{
						return m_host;
					}
				}
				
				public ControlChannel ControlChannel
				{
					get
					{
						return m_ctrlChannel;
					}
				}
				
				// You can only aborting file transfer started by
				// BeginPutFile and BeginGetFile
				public void AbortTransfer()
				{
					
					// Save a copy of m_dataStream since it will be set
					// to null when DataStream call EndDataTransfer
					DataStream tempDataStream = m_dataStream;
					
					if (!(tempDataStream == null))
					{
						tempDataStream.Abort();
						while (! tempDataStream.IsClosed)
						{
							System.Threading.Thread.Sleep(0);
						}
					}
					
				}
				
				public void Close()
				{
					
					m_host.State = new SessionDisconnected(m_host, m_caseInsensitive);
					m_host.Server = m_ctrlChannel.Server;
					m_host.Port = m_ctrlChannel.Port;
					try
					{
						m_ctrlChannel.QUIT();
					}
					catch
					{
						return;
					}
					finally
					{
						m_ctrlChannel.Close();
					}
					
				}
				
				public void Connect(string UserName, string Password)
				{
					
					throw (new InvalidOperationException);
					
				}
				
				internal void BeginDataTransfer(DataStream stream)
				{
					
					lock(this)
					{
						if (m_dataStream != null)
						{
							throw (new DataTransferException());
						}
						
						m_dataStream = stream;
					}
					
				}
				
				internal void EndDataTransfer()
				{
					
					lock(this)
					{
						if (m_dataStream == null)
						{
							throw (new InvalidOperationException);
						}
						
						m_dataStream = null;
					}
					
				}
				
			}
			
		}
	}
}
