using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;

// James Ritchie Carroll - 2003


namespace TVA
{
	namespace Net
	{
		namespace Ftp
		{
			
			
			internal class SessionDisconnected : ISessionState
			{
				
				
				
				private Session m_host;
				private string m_server;
				private int m_port;
				private bool m_caseInsensitive;
				
				internal SessionDisconnected(Session h, bool CaseInsensitive)
				{
					
					m_port = 21;
					m_host = h;
					m_caseInsensitive = CaseInsensitive;
					
				}
				
				public string Server
				{
					get
					{
						return m_server;
					}
					set
					{
						m_server = value;
					}
				}
				
				public int Port
				{
					get
					{
						return m_port;
					}
					set
					{
						m_port = value;
					}
				}
				
				public void Connect(string UserName, string Password)
				{
					
					ControlChannel ctrl = new ControlChannel(m_host);
					
					ctrl.Server = m_server;
					ctrl.Port = m_port;
					ctrl.Connect();
					
					try
					{
						ctrl.Command("USER " + UserName);
						
						if (ctrl.LastResponse.Code == Response.UserAcceptedWaitingPass)
						{
							ctrl.Command("PASS " + Password);
						}
						
						if (ctrl.LastResponse.Code != Response.UserLoggedIn)
						{
							throw (new AuthenticationException("Failed to login.", ctrl.LastResponse));
						}
						
						m_host.State = new SessionConnected(m_host, ctrl, m_caseInsensitive);
						((SessionConnected) m_host.State).InitRootDirectory();
					}
					catch
					{
						ctrl.Close();
						throw;
					}
					
				}
				
				public Directory CurrentDirectory
				{
					get
					{
						throw (new InvalidOperationException);
					}
					set
					{
						throw (new InvalidOperationException);
					}
				}
				
				public ControlChannel ControlChannel
				{
					get
					{
						throw (new InvalidOperationException);
					}
				}
				
				public bool IsBusy
				{
					get
					{
						throw (new InvalidOperationException);
					}
				}
				
				public Directory RootDirectory
				{
					get
					{
						throw (new InvalidOperationException);
					}
				}
				
				public void AbortTransfer()
				{
					
					throw (new InvalidOperationException);
					
				}
				
				public void Close()
				{
					
					// Nothing to do - don't want to throw an error...
					
				}
				
			}
			
		}
	}
}
