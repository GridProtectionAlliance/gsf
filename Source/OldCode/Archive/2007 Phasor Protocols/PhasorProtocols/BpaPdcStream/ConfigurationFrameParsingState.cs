//*******************************************************************************************************
//  ConfigurationFrameParsingState.cs
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
//  04/23/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of the parsing state used by a <see cref="ConfigurationFrame"/>.
    /// </summary>
    public class ConfigurationFrameParsingState : PhasorProtocols.ConfigurationFrameParsingState
    {
        #region [ Members ]

        // Fields
        private string m_configurationFileName;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="ConfigurationFrame"/> being parsed.</param>
        /// <param name="configurationFileName">The required external BPA PDCstream INI based configuration file.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="ConfigurationCell"/> instances.</param>
        public ConfigurationFrameParsingState(int parsedBinaryLength, string configurationFileName, CreateNewCellFunction<IConfigurationCell> createNewCellFunction)
            : base(parsedBinaryLength, createNewCellFunction)
        {
            m_configurationFileName = configurationFileName;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets required external BPA PDCstream INI based configuration file.
        /// </summary>
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

        #endregion
    }
}