//******************************************************************************************************
//  Measurement.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSeriesFramework.UI.DataModels
{

    /// <summary>
    /// Creates a new object that represents a Measurement
    /// </summary>
    public class Measurement
    {
        #region [ Members ]

        private string m_signalID;
        private int? m_historianID;
        private int m_pointID;
        private int? m_deviceID;
        private string m_pointTag;
        private string m_alternateTag;
        private int m_signalTypeID;
        private int? m_phasorSourceIndex;
        private string m_signalReference;
        private double m_adder;
        private double m_multiplier;
        private string m_description;
        private bool m_enabled;
        private string m_historianAcronym;
        private string m_deviceAcronym;
        private int? m_framesPerSecond;
        private string m_signalName;
        private string m_signalAcronym;
        private string m_signalSuffix;
        private string m_phasorLabel;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        private string m_ID; 
       
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets and sets the current Measurement's Signal ID.
        /// </summary>
        public string SignalID
        {
            get 
            { 
                return m_signalID; 
            }
            set 
            { 
                m_signalID = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Historian ID.
        /// </summary>
        public int? HistorianID
        {
            get 
            { 
                return m_historianID; 
            }
            set 
            { 
                m_historianID = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Point ID.
        /// </summary>
        public int PointID
        {
            get 
            { 
                return m_pointID; 
            }
            set 
            { 
                m_pointID = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Device ID.
        /// </summary>
        public int? DeviceID
        {
            get 
            { 
                return m_deviceID; 
            }
            set 
            { 
                m_deviceID = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Point Tag.
        /// </summary>
        public string PointTag
        {
            get 
            { 
                return m_pointTag; 
            }
            set 
            { 
                m_pointTag = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Alternate Tag.
        /// </summary>
        public string AlternateTag
        {
            get 
            { 
                return m_alternateTag; 
            }
            set 
            { 
                m_alternateTag = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Signal Type ID.
        /// </summary>
        public int SignalTypeID
        {
            get 
            { 
                return m_signalTypeID; 
            }
            set 
            { 
                m_signalTypeID = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Phasor Source Index.
        /// </summary>
        public int? PhasorSourceIndex
        {
            get 
            { 
                return m_phasorSourceIndex; 
            }
            set 
            { 
                m_phasorSourceIndex = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Signal Reference.
        /// </summary>
        public string SignalReference
        {
            get 
            { 
                return m_signalReference; 
            }
            set 
            { 
                m_signalReference = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Adder.
        /// </summary>
        public double Adder
        {
            get 
            { 
                return m_adder; 
            }
            set 
            { 
                m_adder = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Multiplier.
        /// </summary>
        public double Multiplier
        {
            get 
            { 
                return m_multiplier; 
            }
            set 
            { 
                m_multiplier = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Description.
        /// </summary>
        public string Description
        {
            get 
            { 
                return m_description; 
            }
            set 
            { 
                m_description = value; 
            }
        }

        /// <summary>
        /// Gets and sets whether the current Measurement is enabled.
        /// </summary>
        public bool Enabled
        {
            get 
            { 
                return m_enabled; 
            }
            set 
            { 
                m_enabled = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Historian Acronym.
        /// </summary>
        public string HistorianAcronym
        {
            get 
            { 
                return m_historianAcronym; 
            }
            set 
            { 
                m_historianAcronym = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Device Acronym.
        /// </summary>
        public string DeviceAcronym
        {
            get 
            { 
                return m_deviceAcronym; 
            }
            set 
            { 
                m_deviceAcronym = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Frames Per Second.
        /// </summary>
        public int? FramesPerSecond
        {
            get 
            { 
                return m_framesPerSecond; 
            }
            set 
            { 
                m_framesPerSecond = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Signal Name.
        /// </summary>
        public string SignalName
        {
            get 
            { 
                return m_signalName; 
            }
            set 
            { 
                m_signalName = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Signal Acronym.
        /// </summary>
        public string SignalAcronym
        {
            get 
            { 
                return m_signalAcronym; 
            }
            set 
            { 
                m_signalAcronym = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Signal Suffix.
        /// </summary>
        public string SignalSuffix
        {
            get 
            { 
                return m_signalSuffix; 
            }
            set 
            { 
                m_signalSuffix = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's Phasor Label.
        /// </summary>
        public string PhasorLabel
        {
            get 
            { 
                return m_phasorLabel; 
            }
            set 
            { 
                m_phasorLabel = value; 
            }
        }

        /// <summary>
        /// Gets and sets when the current Measurement was Created.
        /// </summary>
        public DateTime CreatedOn
        {
            get 
            { 
                return m_createdOn; 
            }
            set 
            { 
                m_createdOn = value; 
            }
        }

        /// <summary>
        /// Gets and sets who the current Measurement was created by.
        /// </summary>
        public string CreatedBy
        {
            get 
            { 
                return m_createdBy; 
            }
            set 
            { 
                m_createdBy = value; 
            }
        }

        /// <summary>
        /// Gets and sets when the current Measurement updated.
        /// </summary>
        public DateTime UpdatedOn
        {
            get 
            { 
                return m_updatedOn; 
            }
            set 
            { 
                m_updatedOn = value; 
            }
        }

        /// <summary>
        /// Gets and sets who the current Measurement was updated by.
        /// </summary>
        public string UpdatedBy
        {
            get 
            { 
                return m_updatedBy; 
            }
            set 
            { 
                m_updatedBy = value; 
            }
        }

        /// <summary>
        /// Gets and sets the current Measurement's ID.
        /// </summary>
        public string ID
        {
            get 
            { 
                return m_ID; 
            }
            set 
            {
                m_ID = value; 
            }
        } 

        #endregion
        
    }
}
