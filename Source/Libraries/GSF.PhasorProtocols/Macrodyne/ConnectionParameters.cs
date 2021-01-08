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
//  07/11/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace GSF.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// INI file name browser used with Macrodyne <see cref="ConnectionParameters"/>.
    /// </summary>
    public class IniFileNameEditor : FileNameEditor
    {
        /// <summary>
        /// Initializes the open file dialog when it is created.
        /// </summary>
        /// <param name="openFileDialog">The <see cref="OpenFileDialog"/> to use to select a file name.</param>
        protected override void InitializeDialog(OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);

            // We override this function to customize file dialog...
            openFileDialog.Title = "Load Macrodyne Configuration from BPA PDCstream Style INI File";
            openFileDialog.Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*";
        }
    }

    /// <summary>
    /// Represents the extra connection parameters required for a connection to a Macrodyne.
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
        private ProtocolVersion m_protocolVersion;
        private string m_deviceLabel;
        private string m_configurationFileName;
        private bool m_refreshConfigurationFileOnChange;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/>.
        /// </summary>
        public ConnectionParameters()
        {
            m_protocolVersion = ProtocolVersion.M;
            m_configurationFileName = null;
            m_refreshConfigurationFileOnChange = false;
            m_deviceLabel = null;
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConnectionParameters(SerializationInfo info, StreamingContext context)
        {
            // Deserialize connection parameters
            m_protocolVersion = info.GetOrDefault("protocolVersion", ProtocolVersion.M);
            m_configurationFileName = info.GetOrDefault("configurationFileName", (string)null);
            m_refreshConfigurationFileOnChange = info.GetOrDefault("refreshConfigurationFileOnChange", false);
            m_deviceLabel = info.GetOrDefault("deviceLabel", (string)null);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines the Macrodyne protocol version.
        /// </summary>
        [Category("Required Connection Parameters"),
        Description("Set to the proper Macrodyne protocol version."),
        DefaultValue(typeof(ProtocolVersion), "M")]
        public ProtocolVersion ProtocolVersion
        {
            get => m_protocolVersion;
            set => m_protocolVersion = value;
        }

        /// <summary>
        /// Gets or sets the optional Macrodyne configuration source based on a BPA PDCstream style INI file.
        /// </summary>
        [Category("Required Connection Parameters"),
        Description("Defines the Macrodyne configuration source based on a BPA PDCstream style INI file. This is only required when protocol version is 1690G."),
        Editor(typeof(IniFileNameEditor), typeof(UITypeEditor))]
        public string ConfigurationFileName
        {
            get => m_configurationFileName;
            set => m_configurationFileName = value;
        }

        /// <summary>
        /// Gets or sets device section label, as defined in associated INI file.
        /// </summary>
        [Category("Required Connection Parameters"),
        Description("Set to the Macrodyne device ID label as defined in the associated INI file as a [section] entry. This is only required when protocol version is 1690G.")]
        public string DeviceLabel
        {
            get => m_deviceLabel;
            set => m_deviceLabel = value;
        }

        /// <summary>
        /// Gets or sets flag that determines if configuration file is automatically reloaded when it has changed on disk.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Set to True to automatically reload configuration file when it has changed on disk."),
        DefaultValue(true)]
        public bool RefreshConfigurationFileOnChange
        {
            get => m_refreshConfigurationFileOnChange;
            set => m_refreshConfigurationFileOnChange = value;
        }

        /// <summary>
        /// Determines if selected Macrodyne configuration file exists when needed.
        /// </summary>
        [Browsable(false)]
        public override bool ValuesAreValid
        {
            get
            {
                // Configuration file and device label are required for 1690G devices
                if (m_protocolVersion == ProtocolVersion.G && (string.IsNullOrEmpty(m_configurationFileName) || string.IsNullOrEmpty(m_deviceLabel)))
                    return false;

                if (!string.IsNullOrEmpty(m_configurationFileName) && !File.Exists(m_configurationFileName))
                    return false;

                return true;
            }
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
            info.AddValue("protocolVersion", m_protocolVersion, typeof(ProtocolVersion));
            info.AddValue("configurationFileName", m_configurationFileName);
            info.AddValue("refreshConfigurationFileOnChange", m_refreshConfigurationFileOnChange);
            info.AddValue("deviceLabel", m_deviceLabel);
        }

        #endregion
    }
}