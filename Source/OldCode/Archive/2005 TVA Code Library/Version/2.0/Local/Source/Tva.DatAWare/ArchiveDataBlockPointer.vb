' 02/18/2007

Public Class ArchiveDataBlockPointer
    Implements IComparable

    Public Const BinaryLength As Integer = 12

    Private m_pointID As Integer
    Private m_startTime As TimeTag

    Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        If binaryImage IsNot Nothing Then
            If binaryImage.Length - startIndex >= BinaryLength Then
                m_pointID = BitConverter.ToInt32(binaryImage, startIndex)
                m_startTime = New TimeTag(BitConverter.ToDouble(binaryImage, startIndex + 4))
            Else
                Throw New ArgumentException("Binary image size from startIndex is too small.    ")
            End If
        Else
            Throw New ArgumentNullException("binaryImage")
        End If

    End Sub

    Public Property PointID() As Integer
        Get
            Return m_pointID
        End Get
        Set(ByVal value As Integer)
            m_pointID = value
        End Set
    End Property

    Public Property StartTime() As TimeTag
        Get
            Return m_startTime
        End Get
        Set(ByVal value As TimeTag)
            m_startTime = value
        End Set
    End Property

    Public ReadOnly Property BinaryImage() As Byte()
        Get

        End Get
    End Property

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        Dim other As ArchiveDataBlockPointer = TryCast(obj, ArchiveDataBlockPointer)
        If other IsNot Nothing Then
            Return m_pointID.CompareTo(other.PointID) And m_startTime.CompareTo(other.StartTime)
        Else
            Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
        End If

    End Function

End Class
