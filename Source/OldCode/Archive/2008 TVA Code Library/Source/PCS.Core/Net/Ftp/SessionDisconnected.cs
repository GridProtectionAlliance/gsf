//*******************************************************************************************************
//  SessionDisconnected.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/22/2003 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.Net.Ftp
{
    internal class SessionDisconnected : ISessionState
    {
        #region [ Members ]

        // Fields
        private Session m_host;
        private string m_server;
        private int m_port;
        private bool m_caseInsensitive;

        #endregion

        #region [ Constructors ]

        internal SessionDisconnected(Session h, bool caseInsensitive)
        {
            m_port = 21;
            m_host = h;
            m_caseInsensitive = caseInsensitive;
        }

        #endregion

        #region [ Properties ]

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

        public Directory CurrentDirectory
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public ControlChannel ControlChannel
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public bool IsBusy
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public Directory RootDirectory
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        #endregion

        #region [ Methods ]

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
                    ctrl.Command("PASS " + Password);

                if (ctrl.LastResponse.Code != Response.UserLoggedIn)
                    throw new AuthenticationException("Failed to login.", ctrl.LastResponse);

                m_host.State = new SessionConnected(m_host, ctrl, m_caseInsensitive);
                ((SessionConnected)m_host.State).InitRootDirectory();
            }
            catch
            {
                ctrl.Close();
                throw;
            }
        }


        public void AbortTransfer()
        {
            throw new InvalidOperationException();
        }

        public void Close()
        {
            // Nothing to do - don't want to throw an error...
        }

        #endregion
    }
}