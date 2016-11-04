//******************************************************************************************************
//  ClientRequest.cs - Gbtc
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
//       Original version of source code generated.
//  04/27/2007 - Pinal C. Patel
//       Added Attachments property for clients to send serializable objects as part of the request.
//  09/30/2008 - J. Ritchie Carroll
//       Converted to C#.
//  03/09/2009 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/14/2010 - Pinal C. Patel
//       Overrode ToString() method to provide a text representation of ClientRequest.
//       Recoded static Parse() method to make it more robust.
//  01/24/201 - Pinal C. Patel
//       Modified ToString() remove leading and trailing white spaces.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using GSF.Console;

namespace GSF.ServiceProcess
{
    /// <summary>
    /// Represents a request sent by <see cref="ClientHelper"/> to <see cref="ServiceHelper"/>.
    /// </summary>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ServiceHelper"/>
    [Serializable]
    public class ClientRequest : ISerializable
    {
        #region [ Members ]

        // Fields
        private string m_command;
        private Arguments m_arguments;
        private readonly List<object> m_attachments;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRequest"/> class.
        /// </summary>
        public ClientRequest()
            : this("UNDEFINED")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRequest"/> class.
        /// </summary>
        /// <param name="command">Command text for the <see cref="ClientRequest"/>.</param>
        public ClientRequest(string command)
            : this(command, new Arguments(""))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRequest"/> class.
        /// </summary>
        /// <param name="command">Command text for the <see cref="ClientRequest"/>.</param>
        /// <param name="arguments"><see cref="Arguments"/> for the <paramref name="command"/>.</param>
        public ClientRequest(string command, Arguments arguments)
        {
            m_command = command.ToUpper();
            m_arguments = arguments;
            m_attachments = new List<object>();
        }

        /// <summary>
        /// Creates a new <see cref="ClientRequest"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ClientRequest(SerializationInfo info, StreamingContext context)
        {
            // Deserialize client request fields
            m_command = info.GetOrDefault("command", "");
            m_arguments = info.GetOrDefault("arguments", new Arguments(""));
            m_attachments = new List<object>();

            int attachmentCount = info.GetOrDefault("attachmentCount", 0);

            for (int i = 0; i < attachmentCount; i++)
            {
                m_attachments.Add(info.GetOrDefault("attachment" + i, null as object));
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the command text for the <see cref="ClientRequest"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is either a null or empty string.</exception>
        public string Command
        {
            get
            {
                return m_command;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_command = value.ToUpper();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Arguments"/> for the <see cref="Command"/>.
        /// </summary>
        public Arguments Arguments
        {
            get
            {
                return m_arguments;
            }
            set
            {
                m_arguments = value;
            }
        }

        /// <summary>
        /// Gets a list of attachments for the <see cref="ClientRequest"/>.
        /// </summary>
        public List<object> Attachments
        {
            get
            {
                return m_attachments;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns the <see cref="String"/> that represents the <see cref="ClientRequest"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the <see cref="ClientRequest"/>.</returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", m_command, m_arguments).Trim();
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize client request fields
            info.AddValue("command", m_command);
            info.AddValue("arguments", m_arguments, typeof(Arguments));
            info.AddValue("attachmentCount", m_attachments.Count);

            for (int i = 0; i < m_attachments.Count; i++)
            {
                info.AddValue("attachment" + i, m_attachments[i]);
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Converts <see cref="string"/> to a <see cref="ClientRequest"/>.
        /// </summary>
        /// <param name="text">Text to be converted to a <see cref="ClientRequest"/>.</param>
        /// <returns><see cref="ClientRequest"/> object if parsing is successful; otherwise null.</returns>
        public static ClientRequest Parse(string text)
        {
            // Input text can't be null.
            if ((object)text == null)
                return null;

            // Input text can't be empty.
            text = text.Trim();
            if (text == "")
                return null;

            string[] textSegments = text.Split(' ');
            ClientRequest request = new ClientRequest();
            request.Command = textSegments[0].ToUpper();
            if (textSegments.Length == 1)
                request.Arguments = new Arguments("");
            else
                request.Arguments = new Arguments(text.Remove(0, text.IndexOf(' ') + 1).Trim());

            return request;
        }

        #endregion
    }
}
