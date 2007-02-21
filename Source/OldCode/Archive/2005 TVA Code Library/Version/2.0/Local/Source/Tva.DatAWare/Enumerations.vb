Public Enum Quality As Int32
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

Public Enum PointType As Int32
    Analog
    Digital
    Composed
    Constant
End Enum