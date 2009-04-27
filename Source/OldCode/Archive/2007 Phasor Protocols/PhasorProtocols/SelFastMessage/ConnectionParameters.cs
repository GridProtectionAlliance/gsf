//*******************************************************************************************************
//  ConnectionParameters.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/27/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PCS.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the extra connection parameters required for a connection to a SEL device.
    /// </summary>
    /// <remarks>
    /// This class is designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.
    /// As a result the <see cref="CategoryAttribute"/> and <see cref="DescriptionAttribute"/> elements should be defined for
    /// each of the exposed properties.
    /// </remarks>
    [Serializable()]
    public class ConnectionParameters : ConnectionParametersBase
    {        
        #region [ Members ]

        // Fields
        private MessagePeriod m_messagePeriod;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/>.
        /// </summary>
        public ConnectionParameters()
        {
            m_messagePeriod = MessagePeriod.DefaultRate;
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConnectionParameters(SerializationInfo info, StreamingContext context)
        {
            // Deserialize connection parameters
            m_messagePeriod = (MessagePeriod)info.GetValue("messagePeriod", typeof(MessagePeriod));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets desired <see cref="SelFastMessage.MessagePeriod"/> for SEL device.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Defines desired message period for SEL device. Default rate is 20 messages per second. Note that slower rates (i.e., 20 messages per minute down to 1 message per minute) are only supported by the SEL 300 series relays."),
        DefaultValue(typeof(MessagePeriod), "DefaultRate")]
        public MessagePeriod MessagePeriod
        {
            get
            {
                return m_messagePeriod;
            }
            set
            {
                m_messagePeriod = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize connection parameters
            info.AddValue("messagePeriod", m_messagePeriod, typeof(MessagePeriod));
        }

        #endregion
    }
}