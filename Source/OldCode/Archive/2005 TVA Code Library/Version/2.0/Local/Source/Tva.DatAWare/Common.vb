'*******************************************************************************************************
'  Common.vb - Common DatAWare declarations and functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/03/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text

Public Enum AccessMode
    [ReadOnly] = 1
    [WriteOnly] = 2
    [ReadWrite] = 3
End Enum

Public Enum Quality
    Unknown
    DeletedFromProcessing
    CouldNotCalcPoint
    DASFrontEndHardwareError
    SensorReadError
    OpenTransducerDetection
    InputCountsOutOfSensorRange
    UnreasonableHigh
    UnreasonableLow
    Old
    SuspectValueAboveHIHILimit
    SuspectValueBelowLOLOLimit
    SuspectValueAboveHILimit
    SuspectValueBelowLOLimit
    SuspectData
    DigitalSuspectAlarm
    InsertedValueAboveHIHILimit
    InsertedValueBelowLOLOLimit
    InsertedValueAboveHILimit
    InsertedValueBelowLOLimit
    InsertedValue
    DigitalInsertedStatusInAlarm
    LogicalAlarm
    ValueAboveHIHIAlarm
    ValueBelowLOLOAlarm
    ValueAboveHIAlarm
    ValueBelowLOAlarm
    DeletedFromAlarmChecks
    InhibitedByCutoutPoint
    Good
End Enum

Public Enum ReturnStatus
    Normal
    Abnormal
    Timeout
    NotConnected
End Enum

Public Enum RequestType
    Raw = 1
    Interpolated = 3
    Averaged = 4
End Enum

Public Enum ReturnType
    [Both]
    [Value]
    [Structure]
End Enum

Public NotInheritable Class Common

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ' TODO: Define any legacy constants here...

End Class
