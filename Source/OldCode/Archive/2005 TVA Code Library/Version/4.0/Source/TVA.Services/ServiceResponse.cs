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
//  03/09/2009 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace TVA.Services
{
    /// <summary>
    /// Represents a response sent by the <see cref="ServiceHelper"/> to a <see cref="ClientRequest"/> from the <see cref="ClientHelper"/>.
    /// </summary>
    /// <seealso cref="ServiceHelper"/>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ClientRequest"/>
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
                    throw new ArgumentNullException();

                m_type = value.ToUpper();
            }
        }

        /// <summary>
        /// Gets or sets the palin-text message associated with the <see cref="ServiceResponse"/>.
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
