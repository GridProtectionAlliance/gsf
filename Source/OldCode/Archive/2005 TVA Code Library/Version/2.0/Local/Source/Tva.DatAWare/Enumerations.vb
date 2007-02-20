Public Enum Quality As Integer
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

Public Enum PointType As Short
    Analog
    Digital
    Composed
    Constant
End Enum