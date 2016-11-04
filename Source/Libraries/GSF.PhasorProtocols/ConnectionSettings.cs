//******************************************************************************************************
//  ConnectionSettings.cs - Gbtc
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
//  06/06/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.Communication;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents information defined in a PMU Connection Tester connection file.
    /// </summary>
    [Serializable]
    public class ConnectionSettings : ISerializable
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Defines <see cref="PhasorProtocol"/> from PmuConnection file.
        /// </summary>
        public PhasorProtocol PhasorProtocol;

        /// <summary>
        /// Defines <see cref="TransportProtocol"/> from PmuConnection file.
        /// </summary>
        public TransportProtocol TransportProtocol;

        /// <summary>
        /// Defines connection string from PmuConnection file.
        /// </summary>
        public string ConnectionString;

        /// <summary>
        /// Defines ID of the source from PmuConnection file.
        /// </summary>
        public int PmuID;

        /// <summary>
        /// Defines frame rate from PmuConnection file.
        /// </summary>
        public int FrameRate;

        /// <summary>
        /// Defines boolean flag if data needs to be repeated again and again.
        /// </summary>
        public bool AutoRepeatPlayback;

        /// <summary>
        /// Defines byte encoding format from PmuConnection file.
        /// </summary>
        public int ByteEncodingDisplayFormat;

        /// <summary>
        /// Defines additional connection information such as alternate command channel from PmuConnection file.
        /// </summary>
        public IConnectionParameters ConnectionParameters;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConnectionSettings"/> instance.
        /// </summary>
        public ConnectionSettings()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionSettings"/> instance from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConnectionSettings(SerializationInfo info, StreamingContext context)
        {
            // Deserialize connection settings values
            if (!Enum.TryParse(info.GetOrDefault("PhasorProtocol", "IEEEC37_118V1"), true, out PhasorProtocol))
                PhasorProtocol = PhasorProtocol.IEEEC37_118V1;

            if (!Enum.TryParse(info.GetOrDefault("TransportProtocol", "Tcp"), true, out TransportProtocol))
                TransportProtocol = TransportProtocol.Tcp;

            ConnectionString = info.GetOrDefault("ConnectionString", (string)null);

            if (!int.TryParse(info.GetOrDefault("PmuID", "1"), out PmuID))
                PmuID = 1;

            if (!int.TryParse(info.GetOrDefault("FrameRate", "30"), out FrameRate))
                FrameRate = 30;

            if (!(info.GetOrDefault("AutoRepeatPlayback", "false")).ParseBoolean())
                AutoRepeatPlayback = false;

            if (!int.TryParse(info.GetOrDefault("ByteEncodingDisplayFormat", "0"), out ByteEncodingDisplayFormat))
                ByteEncodingDisplayFormat = 0;

            ConnectionParameters = info.GetOrDefault("ConnectionParameters", (IConnectionParameters)null);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize connection settings values
            info.AddValue("PhasorProtocol", PhasorProtocol, typeof(PhasorProtocol));
            info.AddValue("TransportProtocol", TransportProtocol, typeof(TransportProtocol));
            info.AddValue("ConnectionString", ConnectionString);
            info.AddValue("PmuID", PmuID);
            info.AddValue("FrameRate", FrameRate);
            info.AddValue("AutoRepeatPlayback", AutoRepeatPlayback);
            info.AddValue("ByteEncodingDisplayFormat", ByteEncodingDisplayFormat);
            info.AddValue("ConnectionParameters", ConnectionParameters, typeof(IConnectionParameters));
        }

        #endregion
    }
}
