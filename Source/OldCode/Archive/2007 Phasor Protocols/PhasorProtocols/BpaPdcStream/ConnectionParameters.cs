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
//  02/26/2007 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace TVA.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// INI file name browser used with BPA PDCstream <see cref="ConnectionParameters"/>.
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
            openFileDialog.Title = "Load BPA PDCstream INI Configuration File";
            openFileDialog.Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*";
        }
    }

    /// <summary>
    /// Represents the extra connection parameters required for a connection to a BPA PDCstream.
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
        private string m_configurationFileName;
        private bool m_refreshConfigurationFileOnChange;
        private bool m_parseWordCountFromByte;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/>.
        /// </summary>
        public ConnectionParameters()
        {
            m_refreshConfigurationFileOnChange = true;
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConnectionParameters(SerializationInfo info, StreamingContext context)
        {
            // Deserialize connection parameters
            m_configurationFileName = info.GetString("configurationFileName");
            m_refreshConfigurationFileOnChange = info.GetBoolean("refreshConfigurationFileOnChange");
            m_parseWordCountFromByte = info.GetBoolean("parseWordCountFromByte");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets required external BPA PDCstream INI based configuration file.
        /// </summary>
        [Category("Required Connection Parameters"), 
        Description("Defines required external BPA PDCstream INI based configuration file."), 
        Editor(typeof(IniFileNameEditor), typeof(UITypeEditor))]
        public string ConfigurationFileName
        {
            get
            {
                return m_configurationFileName;
            }
            set
            {
                m_configurationFileName = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that interprets word count in packet header from a byte instead of a word.
        /// </summary>
        [Category("Required Connection Parameters"), 
        Description("Set to True to interpret word count in packet header from a byte instead of a word - if the sync byte (0xAA) is at position one, then the word count would be interpreted from byte four.  Some older BPA PDC stream implementations have a 0x01 in byte three where there should be a 0x00 and this throws off the frame length, setting this property to True will correctly interpret the word count."), 
        DefaultValue(false)]
        public bool ParseWordCountFromByte
        {
            get
            {
                return m_parseWordCountFromByte;
            }
            set
            {
                m_parseWordCountFromByte = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if configuration file is automatically reloaded when it has changed on disk.
        /// </summary>
        [Category("Optional Connection Parameters"), 
        Description("Set to True to automatically reload configuration file when it has changed on disk."), 
        DefaultValue(true)]
        public bool RefreshConfigurationFileOnChange
        {
            get
            {
                return m_refreshConfigurationFileOnChange;
            }
            set
            {
                m_refreshConfigurationFileOnChange = value;
            }
        }

        /// <summary>
        /// Determines if selected BPA PDCstream configuration file exists.
        /// </summary>
        [Browsable(false)]
        public override bool ValuesAreValid
        {
            get
            {
                return File.Exists(m_configurationFileName);
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
            info.AddValue("configurationFileName", m_configurationFileName);
            info.AddValue("refreshConfigurationFileOnChange", m_refreshConfigurationFileOnChange);
            info.AddValue("parseWordCountFromByte", m_parseWordCountFromByte);
        }

        #endregion
    }
}