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
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// DeletedFromProcessing
        /// </summary>
        DeletedFromProcessing,
        /// <summary>
        /// CouldNotCalculate
        /// </summary>
        CouldNotCalculate,
        /// <summary>
        /// FrontEndHardwareError
        /// </summary>
        FrontEndHardwareError,
        /// <summary>
        /// SensorReadError
        /// </summary>
        SensorReadError,
        /// <summary>
        /// OpenThermocouple
        /// </summary>
        OpenThermocouple,
        /// <summary>
        /// InputCountsOutOfSensorRange
        /// </summary>
        InputCountsOutOfSensorRange,
        /// <summary>
        /// UnreasonableHigh
        /// </summary>
        UnreasonableHigh,
        /// <summary>
        /// UnreasonableLow
        /// </summary>
        UnreasonableLow,
        /// <summary>
        /// Old
        /// </summary>
        Old,
        /// <summary>
        /// SuspectValueAboveHiHiLimit
        /// </summary>
        SuspectValueAboveHiHiLimit,
        /// <summary>
        /// SuspectValueBelowLoLoLimit
        /// </summary>
        SuspectValueBelowLoLoLimit,
        /// <summary>
        /// SuspectValueAboveHiLimit
        /// </summary>
        SuspectValueAboveHiLimit,
        /// <summary>
        /// SuspectValueBelowLoLimit
        /// </summary>
        SuspectValueBelowLoLimit,
        /// <summary>
        /// SuspectData
        /// </summary>
        SuspectData,
        /// <summary>
        /// DigitalSuspectAlarm
        /// </summary>
        DigitalSuspectAlarm,
        /// <summary>
        /// InsertedValueAboveHiHiLimit
        /// </summary>
        InsertedValueAboveHiHiLimit,
        /// <summary>
        /// InsertedValueBelowLoLoLimit
        /// </summary>
        InsertedValueBelowLoLoLimit,
        /// <summary>
        /// InsertedValueAboveHiLimit
        /// </summary>
        InsertedValueAboveHiLimit,
        /// <summary>
        /// InsertedValueBelowLoLimit
        /// </summary>
        InsertedValueBelowLoLimit,
        /// <summary>
        /// InsertedValue
        /// </summary>
        InsertedValue,
        /// <summary>
        /// DigitalInsertedStatusInAlarm
        /// </summary>
        DigitalInsertedStatusInAlarm,
        /// <summary>
        /// LogicalAlarm
        /// </summary>
        LogicalAlarm,
        /// <summary>
        /// ValueAboveHiHiAlarm
        /// </summary>
        ValueAboveHiHiAlarm,
        /// <summary>
        /// ValueBelowLoLoAlarm
        /// </summary>
        ValueBelowLoLoAlarm,
        /// <summary>
        /// ValueAboveHiAlarm
        /// </summary>
        ValueAboveHiAlarm,
        /// <summary>
        /// ValueBelowLoAlarm
        /// </summary>
        ValueBelowLoAlarm,
        /// <summary>
        /// DeletedFromAlarmChecks
        /// </summary>
        DeletedFromAlarmChecks,
        /// <summary>
        /// InhibitedByCutoutPoint
        /// </summary>
        InhibitedByCutoutPoint,
        /// <summary>
        /// Good
        /// </summary>
        Good
    }

    #endregion

    /// <summary>
    /// Defines time series data warehoused by a historian.
    /// </summary>
    /// <seealso cref="TimeTag"/>
    /// <seealso cref="Quality"/>
    public interface IDataPoint : ISupportBinaryImage
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the historian identifier of the time series data point.
        /// </summary>
        int HistorianID { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of the time series data point.
        /// </summary>
        TimeTag Time { get; set; }

        /// <summary>
        /// Gets or sets the value of the time series data point.
        /// </summary>
        float Value { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Quality"/> of the time series data point.
        /// </summary>
        Quality Quality { get; set; }

        #endregion
    }
}
