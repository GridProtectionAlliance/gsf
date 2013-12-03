//******************************************************************************************************
//  Message.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  10/06/2010 - Pinal C. Patel
//       Generated original version of source code.
//  03/11/2011 - Pinal C. Patel
//       Marked the class with Serializable attribute and changed properties to field for serialization.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.ServiceBus
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates how a <see cref="Message"/> is distributed by the <see cref="ServiceBusService"/>.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// <see cref="Message"/> is distributed to all of its registered consumers.
        /// </summary>
        Topic,
        /// <summary>
        /// <see cref="Message"/> is distributed to the first of all its registered consumers.
        /// </summary>
        Queue
    }

    #endregion

    /// <summary>
    /// Represents a message that can be used to exchange information between processes using <see cref="ServiceBusService"/>.
    /// </summary>
    [Serializable]
    public class Message
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when this <see cref="Message"/> was created.
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// Gets or sets the <see cref="MessageType">Type</see> of this <see cref="Message"/>.
        /// </summary>
        public MessageType Type;

        /// <summary>
        /// Gets or sets the identifier of this <see cref="Message"/>.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets the format of the <see cref="Content"/> in this <see cref="Message"/>.
        /// </summary>
        public string Format;

        /// <summary>
        /// Gets or sets the actual payload of this <see cref="Message"/>.
        /// </summary>
        public byte[] Content;

        #endregion
    }
}
