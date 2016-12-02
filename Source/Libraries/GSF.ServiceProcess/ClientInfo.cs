//******************************************************************************************************
//  ClientInfo.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/12/2006 - Pinal C. Patel
//       Generated original version of source code.
//  09/30/2008 - James R. Carroll
//       Converted to C#.
//  03/09/2009 - Pinal C. Patel
//       Edited code comments.
//  07/10/2009 - Pinal C. Patel
//       Modified to transmit serialized identity token used for authentication by the ServiceHelper.
//  07/21/2009 - Pinal C. Patel
//       Modified identity token generation to use the new ClientHelper.AuthenticationInput property.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  02/08/2010 - Pinal C. Patel
//       Corrected the assignment of ClientName property for web applications.
//  06/16/2010 - Pinal C. Patel
//       Made changes necessary to implement role-based security.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Web.Hosting;
using GSF.Identity;
using GSF.Reflection;

namespace GSF.ServiceProcess
{
    /// <summary>
    /// Represents information about a client using <see cref="ClientHelper"/> for connecting to a Windows Service that uses <see cref="ServiceHelper"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="ClientInfo"/> can be serialized and deserialized using <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/> only.
    /// </remarks>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ServiceHelper"/>
    [Serializable]
    public class ClientInfo : ISerializable
    {
        #region [ Members ]

        // Fields
        private Guid m_clientID;
        private readonly ApplicationType m_clientType;
        private readonly string m_clientName;
        private IPrincipal m_clientUser;
        private readonly string m_clientUserCredentials;
        private readonly string m_machineName;
        private DateTime m_connectedAt;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInfo"/> class.
        /// </summary>
        public ClientInfo()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInfo"/> class.
        /// </summary>
        /// <param name="parent">An <see cref="ClientHelper"/> object.</param>
        public ClientInfo(ClientHelper parent)
        {
            string clientUserName = null;

            // Initialize member variables.
            m_clientType = Common.GetApplicationType();
            m_machineName = Environment.MachineName;

            if ((object)parent != null)
                clientUserName = parent.Username;

            // Initialize user principal.
            if (m_clientType == ApplicationType.Web)
                m_clientUser = new GenericPrincipal(new GenericIdentity(clientUserName ?? UserInfo.RemoteUserID), new string[] { });
            else
                m_clientUser = new GenericPrincipal(new GenericIdentity(clientUserName ?? UserInfo.CurrentUserID), new string[] { });

            // TODO: Must validate that SSL is enabled before sending unencrypted username/password across the wire.
            // Initialize user credentials.
            if ((object)parent == null || string.IsNullOrEmpty(parent.Username) || string.IsNullOrEmpty(parent.Password))
                m_clientUserCredentials = string.Empty;
            else
                m_clientUserCredentials = $"{parent.Username}:{parent.Password}";

            // Initialize client application name.
            if (m_clientType == ApplicationType.Web)
            {
                if (HostingEnvironment.ApplicationVirtualPath == "/")
                    m_clientName = HostingEnvironment.SiteName;
                else
                    m_clientName = HostingEnvironment.ApplicationVirtualPath.ToNonNullString().Trim('/');
            }
            else
            {
                m_clientName = AssemblyInfo.EntryAssembly.Name;
            }
        }

        /// <summary>
        /// Creates a new <see cref="ClientInfo"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ClientInfo(SerializationInfo info, StreamingContext context)
        {
            // Deserialize client request fields
            m_clientID = info.GetOrDefault("clientID", Guid.Empty);
            m_clientType = info.GetOrDefault("clientType", ApplicationType.Unknown);
            m_clientName = info.GetOrDefault("clientName", "__undefined");
            m_clientUserCredentials = info.GetOrDefault("clientUserCredentials", "");

            string clientUserName = null;

            if (!string.IsNullOrEmpty(m_clientUserCredentials))
            {
                string[] parts = m_clientUserCredentials.Split(':');

                if (parts.Length == 2)
                    clientUserName = parts[0].Trim();

                if (string.IsNullOrEmpty(clientUserName))
                    clientUserName = null;
            }

            // Initialize user principal.
            if (m_clientType == ApplicationType.Web)
                m_clientUser = new GenericPrincipal(new GenericIdentity(clientUserName ?? UserInfo.RemoteUserID), new string[] { });
            else
                m_clientUser = new GenericPrincipal(new GenericIdentity(clientUserName ?? UserInfo.CurrentUserID), new string[] { });

            m_machineName = info.GetOrDefault("machineName", "__unknown");
            m_connectedAt = info.GetOrDefault("connectedAt", DateTime.UtcNow);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the identifier of the remote client application.
        /// </summary>
        public Guid ClientID
        {
            get
            {
                return m_clientID;
            }
            set
            {
                m_clientID = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="ApplicationType"/> of the remote client application.
        /// </summary>
        public ApplicationType ClientType => m_clientType;

        /// <summary>
        /// Gets the friendly name of the remote client application.
        /// </summary>
        public string ClientName => m_clientName;

        /// <summary>
        /// Gets the <see cref="IPrincipal"/> of the remote client application's user.
        /// </summary>
        public IPrincipal ClientUser => m_clientUser;

        /// <summary>
        /// Gets the credentials in 'username:password' format for authenticating the remote client application's user if a valid <see cref="ClientUser"/> is not available.
        /// </summary>
        public string ClientUserCredentials => m_clientUserCredentials;

        /// <summary>
        /// Gets the name of the machine running the remote client application.
        /// </summary>
        public string MachineName => m_machineName;

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when the remote client application connected to the <see cref="ServiceHelper"/>.
        /// </summary>
        public DateTime ConnectedAt
        {
            get
            {
                return m_connectedAt;
            }
            set
            {
                m_connectedAt = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Updates the <see cref="ClientUser"/>.
        /// </summary>
        /// <param name="user">New <see cref="IPrincipal"/> object to be assigned to <see cref="ClientUser"/>.</param>
        internal void SetClientUser(IPrincipal user)
        {
            if ((object)user == null)
                throw new ArgumentNullException(nameof(user));

            m_clientUser = user;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize client request fields
            info.AddValue("clientID", m_clientID);
            info.AddValue("clientType", m_clientType, typeof(ApplicationType));
            info.AddValue("clientName", m_clientName);
            info.AddValue("clientUserCredentials", m_clientUserCredentials);
            info.AddValue("machineName", m_machineName);
            info.AddValue("connectedAt", m_connectedAt);
        }

        #endregion
    }
}
