//******************************************************************************************************
//  AuthenticationRequest.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/18/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ServiceModel;
using System.Xml.Serialization;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents a subscriber authentication request.
    /// </summary>
    [XmlSerializerFormat]
    public class AuthenticationRequest
    {
        #region [ Members ]

        private string m_acronym;
        private string m_name;
        private string m_sharedSecret;
        private string m_authenticationID;
        private string m_validIPAddresses;
        private string m_key;
        private string m_iv;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the suggested subscriber acronym.
        /// </summary>
        [XmlElement]
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
            }
        }

        /// <summary>
        /// Gets or sets the suggested subscriber name.
        /// </summary>
        [XmlElement]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets the shared secret for the subscriber.
        /// </summary>
        [XmlElement]
        public string SharedSecret
        {
            get
            {
                return m_sharedSecret;
            }
            set
            {
                m_sharedSecret = value;
            }
        }

        /// <summary>
        /// Gets or sets the authentication ID for the subscriber.
        /// </summary>
        [XmlElement]
        public string AuthenticationID
        {
            get
            {
                return m_authenticationID;
            }
            set
            {
                m_authenticationID = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of valid IP addresses for the subscriber.
        /// </summary>
        [XmlElement]
        public string ValidIPAddresses
        {
            get
            {
                return m_validIPAddresses;
            }
            set
            {
                m_validIPAddresses = value;
            }
        }

        /// <summary>
        /// Gets or sets the cryptographic key for the subscriber.
        /// </summary>
        [XmlElement]
        public string Key
        {
            get
            {
                return m_key;
            }
            set
            {
                m_key = value;
            }
        }

        /// <summary>
        /// Gets or sets the cryptographic initialization vector for the subscriber.
        /// </summary>
        [XmlElement]
        public string IV
        {
            get
            {
                return m_iv;
            }
            set
            {
                m_iv = value;
            }
        }

        #endregion
    }
}
