//*******************************************************************************************************
//  SignalReferenceMeasurement.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/26/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using TVA.Measurements;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents a basic <see cref="IMeasurement"/> value that is associated with a given <see cref="SignalReference"/>.
    /// </summary>
    public class SignalReferenceMeasurement : Measurement
    {
        #region [ Members ]

        // Fields
        private SignalReference m_signalReference;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="SignalReferenceMeasurement"/> from the specified parameters.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        /// <param name="signalReference">Associated <see cref="SignalReference"/>.</param>
        public SignalReferenceMeasurement(IMeasurement measurement, SignalReference signalReference)
            : base(measurement.ID, measurement.Source, measurement.Value, measurement.Adder, measurement.Multiplier, measurement.Timestamp)
        {
            base.ValueQualityIsGood = measurement.ValueQualityIsGood;
            base.TimestampQualityIsGood = measurement.TimestampQualityIsGood;
            m_signalReference = signalReference;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="SignalReference"/> associated with this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        public SignalReference SignalReference
        {
            get
            {
                return m_signalReference;
            }
        }

        #endregion
    }
}
