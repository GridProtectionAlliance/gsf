//*******************************************************************************************************
//  IDataPoint.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/24/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using TVA.Parsing;

namespace TVA.Historian
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the quality of time series data.
    /// </summary>
    public enum Quality
    {
        Unknown,
        DeletedFromProcessing,
        CouldNotCalculatePoint,
        DASFrontEndHardwareError,
        SensorReadError,
        OpenTransducerDetection,
        InputCountsOutOfSensorRange,
        UnreasonableHigh,
        UnreasonableLow,
        Old,
        SuspectValueAboveHiHiLimit,
        SuspectValueBelowLoLoLimit,
        SuspectValueAboveHiLimit,
        SuspectValueBelowLoLimit,
        SuspectData,
        DigitalSuspectAlarm,
        InsertedValueAboveHiHiLimit,
        InsertedValueBelowLoLoLimit,
        InsertedValueAboveHiLimit,
        InsertedValueBelowLoLimit,
        InsertedValue,
        DigitalInsertedStatusInAlarm,
        LogicalAlarm,
        ValueAboveHiHiAlarm,
        ValueBelowLoLoAlarm,
        ValueAboveHiAlarm,
        ValueBelowLoAlarm,
        DeletedFromAlarmChecks,
        InhibitedByCutoutPoint,
        Good
    }

    #endregion

    /// <summary>
    /// Defines time series data warehoused by DatAWare.
    /// </summary>
    public interface IDataPoint : ISupportBinaryImage
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the DatAWare identifier of the data point.
        /// </summary>
        int DatAWareId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of the data point.
        /// </summary>
        TimeTag Time { get; set; }

        /// <summary>
        /// Gets or sets the value of the data point.
        /// </summary>
        float Value { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Quality"/> of the data point.
        /// </summary>
        Quality Quality { get; set; }

        #endregion
    }
}
