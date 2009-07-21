//*******************************************************************************************************
//  ClientInfo.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
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
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;
using TVA.Identity;

namespace TVA.Services
{
    /// <summary>
    /// Represents information about a client using <see cref="ClientHelper"/> for connecting to a Windows Service that uses <see cref="ServiceHelper"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="ClientInfo"/> can be serialized and deserialized using <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/> only.
    /// </remarks>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ServiceHelper"/>
    [Serializable()]
    public class ClientInfo
    {
        #region [ Members ]

        // Fields
        private Guid m_clientID;
        private DateTime m_connectedAt;
        private ApplicationType m_clientType;
        private string m_clientName;
        private string m_userName;
        private string m_machineName;
        private string m_serializedIdentityToken;
        [NonSerialized()]
        private SecurityToken m_deserializedIdentityToken;

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
            m_clientID = Guid.Empty;
            m_clientType = Common.GetApplicationType();
            m_machineName = Environment.MachineName;

            // Get the user login id.
            if (!string.IsNullOrEmpty(UserInfo.RemoteUserID))
                m_userName = UserInfo.RemoteUserID;
            else
                m_userName = UserInfo.CurrentUserID;

            // Get the type of client application.
            if (ClientType == ApplicationType.WindowsCui || ClientType == ApplicationType.WindowsGui)
                m_clientName = AppDomain.CurrentDomain.FriendlyName;
            else if (ClientType == ApplicationType.Web)
                m_clientName = HttpContext.Current.Request.ApplicationPath;

            // Initialize the serialized identity token.
            m_serializedIdentityToken = string.Empty;
            if (parent != null && parent.AuthenticationMethod != IdentityToken.None)
            {
                SecurityToken token = null;
                StringWriter stringWriter = new StringWriter();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                SerializableTokenWrapper<SecurityToken> serializer = new SerializableTokenWrapper<SecurityToken>();

                try
                {
                    // Create a token based on the selected method.
                    if (parent.AuthenticationMethod == IdentityToken.Ntlm)
                    {
                        if (!string.IsNullOrEmpty(parent.AuthenticationInput) && 
                            parent.AuthenticationInput.Contains(":"))
                        {
                            // Input format: <username>:<password>
                            string[] loginParts = parent.AuthenticationInput.Split(':');
                            token = new UsernameToken(loginParts[0], loginParts[1], PasswordOption.SendPlainText);
                        }
                    }
                    else if (parent.AuthenticationMethod == IdentityToken.Kerberos)
                    {
                        if (!string.IsNullOrEmpty(parent.AuthenticationInput) &&
                            parent.AuthenticationInput.Contains("/"))
                        {
                            // Input format: host/<machine name>
                            token = new KerberosToken(parent.AuthenticationInput, ImpersonationLevel.Impersonation);
                        }
                    }

                    // Serialize the token to XML for transportation.
                    if (token != null)
                    {
                        serializer.WriteToken(xmlTextWriter, token);
                        m_serializedIdentityToken = stringWriter.ToString();
                    }
                }
                catch
                {
                    // Identity token creation failed due to an exception.
                }
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the ID of the remote client application.
        /// </summary>
        public Guid ClientID
        {
            get { return m_clientID; }
            set { m_clientID = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when the remote client application connected to the <see cref="ServiceHelper"/>.
        /// </summary>
        public DateTime ConnectedAt
        {
            get { return m_connectedAt; }
            set { m_connectedAt = value; }
        }

        /// <summary>
        /// Gets the <see cref="ApplicationType"/> of the remote client application.
        /// </summary>
        public ApplicationType ClientType
        {
            get { return m_clientType; }
        }

        /// <summary>
        /// Gets the name of the remote client application.
        /// </summary>
        public string ClientName
        {
            get { return m_clientName; }
        }

        /// <summary>
        /// Gets the name of the user running the remote client application.
        /// </summary>
        public string UserName
        {
            get { return m_userName; }
        }

        /// <summary>
        /// Gets the name of the machine running the remote client application.
        /// </summary>
        public string MachineName
        {
            get { return m_machineName; }
        }

        /// <summary>
        /// Gets the serialized identity token used for authenticating the remote client user.
        /// </summary>
        public string SerializedIdentityToken
        {
            get { return m_serializedIdentityToken; }
        }

        /// <summary>
        /// Gets the deserialized identity token used for authenticating the remote client user.
        /// </summary>
        public SecurityToken DeserializedIdentityToken
        {
            get { return m_deserializedIdentityToken; }
        }

        #endregion

        #region [ Methods ]

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (!string.IsNullOrEmpty(SerializedIdentityToken))
            {
                try
                {
                    // Deserialize the serialized identity token.
                    XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(SerializedIdentityToken));
                    SerializableTokenWrapper<SecurityToken> deserializer = new SerializableTokenWrapper<SecurityToken>();

                    xmlTextReader.Read();
                    m_deserializedIdentityToken = deserializer.ReadToken(xmlTextReader);
                }
                catch
                {
                    // Exception may be encountered when deserializing if authentication fails.
                }
            }
        }

        #endregion
    }
}
