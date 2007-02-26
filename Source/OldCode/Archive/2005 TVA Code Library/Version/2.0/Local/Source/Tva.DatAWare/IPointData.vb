' 02/24/2007

Public Interface IPointData

    Property TTag() As TimeTag

    Property Value() As Single

    Property Quality() As Quality

    Property Flags() As Int32

    Property Definition() As PointDefinition

    ReadOnly Property IsNull() As Boolean

    ReadOnly Property BinaryImage() As Byte()

End Interface
