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
            // Initialize member variables.
            ClientType = Common.GetApplicationType();
            MachineName = Environment.MachineName;

            if (parent is not null)
                ClientUsername = parent.Username;

            // Initialize user principal.
            ClientUser = ClientType == ApplicationType.Web ? 
                new GenericPrincipal(new GenericIdentity(ClientUsername ?? UserInfo.RemoteUserID), new string[] { }) : 
                new GenericPrincipal(new GenericIdentity(ClientUsername ?? UserInfo.CurrentUserID), new string[] { });

            // Initialize user credentials.
            if (parent is not null && !string.IsNullOrEmpty(parent.Username) && parent.Password is not null && parent.Password.Length > 0)
                SecureClientPassword = parent.SecurePassword;

            // Initialize client application name.
            if (ClientType == ApplicationType.Web)
            {
                ClientName = HostingEnvironment.ApplicationVirtualPath == "/" ? 
                    HostingEnvironment.SiteName : 
                    HostingEnvironment.ApplicationVirtualPath.ToNonNullString().Trim('/');
            }
            else
            {
                ClientName = AssemblyInfo.EntryAssembly.Name;
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
            ClientID = info.GetOrDefault("clientID", Guid.Empty);
            ClientType = info.GetOrDefault("clientType", ApplicationType.Unknown);
            ClientName = info.GetOrDefault("clientName", "__undefined");

            string clientUserCredentials = info.GetOrDefault("clientUserCredentials", "");

            if (!string.IsNullOrEmpty(clientUserCredentials))
            {
                string[] parts = clientUserCredentials.Split(':');

                if (parts.Length == 2)
                {
                    if (!string.IsNullOrEmpty(parts[0]))
                        ClientUsername = parts[0].Trim();

                    if (!string.IsNullOrEmpty(parts[1]))
                        SecureClientPassword = parts[1].ToSecureString();
                }
            }

            // Initialize user principal.
            ClientUser = ClientType == ApplicationType.Web ? 
                new GenericPrincipal(new GenericIdentity(ClientUsername ?? UserInfo.RemoteUserID), new string[] { }) : 
                new GenericPrincipal(new GenericIdentity(ClientUsername ?? UserInfo.CurrentUserID), new string[] { });

            MachineName = info.GetOrDefault("machineName", "__unknown");
            ConnectedAt = info.GetOrDefault("connectedAt", DateTime.UtcNow);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the identifier of the remote client application.
        /// </summary>
        public Guid ClientID { get; set; }

        /// <summary>
        /// Gets the <see cref="ApplicationType"/> of the remote client application.
        /// </summary>
        public ApplicationType ClientType { get; }

        /// <summary>
        /// Gets the friendly name of the remote client application.
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// Gets the <see cref="IPrincipal"/> of the remote client application's user.
        /// </summary>
        public IPrincipal ClientUser { get; private set; }

        /// <summary>
        /// Gets the username portion of the credentials supplied by the client.
        /// </summary>
        public string ClientUsername { get; }

        /// <summary>
        /// Gets the password portion of the credentials supplied by the client.
        /// </summary>
        public string ClientPassword => SecureClientPassword.ToUnsecureString();

        /// <summary>
        /// Gets the <see cref="ClientPassword"/> in a <see cref="SecureString"/>.
        /// </summary>
        public SecureString SecureClientPassword { get; }

        /// <summary>
        /// Gets the credentials in 'username:password' format for authenticating the remote client application's user if a valid <see cref="ClientUser"/> is not available.
        /// </summary>
        public string ClientUserCredentials => $"{ClientUsername}:{SecureClientPassword.ToUnsecureString()}";

        /// <summary>
        /// Gets the name of the machine running the remote client application.
        /// </summary>
        public string MachineName { get; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when the remote client application connected to the <see cref="ServiceHelper"/>.
        /// </summary>
        public DateTime ConnectedAt { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Updates the <see cref="ClientUser"/>.
        /// </summary>
        /// <param name="user">New <see cref="IPrincipal"/> object to be assigned to <see cref="ClientUser"/>.</param>
        public void SetClientUser(IPrincipal user) => 
            ClientUser = user ?? throw new ArgumentNullException(nameof(user));

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize client request fields
            info.AddValue("clientID", ClientID);
            info.AddValue("clientType", ClientType, typeof(ApplicationType));
            info.AddValue("clientName", ClientName);
            info.AddValue("clientUserCredentials", ClientUserCredentials);
            info.AddValue("machineName", MachineName);
            info.AddValue("connectedAt", ConnectedAt);
        }

        #endregion
    }
}
