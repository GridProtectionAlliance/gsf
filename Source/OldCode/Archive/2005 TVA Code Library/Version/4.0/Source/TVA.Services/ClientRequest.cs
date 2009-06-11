//*******************************************************************************************************
//  ClientRequest.cs
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
//  08/29/2006 - Pinal C. Patel
//       Original version of source code generated
//  04/27/2007 - Pinal C. Patel
//       Added Attachments property for clients to send serializable objects as part of the request
//  09/30/2008 - James R Carroll
//       Converted to C#.
//  03/09/2009 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA.Console;

namespace TVA.Services
{
    /// <summary>
    /// Represents a request sent by <see cref="ClientHelper"/> to <see cref="ServiceHelper"/>.
    /// </summary>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ServiceHelper"/>
	[Serializable()]
    public class ClientRequest
	{
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents information about a <see cref="ClientRequest"/> sent by <see cref="ClientHelper"/>.
        /// </summary>
        /// <seealso cref="ClientInfo"/>
        /// <seealso cref="ClientRequest"/>
        public class Info
        {
            #region [ Constructors ]

            /// <summary>
            /// Initializes a new instance of the <see cref="Info"/> class.
            /// </summary>
            /// <param name="sender"><see cref="ClientInfo"/> object of the <paramref name="request"/> sender.</param>
            /// <param name="request"><see cref="ClientRequest"/> object sent by the <paramref name="sender"/>.</param>
            public Info(ClientInfo sender, ClientRequest request)
            {
                Request = request;
                Sender = sender;
                ReceivedAt = DateTime.Now;
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets or sets the <see cref="ClientInfo"/> object of the <see cref="Request"/> sender.
            /// </summary>
            public ClientInfo Sender { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="ClientRequest"/> object sent by the <see cref="Sender"/>.
            /// </summary>
            public ClientRequest Request { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="DateTime"/> when the <see cref="Request"/> was received from the <see cref="Sender"/>.
            /// </summary>
            public DateTime ReceivedAt { get; set; }

            #endregion
        }

        // Fields
        private string m_command;
        private Arguments m_arguments;
        private List<object> m_attachments;

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

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the command text for the <see cref="ClientRequest"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is either a null or empty string.</exception>
        public string Command
        {
            get
            {
                return m_command;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

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

        #region [ Static ]

        /// <summary>
        /// Converts <see cref="string"/> to a <see cref="ClientRequest"/>.
        /// </summary>
        /// <param name="text">Text to be converted to a <see cref="ClientRequest"/>.</param>
        /// <returns><see cref="ClientRequest"/> object if parsing is successful; otherwise null.</returns>
        public static ClientRequest Parse(string text)
        {
            ClientRequest request = null;

            if (!string.IsNullOrEmpty(text))
            {
                string[] textSegments = text.Split(' ');

                if (textSegments.Length > 0)
                {
                    request = new ClientRequest();
                    request.Command = textSegments[0].ToUpper();

                    if (textSegments.Length == 1)
                    {
                        request.Arguments = new Arguments("");
                    }
                    else
                    {
                        request.Arguments = new Arguments(text.Remove(0, text.IndexOf(' ')));
                    }
                }
            }

            return request;
        }

        #endregion
	}
}
 