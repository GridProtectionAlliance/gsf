//******************************************************************************************************
//  Phasor.cs - Gbtc
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
    /// Creates a new object that represents a Phasor
    /// </summary>
    public class Phasor
    {

        #region [ Members ]
        private int m_ID;
        private int m_DeviceID;
        private string m_Label;
        private string m_Type;
        private string m_Phase;
        private int? m_DestinationPhasorID;
        private int m_SourceIndex;
        private string m_DestinationPhasorLabel;
        private string m_DeviceAcronym;
        private string m_PhasorType;
        private string m_PhaseType;
        private DateTime m_CreatedOn;
        private string m_CreatedBy;
        private DateTime m_UpdatedOn;
        private string m_UpdatedBy;
        
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets and sets the Phasor ID
        /// </summary>
        public int ID
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
        /// <summary>
        /// Gets and sets the Phasor DeviceID
        /// </summary>
        public int DeviceID
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
        /// Gets and sets the Phasor Label
        /// </summary>
        public string Label
        {
            get 
            { 
                return m_Label; 
            }
            set 
            { 
                m_Label = value; 
            }
        }
        /// <summary>
        /// Gets and sets the Phasor Type
        /// </summary>
        public string Type
        {
            get 
            { 
                return m_Type; 
            }
            set 
            { 
                m_Type = value; 
            }
        }
        /// <summary>
        /// Gets and sets the Phase of the current Phasor
        /// </summary>
        public string Phase
        {
            get 
            { 
                return m_Phase; 
            }
            set 
            { 
                m_Phase = value; 
            }
        }
        /// <summary>
        /// Gets and sets Destination Phasor ID for the current Phasor
        /// </summary>
        public int? DestinationPhasorID
        {
            get 
            { 
                return m_DestinationPhasorID; 
            }
            set 
            {
                m_DestinationPhasorID = value; 
            }
        }
        /// <summary>
        /// Gets and sets Source Index for the current Phasor
        /// </summary>
        public int SourceIndex
        {
            get 
            { 
                return m_SourceIndex; 
            }
            set 
            {
                m_SourceIndex = value; 
            }
        }
        /// <summary>
        /// Gets and sets Destination Phasor Label for the current Phasor
        /// </summary>
        public string DestinationPhasorLabel
        {
            get 
            { 
                return m_DestinationPhasorLabel; 
            }
            set 
            { 
                m_DestinationPhasorLabel = value; 
            }
        }
        /// <summary>
        /// Gets and sets the Device Acronym for the current Phasor
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
        /// Gets and sets Phasor Type for the current Phasor
        /// </summary>
        public string PhasorType
        {
            get 
            { 
                return m_PhasorType; 
            }
            set 
            { 
                m_PhasorType = value; 
            }
        }
        /// <summary>
        /// Gets and sets the Date and Time the current Phasor was created on
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
        /// Gets and sets who the current Phasor was created by
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
        /// Gets and sets the Date and Time the current Phasor was updated on
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
        /// Gets and sets who the current Phasor was updated by
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

        #endregion
        
    }
}
