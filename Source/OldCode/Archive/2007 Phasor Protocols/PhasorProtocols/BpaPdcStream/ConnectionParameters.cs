//*******************************************************************************************************
//  ConnectionParameters.vb - BPA PDCstream specific connection parameters
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/26/2007 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace PCS.PhasorProtocols
{
    namespace BpaPdcStream
    {
        #region " INI File Name Editor for Property Grid "

        public class IniFileNameEditor : FileNameEditor
        {
            protected override void InitializeDialog(OpenFileDialog openFileDialog)
            {
                base.InitializeDialog(openFileDialog);

                // We override this function to customize file dialog...
                openFileDialog.Title = "Load BPA PDCstream INI Configuration File";
                openFileDialog.Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*";
            }
        }

        #endregion

        /// <summary>BPA PDCstream specific connection parameters</summary>
        [Serializable()]
        public class ConnectionParameters : ConnectionParametersBase
        {
            private string m_configurationFileName;
            private bool m_refreshConfigurationFileOnChange;
            private bool m_parseWordCountFromByte;

            protected ConnectionParameters(SerializationInfo info, StreamingContext context)
            {
                // Deserialize connection parameters
                m_configurationFileName = info.GetString("configurationFileName");
                m_refreshConfigurationFileOnChange = info.GetBoolean("refreshConfigurationFileOnChange");
                m_parseWordCountFromByte = info.GetBoolean("parseWordCountFromByte");
            }

            public ConnectionParameters()
            {
                m_refreshConfigurationFileOnChange = true;
            }

            [Category("Required Connection Parameters"), Description("Defines required external BPA PDCstream INI based configuration file."), Editor(typeof(IniFileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

            [Category("Required Connection Parameters"), Description("Set to True to interpret word count in packet header from a byte instead of a word - if the sync byte (0xAA) is at position one, then the word count would be interpreted from byte four.  Some older BPA PDC stream implementations have a 0x01 in byte three where there should be a 0x00 and this throws off the frame length, setting this property to True will correctly interpret the word count."), DefaultValue(false)]
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

            [Category("Optional Connection Parameters"), Description("Set to True to automatically reload configuration file when it has changed on disk."), DefaultValue(true)]
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

            public override bool ValuesAreValid
            {
                get
                {
                    return File.Exists(m_configurationFileName);
                }
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                // Serialize connection parameters
                info.AddValue("configurationFileName", m_configurationFileName);
                info.AddValue("refreshConfigurationFileOnChange", m_refreshConfigurationFileOnChange);
                info.AddValue("parseWordCountFromByte", m_parseWordCountFromByte);
            }
        }
    }
}
