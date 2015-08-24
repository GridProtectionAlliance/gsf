//*******************************************************************************************************
//  ServiceResponse.cs
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
//       Generated original version of source code.
//  09/30/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace TVA.Services
{
    /// <summary>Service Response to Clients.</summary>
	[Serializable()]
    public class ServiceResponse
	{
        #region [ Members ]

        // Fields
        private string m_type;
        private string m_message;
        private List<object> m_attachments;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a default instance of service response.
        /// </summary>
        public ServiceResponse()
            : this("UNDETERMINED")
        {
        }

        /// <summary>
        /// Initializes a instance of service response with the specified type.
        /// </summary>
        /// <param name="type">The type of service response.</param>
        public ServiceResponse(string type)
            : this(type, "")
        {
        }

        /// <summary>
        /// Initializes a instance of service response with the specified type and message.
        /// </summary>
        /// <param name="type">The type of service response.</param>
        /// <param name="message">The message of the service response.</param>
        public ServiceResponse(string type, string message)
        {
            m_type = type.ToUpper();
            m_message = message;
            m_attachments = new List<object>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the type of response being sent to the client.
        /// </summary>
        /// <value></value>
        /// <returns>The type of response being sent to the client.</returns>
        public string Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value.ToUpper();
            }
        }

        /// <summary>
        /// Gets or sets the message being sent to the client.
        /// </summary>
        /// <value></value>
        /// <returns>The message being sent to the client.</returns>
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
        /// Gets a list of attachments being sent to the client.
        /// </summary>
        /// <value></value>
        /// <returns>A list of attachments being sent to the client.</returns>
        public List<object> Attachments
        {
            get
            {
                return m_attachments;
            }
        }

        #endregion
	}
}
