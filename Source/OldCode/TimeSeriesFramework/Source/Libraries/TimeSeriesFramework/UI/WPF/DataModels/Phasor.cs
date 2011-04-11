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

        // Fields
        private int m_ID;
        private int m_deviceID;
        private string m_label;
        private string m_type;
        private string m_phase;
        private int? m_destinationPhasorID;
        private int m_sourceIndex;
        private string m_destinationPhasorLabel;
        private string m_deviceAcronym;
        private string m_phasorType;
        private string m_phaseType;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the<see cref="Phasor"/>ID.
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
        /// Gets or sets the<see cref="Phasor"/>DeviceID.
        /// </summary>
        public int DeviceID
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
        /// Gets or sets the<see cref="Phasor"/>Label.
        /// </summary>
        public string Label
        {
            get 
            { 
                return m_label; 
            }
            set 
            { 
                m_label = value; 
            }
        }

        /// <summary>
        /// Gets or sets the<see cref="Phasor"/>Type.
        /// </summary>
        public string Type
        {
            get 
            { 
                return m_type; 
            }
            set 
            { 
                m_type = value; 
            }
        }

        /// <summary>
        /// Gets or sets the Phase of the current Phasor.
        /// </summary>
        public string Phase
        {
            get 
            { 
                return m_phase; 
            }
            set 
            { 
                m_phase = value; 
            }
        }

        /// <summary>
        /// Gets or sets Destination<see cref="Phasor"/>ID for the current Phasor.
        /// </summary>
        public int? DestinationPhasorID
        {
            get 
            { 
                return m_destinationPhasorID; 
            }
            set 
            {
                m_destinationPhasorID = value; 
            }
        }

        /// <summary>
        /// Gets or sets Source Index for the current Phasor.
        /// </summary>
        public int SourceIndex
        {
            get 
            { 
                return m_sourceIndex; 
            }
            set 
            {
                m_sourceIndex = value; 
            }
        }

        /// <summary>
        /// Gets or sets Destination<see cref="Phasor"/>Label for the current Phasor.
        /// </summary>
        public string DestinationPhasorLabel
        {
            get 
            { 
                return m_destinationPhasorLabel; 
            }
            set 
            { 
                m_destinationPhasorLabel = value; 
            }
        }

        /// <summary>
        /// Gets or sets the Device Acronym for the current Phasor.
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
        /// Gets or sets<see cref="Phasor"/>Type for the current Phasor.
        /// </summary>
        public string PhasorType
        {
            get 
            { 
                return m_phasorType; 
            }
            set 
            { 
                m_phasorType = value; 
            }
        }

        /// <summary>
        /// Gets or sets Phase Type for the current Phasor.
        /// </summary>
        public string PhaseType
        {
            get
            {
                return m_phaseType;
            }
            set
            {
                m_phaseType = value;
            }
        }

        /// <summary>
        /// Gets or sets the Date or Time the current<see cref="Phasor"/>was created on.
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
        /// Gets or sets who the current<see cref="Phasor"/>was created by.
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
        /// Gets or sets the Date or Time the current<see cref="Phasor"/>was updated on.
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
        /// Gets or sets who the current<see cref="Phasor"/>was updated by.
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

        #endregion
        
    }
}
