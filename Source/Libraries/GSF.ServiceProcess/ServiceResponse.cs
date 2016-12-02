//******************************************************************************************************
//  ServiceResponse.cs - Gbtc
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
//  08/29/2006 - Pinal C. Patel
//       Generated original version of source code.
//  09/30/2008 - J. Ritchie Carroll
//       Converted to C#.
//  03/09/2009 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;

namespace GSF.ServiceProcess
{
    /// <summary>
    /// Represents a response sent by the <see cref="ServiceHelper"/> to a <see cref="ClientRequest"/> from the <see cref="ClientHelper"/>.
    /// </summary>
    /// <seealso cref="ServiceHelper"/>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ClientRequest"/>
    [Serializable]
    public class ServiceResponse : ISerializable
    {
        #region [ Members ]

        // Fields
        private string m_type;
        private string m_message;
        private readonly List<object> m_attachments;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ClientRequest"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ServiceResponse(SerializationInfo info, StreamingContext context)
        {
            // Deserialize client request fields
            m_type = info.GetOrDefault("type", "");
            m_message = info.GetOrDefault("message", "");
            m_attachments = new List<object>();

            int attachmentCount = info.GetOrDefault("attachmentCount", 0);

            for (int i = 0; i < attachmentCount; i++)
            {
                m_attachments.Add(info.GetOrDefault("attachment" + i, null as object));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResponse"/> class.
        /// </summary>
        public ServiceResponse()
            : this("UNDETERMINED")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResponse"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="ServiceResponse"/> in plain text.</param>
        public ServiceResponse(string type)
            : this(type, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResponse"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="ServiceResponse"/> in plain-text.</param>
        /// <param name="message">Message associated with the <see cref="ServiceResponse"/>.</param>
        public ServiceResponse(string type, string message)
        {
            m_type = type.ToUpper();
            m_message = message;
            m_attachments = new List<object>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the plain-text type of the <see cref="ServiceResponse"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is either a null or empty string.</exception>
        public string Type
        {
            get
            {
                return m_type;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_type = value.ToUpper();
            }
        }

        /// <summary>
        /// Gets or sets the plain-text message associated with the <see cref="ServiceResponse"/>.
        /// </summary>
        public string Message
        {
            get
            {
                return m_message;
            }
            set
            {
                m_message = value;
            }
        }

        /// <summary>
        /// Gets a list of serializable attachments of the <see cref="ServiceResponse"/>.
        /// </summary>
        public List<object> Attachments => m_attachments;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize client request fields
            info.AddValue("type", m_type);
            info.AddValue("message", m_message);
            info.AddValue("attachmentCount", m_attachments.Count);

            for (int i = 0; i < m_attachments.Count; i++)
            {
                info.AddValue("attachment" + i, m_attachments[i]);
            }
        }

        #endregion
    }
}
