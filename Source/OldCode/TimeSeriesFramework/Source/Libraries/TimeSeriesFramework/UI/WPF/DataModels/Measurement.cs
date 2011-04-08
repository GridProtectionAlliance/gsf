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

        // Nested Types

        // Constants

        // Delegates

        // Events

        // Fields
        private string m_SignalID;
        private int? m_HistorianID;
        private int m_PointID;
        private int? m_DeviceID;
        private string m_PointTag;
        private string m_AlternateTag;
        private int m_SignalTypeID;
        private int? m_PhasorSourceIndex;
        private string m_SignalReference;
        private double m_Adder;
        private double m_Multiplier;
        private string m_Description;
        private bool m_Enabled;
        private string m_HistorianAcronym;
        private string m_DeviceAcronym;
        private int? m_FramesPerSecond;
        private string m_SignalName;
        private string m_SignalAcronym;
        private string m_SignalSuffix;
        private string m_PhasorLabel;
        private DateTime m_CreatedOn;
        private string m_CreatedBy;
        private DateTime m_UpdatedOn;
        private string m_UpdatedBy;
        private string m_ID;        
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets and sets the current Measurement's Signal ID
        /// </summary>
        public string SignalID
        {
            get 
            { 
                return m_SignalID; 
            }
            set 
            { 
                m_SignalID = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Historian ID
        /// </summary>
        public int? HistorianID
        {
            get 
            { 
                return m_HistorianID; 
            }
            set 
            { 
                m_HistorianID = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Point ID
        /// </summary>
        public int PointID
        {
            get 
            { 
                return m_PointID; 
            }
            set 
            { 
                m_PointID = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Device ID
        /// </summary>
        public int? DeviceID
        {
            get 
            { 
                return m_DeviceID; 
            }
            set 
            { 
                m_DeviceID = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Point Tag
        /// </summary>
        public string PointTag
        {
            get 
            { 
                return m_PointTag; 
            }
            set 
            { 
                m_PointTag = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Alternate Tag
        /// </summary>
        public string AlternateTag
        {
            get 
            { 
                return m_AlternateTag; 
            }
            set 
            { 
                m_AlternateTag = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Signal Type ID
        /// </summary>
        public int SignalTypeID
        {
            get 
            { 
                return m_SignalTypeID; 
            }
            set 
            { 
                m_SignalTypeID = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Phasor Source Index
        /// </summary>
        public int? PhasorSourceIndex
        {
            get 
            { 
                return m_PhasorSourceIndex; 
            }
            set 
            { 
                m_PhasorSourceIndex = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Signal Reference
        /// </summary>
        public string SignalReference
        {
            get 
            { 
                return m_SignalReference; 
            }
            set 
            { 
                m_SignalReference = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Adder
        /// </summary>
        public double Adder
        {
            get 
            { 
                return m_Adder; 
            }
            set 
            { 
                m_Adder = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Multiplier
        /// </summary>
        public double Multiplier
        {
            get 
            { 
                return m_Multiplier; 
            }
            set 
            { 
                m_Multiplier = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Description
        /// </summary>
        public string Description
        {
            get 
            { 
                return m_Description; 
            }
            set 
            { 
                m_Description = value; 
            }
        }
        /// <summary>
        /// Gets and sets whether the current Measurement is enabled
        /// </summary>
        public bool Enabled
        {
            get 
            { 
                return m_Enabled; 
            }
            set 
            { 
                m_Enabled = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Historian Acronym
        /// </summary>
        public string HistorianAcronym
        {
            get 
            { 
                return m_HistorianAcronym; 
            }
            set 
            { 
                m_HistorianAcronym = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Device Acronym
        /// </summary>
        public string DeviceAcronym
        {
            get 
            { 
                return m_DeviceAcronym; 
            }
            set 
            { 
                m_DeviceAcronym = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Frames Per Second
        /// </summary>
        public int? FramesPerSecond
        {
            get 
            { 
                return m_FramesPerSecond; 
            }
            set 
            { 
                m_FramesPerSecond = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Signal Name
        /// </summary>
        public string SignalName
        {
            get 
            { 
                return m_SignalName; 
            }
            set 
            { 
                m_SignalName = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Signal Acronym
        /// </summary>
        public string SignalAcronym
        {
            get 
            { 
                return m_SignalAcronym; 
            }
            set 
            { 
                m_SignalAcronym = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Signal Suffix
        /// </summary>
        public string SignalSuffix
        {
            get 
            { 
                return m_SignalSuffix; 
            }
            set 
            { 
                m_SignalSuffix = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's Phasor Label
        /// </summary>
        public string PhasorLabel
        {
            get 
            { 
                return m_PhasorLabel; 
            }
            set 
            { 
                m_PhasorLabel = value; 
            }
        }
        /// <summary>
        /// Gets and sets when the current Measurement was Created
        /// </summary>
        public DateTime CreatedOn
        {
            get 
            { 
                return m_CreatedOn; 
            }
            set 
            { 
                m_CreatedOn = value; 
            }
        }
        /// <summary>
        /// Gets and sets who the current Measurement was created by
        /// </summary>
        public string CreatedBy
        {
            get 
            { 
                return m_CreatedBy; 
            }
            set 
            { 
                m_CreatedBy = value; 
            }
        }
        /// <summary>
        /// Gets and sets when the current Measurement updated
        /// </summary>
        public DateTime UpdatedOn
        {
            get 
            { 
                return m_UpdatedOn; 
            }
            set 
            { 
                m_UpdatedOn = value; 
            }
        }
        /// <summary>
        /// Gets and sets who the current Measurement was updated by
        /// </summary>
        public string UpdatedBy
        {
            get 
            { 
                return m_UpdatedBy; 
            }
            set 
            { 
                m_UpdatedBy = value; 
            }
        }
        /// <summary>
        /// Gets and sets the current Measurement's ID
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
