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

''' <summary>
''' Specifies the action to be taken on the packet.
''' </summary>
Public Enum PacketActionType
    ''' <summary>
    ''' The packet is to be saved and no reply to the sender is required.
    ''' </summary>
    SaveOnly
    ''' <summary>
    ''' A reply is to be sent to the sender and the packet is not to be saved anywhere.
    ''' </summary>
    ReplyOnly
    ''' <summary>
    ''' The packet is to be saved and a reply is to be sent to the sender.
    ''' </summary>
    SaveAndReply
End Enum