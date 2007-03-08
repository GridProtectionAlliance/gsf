' 02/24/2007

Public Interface IPointData
    Inherits IBinaryDataProvider

    Property TimeTag() As TimeTag

    Property Value() As Single

    Property Quality() As Quality

    Property Flags() As Int32

    Property Definition() As PointDefinition

    ReadOnly Property IsNull() As Boolean

End Interface
