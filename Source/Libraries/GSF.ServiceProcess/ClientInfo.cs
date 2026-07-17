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
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Web.Hosting;
using GSF.Identity;
using GSF.Parsing;
using GSF.Reflection;

namespace GSF.ServiceProcess
{
    /// <summary>
    /// Represents information about a client using <see cref="ClientHelper"/> for connecting to a Windows Service that uses <see cref="ServiceHelper"/>.
    /// </summary>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ServiceHelper"/>
    public class ClientInfo
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

        private ClientInfo(Guid clientID, ApplicationType clientType, string clientName, string clientUserCredentials, string machineName, DateTime connectedAt)
        {
            ClientID = clientID;
            ClientType = clientType;
            ClientName = clientName;

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

            ClientUser = ClientType == ApplicationType.Web ?
                new GenericPrincipal(new GenericIdentity(ClientUsername ?? UserInfo.RemoteUserID), new string[] { }) :
                new GenericPrincipal(new GenericIdentity(ClientUsername ?? UserInfo.CurrentUserID), new string[] { });

            MachineName = machineName;
            ConnectedAt = connectedAt;
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

        /// <summary>
        /// Gets the length of the binary image when the <see cref="ClientInfo"/>
        /// is converted into raw binary data.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                const int GuidSize = 16;
                const int MinStringSize = sizeof(int);
                const int DateTimeSize = sizeof(long);

                string clientName = ClientName ?? string.Empty;
                string clientUserCredentials = ClientUserCredentials ?? string.Empty;
                string machineName = MachineName ?? string.Empty;
                int nameLength = Encoding.UTF8.GetByteCount(clientName);
                int userCredentialsLength = Encoding.UTF8.GetByteCount(clientUserCredentials);
                int machineNameLength = Encoding.UTF8.GetByteCount(machineName);

                return GuidSize + sizeof(int)
                    + MinStringSize + nameLength
                    + MinStringSize + userCredentialsLength
                    + MinStringSize + machineNameLength
                    + DateTimeSize;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Updates the <see cref="ClientUser"/>.
        /// </summary>
        /// <param name="user">New <see cref="IPrincipal"/> object to be assigned to <see cref="ClientUser"/>.</param>
        public void SetClientUser(IPrincipal user) => 
            ClientUser = user ?? throw new ArgumentNullException(nameof(user));

        /// <summary>
        /// Converts this <see cref="ClientInfo"/> instance into raw binary data.
        /// </summary>
        /// <param name="buffer">The buffer into which raw binary data will be written</param>
        /// <param name="startIndex">The start of the region in the buffer into which the raw binary data will be written</param>
        /// <returns>The number of bytes written to the buffer</returns>
        /// <exception cref="IndexOutOfRangeException">Insufficient bytes to serialize ClientInfo</exception>
        public int Serialize(byte[] buffer, int startIndex)
        {
            const int GuidSize = 16;
            const int MinStringSize = sizeof(int);
            const int DateTimeSize = sizeof(long);

            string clientName = ClientName ?? string.Empty;
            string clientUserCredentials = ClientUserCredentials ?? string.Empty;
            string machineName = MachineName ?? string.Empty;
            int nameLength = Encoding.UTF8.GetByteCount(clientName);
            int userCredentialsLength = Encoding.UTF8.GetByteCount(clientUserCredentials);
            int machineNameLength = Encoding.UTF8.GetByteCount(machineName);

            int idOffset = startIndex;
            int typeOffset = idOffset + GuidSize;
            int nameOffset = typeOffset + sizeof(int);
            int userCredentialsOffset = nameOffset + MinStringSize + nameLength;
            int machineNameOffset = userCredentialsOffset + MinStringSize + userCredentialsLength;
            int connectedAtOffset = machineNameOffset + MinStringSize + machineNameLength;

            if (buffer.Length - startIndex < connectedAtOffset + DateTimeSize)
                throw new IndexOutOfRangeException("Insufficient bytes to serialize ClientInfo");

            ClientID.ToRfcBytes(buffer, idOffset);
            BigEndian.CopyBytes((int)ClientType, buffer, typeOffset);
            BigEndian.CopyBytes(nameLength, buffer, nameOffset);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(clientName), 0, buffer, nameOffset + MinStringSize, nameLength);
            BigEndian.CopyBytes(userCredentialsLength, buffer, userCredentialsOffset);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(clientUserCredentials), 0, buffer, userCredentialsOffset + MinStringSize, userCredentialsLength);
            BigEndian.CopyBytes(machineNameLength, buffer, machineNameOffset);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(machineName), 0, buffer, machineNameOffset + MinStringSize, machineNameLength);
            BigEndian.CopyBytes(ConnectedAt.Ticks, buffer, connectedAtOffset);

            return connectedAtOffset + DateTimeSize - startIndex;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Creates an instance of <see cref="ClientInfo"/> from raw binary data.
        /// </summary>
        /// <param name="buffer">The byte array containing the raw bytes</param>
        /// <param name="startIndex">The index of the first byte in the region that represents the client info</param>
        /// <param name="length">The total number of bytes available for deserialization</param>
        /// <returns>An instance of the <see cref="ClientInfo"/> class.</returns>
        /// <exception cref="IndexOutOfRangeException">Insufficient bytes to deserialize <see cref="ClientInfo"/></exception>
        public static ClientInfo Deserialize(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            const int GuidSize = 16;
            const int MinStringSize = sizeof(int);
            const int DateTimeSize = sizeof(long);

            int idOffset = startIndex;
            int typeOffset = idOffset + GuidSize;
            int nameOffset = typeOffset + sizeof(int);

            if (startIndex + length < nameOffset + MinStringSize)
                throw new IndexOutOfRangeException("Insufficient bytes to deserialize ClientInfo");

            int nameLength = BigEndian.ToInt32(buffer, nameOffset);
            int userCredentialsOffset = nameOffset + MinStringSize + nameLength;

            if (startIndex + length < userCredentialsOffset + MinStringSize)
                throw new IndexOutOfRangeException("Insufficient bytes to deserialize ClientInfo");

            int userCredentialsLength = BigEndian.ToInt32(buffer, userCredentialsOffset);
            int machineNameOffset = userCredentialsOffset + MinStringSize + userCredentialsLength;

            if (startIndex + length < machineNameOffset + MinStringSize)
                throw new IndexOutOfRangeException("Insufficient bytes to deserialize ClientInfo");

            int machineNameLength = BigEndian.ToInt32(buffer, machineNameOffset);
            int connectedAtOffset = machineNameOffset + MinStringSize + machineNameLength;

            if (startIndex + length < connectedAtOffset + DateTimeSize)
                throw new IndexOutOfRangeException("Insufficient bytes to deserialize ClientInfo");

            Guid clientID = buffer.ToRfcGuid(idOffset);
            ApplicationType clientType = (ApplicationType)BigEndian.ToInt32(buffer, typeOffset);
            string clientName = Encoding.UTF8.GetString(buffer, nameOffset + MinStringSize, nameLength);
            string clientUserCredentials = Encoding.UTF8.GetString(buffer, userCredentialsOffset + MinStringSize, userCredentialsLength);
            string machineName = Encoding.UTF8.GetString(buffer, machineNameOffset + MinStringSize, machineNameLength);
            DateTime connectedAt = new DateTime(BigEndian.ToInt64(buffer, connectedAtOffset), DateTimeKind.Utc);

            return new ClientInfo(clientID, clientType, clientName, clientUserCredentials, machineName, connectedAt);
        }

        #endregion
    }
}
