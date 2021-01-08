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
//  05/02/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.IEC61850_90_5
{
    /// <summary>
    /// Represents the extra connection parameters available for a connection to an IEC 61850-90-5 stream.
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

        // Constants

        /// <summary>
        /// Default value for <see cref="UseETRConfiguration"/> property.
        /// </summary>
        public const bool DefaultUseETRConfiguration = false;

        /// <summary>
        /// Default value for <see cref="GuessConfiguration"/> property.
        /// </summary>
        public const bool DefaultGuessConfiguration = false;

        /// <summary>
        /// Default value for <see cref="ParseRedundantASDUs"/> property.
        /// </summary>
        public const bool DefaultParseRedundantASDUs = false;

        /// <summary>
        /// Default value for <see cref="IgnoreSignatureValidationFailures"/> property.
        /// </summary>
        public const bool DefaultIgnoreSignatureValidationFailures = true;

        /// <summary>
        /// Default value for <see cref="IgnoreSampleSizeValidationFailures"/> property.
        /// </summary>
        public const bool DefaultIgnoreSampleSizeValidationFailures = false;

        /// <summary>
        /// Default value for <see cref="PhasorAngleFormat"/> property.
        /// </summary>
        public const string DefaultPhasorAngleFormat = "Degrees";

        // Fields
        private bool m_useETRConfiguration;
        private bool m_guessConfiguration;
        private bool m_parseRedundantASDUs;
        private bool m_ignoreSignatureValidationFailures;
        private bool m_ignoreSampleSizeValidationFailures;
        private AngleFormat m_phasorAngleFormat;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/>.
        /// </summary>
        public ConnectionParameters()
        {
            m_useETRConfiguration = DefaultUseETRConfiguration;
            m_guessConfiguration = DefaultGuessConfiguration;
            m_parseRedundantASDUs = DefaultParseRedundantASDUs;
            m_ignoreSignatureValidationFailures = DefaultIgnoreSignatureValidationFailures;
            m_ignoreSampleSizeValidationFailures = DefaultIgnoreSampleSizeValidationFailures;
            m_phasorAngleFormat = (AngleFormat)Enum.Parse(typeof(AngleFormat), DefaultPhasorAngleFormat, true);
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConnectionParameters(SerializationInfo info, StreamingContext context)
        {
            // Deserialize connection parameters
            m_useETRConfiguration = info.GetOrDefault("useETRConfiguration", DefaultUseETRConfiguration);
            m_guessConfiguration = info.GetOrDefault("guessConfiguration", DefaultGuessConfiguration);
            m_parseRedundantASDUs = info.GetOrDefault("parseRedundantASDUs", DefaultParseRedundantASDUs);
            m_ignoreSignatureValidationFailures = info.GetOrDefault("ignoreSignatureValidationFailures", DefaultIgnoreSignatureValidationFailures);
            m_ignoreSampleSizeValidationFailures = info.GetOrDefault("ignoreSampleSizeValidationFailures", DefaultIgnoreSampleSizeValidationFailures);
            m_phasorAngleFormat = info.GetOrDefault("phasorAngleFormat", (AngleFormat)Enum.Parse(typeof(AngleFormat), DefaultPhasorAngleFormat, true));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if system should find associated ETR file using MSVID with same name for configuration.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Determines if system should find associated ETR file using MSVID with same name for configuration."),
        DefaultValue(DefaultUseETRConfiguration)]
        public bool UseETRConfiguration
        {
            get => m_useETRConfiguration;
            set => m_useETRConfiguration = value;
        }

        /// <summary>
        /// Gets or sets flag that determines if system should try to guess at a possible configuration given payload size.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Determines if system should try to guess at a possible configuration given payload size."),
        DefaultValue(DefaultGuessConfiguration)]
        public bool GuessConfiguration
        {
            get => m_guessConfiguration;
            set => m_guessConfiguration = value;
        }

        /// <summary>
        /// Gets or sets flag that determines if system should expose redundantly parsed ASDUs.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Determines if system should expose redundantly parsed ASDUs."),
        DefaultValue(DefaultParseRedundantASDUs)]
        public bool ParseRedundantASDUs
        {
            get => m_parseRedundantASDUs;
            set => m_parseRedundantASDUs = value;
        }

        /// <summary>
        /// Gets or sets flag that determines if system should ignore checksum signature validation errors.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Determines if system should ignore checksum signature validation errors."),
        DefaultValue(DefaultIgnoreSignatureValidationFailures)]
        public bool IgnoreSignatureValidationFailures
        {
            get => m_ignoreSignatureValidationFailures;
            set => m_ignoreSignatureValidationFailures = value;
        }

        /// <summary>
        /// Gets or sets flag that determines if system should ignore sample size validation errors.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Determines if system should ignore sample size validation errors."),
        DefaultValue(DefaultIgnoreSampleSizeValidationFailures)]
        public bool IgnoreSampleSizeValidationFailures
        {
            get => m_ignoreSampleSizeValidationFailures;
            set => m_ignoreSampleSizeValidationFailures = value;
        }

        /// <summary>
        /// Gets or sets flag that determines the phasor angle parsing format.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Determines the phasor angle parsing format."),
        DefaultValue(typeof(AngleFormat), DefaultPhasorAngleFormat)]
        public AngleFormat PhasorAngleFormat
        {
            get => m_phasorAngleFormat;
            set => m_phasorAngleFormat = value;
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
            info.AddValue("useETRConfiguration", m_useETRConfiguration);
            info.AddValue("guessConfiguration", m_guessConfiguration);
            info.AddValue("parseRedundantASDUs", m_parseRedundantASDUs);
            info.AddValue("ignoreSignatureValidationFailures", m_ignoreSignatureValidationFailures);
            info.AddValue("phasorAngleFormat", m_phasorAngleFormat, typeof(AngleFormat));
        }

        #endregion
    }
}