//******************************************************************************************************
//  MetadataRecordAlarmFlags.cs - Gbtc
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
//  02/22/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, GSF.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.Historian.Files;

/// <summary>
/// Defines which data <see cref="Quality"/> should trigger an alarm notification.
/// </summary>
/// <seealso cref="MetadataRecord"/>
public class MetadataRecordAlarmFlags
{
    #region [ Properties ]

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.Unknown"/> should trigger an alarm notification.
    /// </summary>
    public bool Unknown
    {
        get => Value.CheckBits(Bits.Bit00);
        set => Value = value ? Value.SetBits(Bits.Bit00) : Value.ClearBits(Bits.Bit00);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.DeletedFromProcessing"/> should trigger an alarm notification.
    /// </summary>
    public bool DeletedFromProcessing
    {
        get => Value.CheckBits(Bits.Bit01);
        set => Value = value ? Value.SetBits(Bits.Bit01) : Value.ClearBits(Bits.Bit01);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.CouldNotCalculate"/> should trigger an alarm notification.
    /// </summary>
    public bool CouldNotCalculate
    {
        get => Value.CheckBits(Bits.Bit02);
        set => Value = value ? Value.SetBits(Bits.Bit02) : Value.ClearBits(Bits.Bit02);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.FrontEndHardwareError"/> should trigger an alarm notification.
    /// </summary>
    public bool FrontEndHardwareError
    {
        get => Value.CheckBits(Bits.Bit03);
        set => Value = value ? Value.SetBits(Bits.Bit03) : Value.ClearBits(Bits.Bit03);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SensorReadError"/> should trigger an alarm notification.
    /// </summary>
    public bool SensorReadError
    {
        get => Value.CheckBits(Bits.Bit04);
        set => Value = value ? Value.SetBits(Bits.Bit04) : Value.ClearBits(Bits.Bit04);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.OpenThermocouple"/> should trigger an alarm notification.
    /// </summary>
    public bool OpenThermocouple
    {
        get => Value.CheckBits(Bits.Bit05);
        set => Value = value ? Value.SetBits(Bits.Bit05) : Value.ClearBits(Bits.Bit05);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InputCountsOutOfSensorRange"/> should trigger an alarm notification.
    /// </summary>
    public bool InputCountsOutOfSensorRange
    {
        get => Value.CheckBits(Bits.Bit06);
        set => Value = value ? Value.SetBits(Bits.Bit06) : Value.ClearBits(Bits.Bit06);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.UnreasonableHigh"/> should trigger an alarm notification.
    /// </summary>
    public bool UnreasonableHigh
    {
        get => Value.CheckBits(Bits.Bit07);
        set => Value = value ? Value.SetBits(Bits.Bit07) : Value.ClearBits(Bits.Bit07);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.UnreasonableLow"/> should trigger an alarm notification.
    /// </summary>
    public bool UnreasonableLow
    {
        get => Value.CheckBits(Bits.Bit08);
        set => Value = value ? Value.SetBits(Bits.Bit08) : Value.ClearBits(Bits.Bit08);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.Old"/> should trigger an alarm notification.
    /// </summary>
    public bool Old
    {
        get => Value.CheckBits(Bits.Bit09);
        set => Value = value ? Value.SetBits(Bits.Bit09) : Value.ClearBits(Bits.Bit09);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectValueAboveHiHiLimit"/> should trigger an alarm notification.
    /// </summary>
    public bool SuspectValueAboveHiHiLimit
    {
        get => Value.CheckBits(Bits.Bit10);
        set => Value = value ? Value.SetBits(Bits.Bit10) : Value.ClearBits(Bits.Bit10);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectValueBelowLoLoLimit"/> should trigger an alarm notification.
    /// </summary>
    public bool SuspectValueBelowLoLoLimit
    {
        get => Value.CheckBits(Bits.Bit11);
        set => Value = value ? Value.SetBits(Bits.Bit11) : Value.ClearBits(Bits.Bit11);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectValueAboveHiLimit"/> should trigger an alarm notification.
    /// </summary>
    public bool SuspectValueAboveHiLimit
    {
        get => Value.CheckBits(Bits.Bit12);
        set => Value = value ? Value.SetBits(Bits.Bit12) : Value.ClearBits(Bits.Bit12);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectValueBelowLoLimit"/> should trigger an alarm notification.
    /// </summary>
    public bool SuspectValueBelowLoLimit
    {
        get => Value.CheckBits(Bits.Bit13);
        set => Value = value ? Value.SetBits(Bits.Bit13) : Value.ClearBits(Bits.Bit13);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.SuspectData"/> should trigger an alarm notification.
    /// </summary>
    public bool SuspectData
    {
        get => Value.CheckBits(Bits.Bit14);
        set => Value = value ? Value.SetBits(Bits.Bit14) : Value.ClearBits(Bits.Bit14);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.DigitalSuspectAlarm"/> should trigger an alarm notification.
    /// </summary>
    public bool DigitalSuspectAlarm
    {
        get => Value.CheckBits(Bits.Bit15);
        set => Value = value ? Value.SetBits(Bits.Bit15) : Value.ClearBits(Bits.Bit15);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValueAboveHiHiLimit"/> should trigger an alarm notification.
    /// </summary>
    public bool InsertedValueAboveHiHiLimit
    {
        get => Value.CheckBits(Bits.Bit16);
        set => Value = value ? Value.SetBits(Bits.Bit16) : Value.ClearBits(Bits.Bit16);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValueBelowLoLoLimit"/> should trigger an alarm notification.
    /// </summary>
    public bool InsertedValueBelowLoLoLimit
    {
        get => Value.CheckBits(Bits.Bit17);
        set => Value = value ? Value.SetBits(Bits.Bit17) : Value.ClearBits(Bits.Bit17);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValueAboveHiLimit"/> should trigger an alarm notification.
    /// </summary>
    public bool InsertedValueAboveHiLimit
    {
        get => Value.CheckBits(Bits.Bit18);
        set => Value = value ? Value.SetBits(Bits.Bit18) : Value.ClearBits(Bits.Bit18);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValueBelowLoLimit"/> should trigger an alarm notification.
    /// </summary>
    public bool InsertedValueBelowLoLimit
    {
        get => Value.CheckBits(Bits.Bit19);
        set => Value = value ? Value.SetBits(Bits.Bit19) : Value.ClearBits(Bits.Bit19);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InsertedValue"/> should trigger an alarm notification.
    /// </summary>
    public bool InsertedValue
    {
        get => Value.CheckBits(Bits.Bit20);
        set => Value = value ? Value.SetBits(Bits.Bit20) : Value.ClearBits(Bits.Bit20);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.DigitalInsertedStatusInAlarm"/> should trigger an alarm notification.
    /// </summary>
    public bool DigitalInsertedStatusInAlarm
    {
        get => Value.CheckBits(Bits.Bit21);
        set => Value = value ? Value.SetBits(Bits.Bit21) : Value.ClearBits(Bits.Bit21);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.LogicalAlarm"/> should trigger an alarm notification.
    /// </summary>
    public bool LogicalAlarm
    {
        get => Value.CheckBits(Bits.Bit22);
        set => Value = value ? Value.SetBits(Bits.Bit22) : Value.ClearBits(Bits.Bit22);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.ValueAboveHiHiAlarm"/> should trigger an alarm notification.
    /// </summary>
    public bool ValueAboveHiHiAlarm
    {
        get => Value.CheckBits(Bits.Bit23);
        set => Value = value ? Value.SetBits(Bits.Bit23) : Value.ClearBits(Bits.Bit23);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.ValueBelowLoLoAlarm"/> should trigger an alarm notification.
    /// </summary>
    public bool ValueBelowLoLoAlarm
    {
        get => Value.CheckBits(Bits.Bit24);
        set => Value = value ? Value.SetBits(Bits.Bit24) : Value.ClearBits(Bits.Bit24);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.ValueAboveHiAlarm"/> should trigger an alarm notification.
    /// </summary>
    public bool ValueAboveHiAlarm
    {
        get => Value.CheckBits(Bits.Bit25);
        set => Value = value ? Value.SetBits(Bits.Bit25) : Value.ClearBits(Bits.Bit25);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.ValueBelowLoAlarm"/> should trigger an alarm notification.
    /// </summary>
    public bool ValueBelowLoAlarm
    {
        get => Value.CheckBits(Bits.Bit26);
        set => Value = value ? Value.SetBits(Bits.Bit26) : Value.ClearBits(Bits.Bit26);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.DeletedFromAlarmChecks"/> should trigger an alarm notification.
    /// </summary>
    public bool DeletedFromAlarmChecks
    {
        get => Value.CheckBits(Bits.Bit27);
        set => Value = value ? Value.SetBits(Bits.Bit27) : Value.ClearBits(Bits.Bit27);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.InhibitedByCutoutPoint"/> should trigger an alarm notification.
    /// </summary>
    public bool InhibitedByCutoutPoint
    {
        get => Value.CheckBits(Bits.Bit28);
        set => Value = value ? Value.SetBits(Bits.Bit28) : Value.ClearBits(Bits.Bit28);
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether a data <see cref="Quality"/> of <see cref="Quality.Good"/> should trigger an alarm notification.
    /// </summary>
    public bool Good
    {
        get => Value.CheckBits(Bits.Bit29);
        set => Value = value ? Value.SetBits(Bits.Bit29) : Value.ClearBits(Bits.Bit29);
    }

    /// <summary>
    /// Gets or sets the 32-bit integer value used for defining which data <see cref="Quality"/> should trigger an alarm notification.
    /// </summary>
    public int Value { get; set; }

    #endregion
}