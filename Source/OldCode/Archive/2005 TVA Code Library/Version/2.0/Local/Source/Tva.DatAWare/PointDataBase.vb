' 02/24/2007

Imports Tva.Interop.Bit

Public MustInherit Class PointDataBase
    Implements IPointData, IComparable

#Region " Member Declaration "

    Private m_tTag As TimeTag
    Private m_value As Single
    Private m_flags As Int32
    Private m_definition As PointDefinition

    Private Const QualityMask As Int32 = Bit0 Or Bit1 Or Bit2 Or Bit3 Or Bit4

#End Region

#Region " Public Code "

    Public Const BinaryLength As Integer = -1

    Public Overridable Property TTag() As TimeTag Implements IPointData.TTag
        Get
            Return m_tTag
        End Get
        Set(ByVal value As TimeTag)
            m_tTag = value
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

    Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IPointData.BinaryImage

#Region " IComparable Implementation "

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        Dim other As PointDataBase = TryCast(obj, PointDataBase)
        If other IsNot Nothing Then
            Return m_tTag.CompareTo(other.TTag)
        Else
            Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
        End If

    End Function

#End Region

#End Region

End Class
