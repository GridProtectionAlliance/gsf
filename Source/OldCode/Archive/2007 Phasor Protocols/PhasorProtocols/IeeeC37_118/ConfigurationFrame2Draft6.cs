//*******************************************************************************************************
//  ConfigurationFrame2Draft6.cs
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
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using TVA;
using TVA.Parsing;

namespace PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 draft 6 implementation of a <see cref="IConfigurationFrame"/>, type 2, that can be sent or received.
    /// </summary>
    [Serializable(), SuppressMessage("Microsoft.Maintainability", "CA1501")]
    public class ConfigurationFrame2Draft6 : ConfigurationFrame2
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame2Draft6"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an IEEE C37.118 draft 6 configuration frame, type 2.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigurationFrame2Draft6()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame2Draft6"/> from specified parameters.
        /// </summary>
        /// <param name="timebase">Timebase to use for fraction second resolution.</param>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame2Draft6"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame2Draft6"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame2Draft6"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE C37.118 draft 6 configuration frame, type 2.
        /// </remarks>
        public ConfigurationFrame2Draft6(uint timebase, ushort idCode, Ticks timestamp, ushort frameRate)
            : base(timebase, idCode, timestamp, frameRate)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame2Draft6"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame2Draft6(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="IeeeC37_118.DraftRevision"/> of this <see cref="ConfigurationFrame2Draft6"/>.
        /// </summary>
        public override DraftRevision DraftRevision
        {
            get
            {
                return IeeeC37_118.DraftRevision.Draft6;
            }
        }

        #endregion
    }
}