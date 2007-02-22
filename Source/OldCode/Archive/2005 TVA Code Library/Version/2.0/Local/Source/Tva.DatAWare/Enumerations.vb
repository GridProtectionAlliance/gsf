Public Enum Quality As Int32
    Unknown
    DeletedFromProcessing
    CouldNotCalculatePoint
    DASFrontEndHardwareError
    SensorReadError
    OpenTransducerDetection
    InputCountsOutOfSensorRange
    UnreasonableHigh
    UnreasonableLow
    Old
    SuspectValueAboveHiHiLimit
    SuspectValueBelowLoLoLimit
    SuspectValueAboveHiLimit
    SuspectValueBelowLoLimit
    SuspectData
    DigitalSuspectAlarm
    InsertedValueAboveHiHiLimit
    InsertedValueBelowLoLoLimit
    InsertedValueAboveHiLimit
    InsertedValueBelowLoLimit
    InsertedValue
    DigitalInsertedStatusInAlarm
    LogicalAlarm
    ValueAboveHiHiAlarm
    ValueBelowLoLoAlarm
    ValueAboveHiAlarm
    ValueBelowLoAlarm
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