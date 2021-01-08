//******************************************************************************************************
//  ConnectionParameters.cs - Gbtc
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
//  04/27/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the extra connection parameters required for a connection to a SEL device.
    /// </summary>
    /// <remarks>
    /// This class is designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.
    /// As a result the <see cref="CategoryAttribute"/> and <see cref="DescriptionAttribute"/> elements should be defined for
    /// each of the exposed properties.
    /// </remarks>
    [Serializable]
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
            m_messagePeriod = info.GetOrDefault("messagePeriod", MessagePeriod.DefaultRate);
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
            get => m_messagePeriod;
            set => m_messagePeriod = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize connection parameters
            info.AddValue("messagePeriod", m_messagePeriod, typeof(MessagePeriod));
        }

        #endregion
    }
}