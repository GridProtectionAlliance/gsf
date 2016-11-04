//******************************************************************************************************
//  DigitalChannel.cs - Gbtc
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
//  05/19/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Collections;

namespace GSF.COMTRADE
{
    /// <summary>
    /// Represents a digital channel definition of the <see cref="Schema"/>.
    /// </summary>
    public class DigitalChannel
    {
        #region [ Members ]

        // Fields
        private int m_index;
        private string m_stationName;
        private string m_channelName;
        private string m_phaseID;
        private string m_circuitComponent;
        private bool m_normalState;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DigitalChannel"/>.
        /// </summary>
        public DigitalChannel()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DigitalChannel"/> from an existing line image.
        /// </summary>
        /// <param name="lineImage">Line image to parse.</param>
        public DigitalChannel(string lineImage)
        {
            // Dn,ch_id,ph,ccbm,y
            string[] parts = lineImage.Split(',');


            if(parts.Length == 5)
            {
                Index = int.Parse(parts[0].Trim());
                Name = parts[1];
                PhaseID = parts[2];
                CircuitComponent = parts[3];
                NormalState = parts[4].Trim().ParseBoolean();
            }
            else if(parts.Length == 3)
            {
                Index = int.Parse(parts[0].Trim());
                Name = parts[1];
                NormalState = parts[2].Trim().ParseBoolean();
            }
            else
                throw new InvalidOperationException(string.Format("Unexpected number of line image elements for digital channel definition: {0} - expected 5\r\nImage = {1}", parts.Length, lineImage));

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets index of this <see cref="DigitalChannel"/>.
        /// </summary>
        public int Index
        {
            get
            {
                return m_index;
            }
            set
            {
                m_index = value;
            }
        }

        /// <summary>
        /// Gets or sets name of this <see cref="DigitalChannel"/> formatted as station_name:channel_name.
        /// </summary>
        /// <exception cref="FormatException">Name must be formatted as station_name:channel_name.</exception>
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_stationName))
                    return m_channelName;

                return string.Format("{0}:{1}", m_stationName, m_channelName);
            }
            set
            {
                string[] parts = value.Split(':');

                if (parts.Length == 2)
                {
                    m_stationName = parts[0].Trim();
                    m_channelName = parts[1].Trim();
                }
                else
                {
                    m_stationName = "";
                    m_channelName = parts[0].Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets station name component of this <see cref="DigitalChannel"/>.
        /// </summary>
        public string StationName
        {
            get
            {
                return m_stationName;
            }
            set
            {
                m_stationName = value.Replace(":", "_").Trim();
            }
        }

        /// <summary>
        /// Gets or sets channel name component of this <see cref="DigitalChannel"/>.
        /// </summary>
        public string ChannelName
        {
            get
            {
                return m_channelName;
            }
            set
            {
                m_channelName = value.Replace(":", "_").Trim();
            }
        }

        /// <summary>
        /// Gets or sets the 2-character phase identifier for this <see cref="DigitalChannel"/>.
        /// </summary>
        public string PhaseID
        {
            get
            {
                return m_phaseID;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    m_phaseID = value.Trim();
                else
                    m_phaseID = "";

                // Even though the COMTRADE standard specifically says this is 2 characters in length, the
                // sample data in the schema for phasor data has phase IDs with lengths of 3 (e.g., T10)
                if (m_phaseID.Length > 3)
                    m_phaseID = m_phaseID.Substring(0, 3);
            }
        }

        /// <summary>
        /// Gets or sets circuit component of this <see cref="DigitalChannel"/>.
        /// </summary>
        public string CircuitComponent
        {
            get
            {
                return m_circuitComponent;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    m_circuitComponent = value.Trim();
                else
                    m_circuitComponent = "";

                if (m_circuitComponent.Length > 64)
                    m_circuitComponent = m_circuitComponent.Substring(0, 64);
            }
        }

        /// <summary>
        /// Gets or sets normal state of this <see cref="DigitalChannel"/>.
        /// </summary>
        public bool NormalState
        {
            get
            {
                return m_normalState;
            }
            set
            {
                m_normalState = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Converts <see cref="DigitalChannel"/> to its string format.
        /// </summary>
        public override string ToString()
        {
            string[] values = new string[5];

            // Dn,ch_id,ph,ccbm,y
            values[0] = Index.ToString();
            values[1] = Name;
            values[2] = PhaseID;
            values[3] = CircuitComponent;
            values[4] = NormalState ? "1" : "0";

            return values.ToDelimitedString(',');
        }

        #endregion
    }
}