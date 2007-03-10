' 02/24/2007

Imports Tva.Interop.Bit
Imports Tva.Measurements

Public MustInherit Class PointDataBase
    Implements IPointData, IComparable 'IMeasurement 

#Region " Member Declaration "

    Private m_timeTag As TimeTag
    Private m_value As Single
    Private m_flags As Int32
    Private m_definition As PointDefinition

    Private Const QualityMask As Int32 = Bit0 Or Bit1 Or Bit2 Or Bit3 Or Bit4

#End Region

#Region " Public Code "

    Public Const Size As Integer = -1

    Public Sub New()

        MyBase.New()
        m_timeTag = TimeTag.MinValue

    End Sub

    Public Sub New(ByVal timeTag As TimeTag, ByVal value As Single, ByVal quality As Quality)

        MyClass.New()
        m_timeTag = timeTag
        m_value = value
        Quality = quality

    End Sub

    Public Overridable Property TimeTag() As TimeTag Implements IPointData.TimeTag
        Get
            Return m_timeTag
        End Get
        Set(ByVal value As TimeTag)
            m_timeTag = value
        End Set
    End Property

    Public Overridable Property Value() As Single Implements IPointData.Value
        Get
            Return m_value
        End Get
        Set(ByVal value As Single)
            m_value = value
        End Set
    End Property

    Public Overridable Property Quality() As Quality Implements IPointData.Quality
        Get
            Return CType((m_flags And QualityMask), Quality)
        End Get
        Set(ByVal value As Quality)
            m_flags = (m_flags And Not QualityMask Or value)
        End Set
    End Property

    Public Property Flags() As Integer Implements IPointData.Flags
        Get
            Return m_flags
        End Get
        Set(ByVal value As Int32)
            m_flags = value
        End Set
    End Property

    Public Property Definition() As PointDefinition Implements IPointData.Definition
        Get
            Return m_definition
        End Get
        Set(ByVal value As PointDefinition)
            m_definition = value
        End Set
    End Property

    Public ReadOnly Property IsNull() As Boolean Implements IPointData.IsNull
        Get
            Return m_timeTag.CompareTo(TimeTag.MinValue) = 0
        End Get
    End Property

    Public Overrides Function Equals(ByVal obj As Object) As Boolean

        Return CompareTo(obj) = 0

    End Function

#Region " IComparable Implementation "

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        Dim other As PointDataBase = TryCast(obj, PointDataBase)
        If other IsNot Nothing Then
            Return m_timeTag.CompareTo(other.TimeTag)
        Else
            Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
        End If

    End Function

#End Region

#Region " IMeasurement Implementation "

    'Public Function CompareTo1(ByVal other As Measurements.IMeasurement) As Integer Implements System.IComparable(Of Measurements.IMeasurement).CompareTo

    'End Function

    'Public Function Equals1(ByVal other As Measurements.IMeasurement) As Boolean Implements System.IEquatable(Of Measurements.IMeasurement).Equals

    'End Function

    'Public Property Adder() As Double Implements Measurements.IMeasurement.Adder
    '    Get

    '    End Get
    '    Set(ByVal value As Double)

    '    End Set
    'End Property

    'Public ReadOnly Property AdjustedValue() As Double Implements Measurements.IMeasurement.AdjustedValue
    '    Get

    '    End Get
    'End Property

    'Public Property ID() As Integer Implements Measurements.IMeasurement.ID
    '    Get

    '    End Get
    '    Set(ByVal value As Integer)

    '    End Set
    'End Property

    'Public ReadOnly Property Key() As Measurements.MeasurementKey Implements Measurements.IMeasurement.Key
    '    Get

    '    End Get
    'End Property

    'Public Property Multiplier() As Double Implements Measurements.IMeasurement.Multiplier
    '    Get

    '    End Get
    '    Set(ByVal value As Double)

    '    End Set
    'End Property

    'Public Property Source() As String Implements Measurements.IMeasurement.Source
    '    Get

    '    End Get
    '    Set(ByVal value As String)

    '    End Set
    'End Property

    'Public Property Tag() As String Implements Measurements.IMeasurement.Tag
    '    Get

    '    End Get
    '    Set(ByVal value As String)

    '    End Set
    'End Property

    'Public ReadOnly Property This() As Measurements.IMeasurement Implements Measurements.IMeasurement.This
    '    Get

    '    End Get
    'End Property

    'Public Property Ticks() As Long Implements Measurements.IMeasurement.Ticks
    '    Get

    '    End Get
    '    Set(ByVal value As Long)

    '    End Set
    'End Property

    'Public ReadOnly Property Timestamp() As Date Implements Measurements.IMeasurement.Timestamp
    '    Get

    '    End Get
    'End Property

    'Public Property TimestampQualityIsGood() As Boolean Implements Measurements.IMeasurement.TimestampQualityIsGood
    '    Get

    '    End Get
    '    Set(ByVal value As Boolean)

    '    End Set
    'End Property

    'Public Property Value1() As Double Implements Measurements.IMeasurement.Value
    '    Get

    '    End Get
    '    Set(ByVal value As Double)

    '    End Set
    'End Property

    'Public Property ValueQualityIsGood() As Boolean Implements Measurements.IMeasurement.ValueQualityIsGood
    '    Get

    '    End Get
    '    Set(ByVal value As Boolean)

    '    End Set
    'End Property

#End Region

#Region " IBinaryDataProvider Implementation "

    Public MustOverride ReadOnly Property BinaryData() As Byte() Implements IBinaryDataProvider.BinaryData

    Public MustOverride ReadOnly Property BinaryDataLength() As Integer Implements IBinaryDataProvider.BinaryDataLength

#End Region

#End Region

End Class
