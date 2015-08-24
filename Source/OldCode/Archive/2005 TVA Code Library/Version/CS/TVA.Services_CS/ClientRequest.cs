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
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA.Console;

namespace TVA.Services
{
    /// <summary>Client Request to Service.</summary>
	[Serializable()]
    public class ClientRequest
	{
        #region [ Members ]

        // Fields
        private string m_command;
        private Arguments m_arguments;
        private List<object> m_attachments;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a default instance of client request.
        /// </summary>
        public ClientRequest()
            : this("UNDEFINED")
        {
        }

        /// <summary>
        /// Initializes a instance of client request with the specified command.
        /// </summary>
        /// <param name="command">The command for the client request.</param>
        public ClientRequest(string command)
            : this(command, new Arguments(""))
        {
        }

        /// <summary>
        /// Initializes a instance of client request with the specified command and arguments.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        public ClientRequest(string command, Arguments arguments)
        {
            m_command = command.ToUpper();
            m_arguments = arguments;
            m_attachments = new List<object>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the command for the request being sent to the service.
        /// </summary>
        /// <value>The command for the request being sent to the service.</value>
        public string Command
        {
            get
            {
                return m_command;
            }
            set
            {
                m_command = value.ToUpper();
            }
        }

        /// <summary>
        /// Gets or sets additional parameters being sent to the service.
        /// </summary>
        /// <value></value>
        /// <returns>Additional parameters being sent to the service.</returns>
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
        /// Gets a list of attachments being sent to the service.
        /// </summary>
        /// <value></value>
        /// <returns>A list of attachments being sent to the service.</returns>
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
        /// Parses the specified text into TVA.Services.ClientRequest.
        /// </summary>
        /// <param name="text">The text to be parsed.</param>
        /// <returns>A TVA.Services.ClientRequest instance.</returns>
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
