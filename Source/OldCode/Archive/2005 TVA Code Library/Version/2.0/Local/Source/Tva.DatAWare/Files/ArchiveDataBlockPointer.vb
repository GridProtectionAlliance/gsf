' 02/18/2007

Namespace Files

    Public Class ArchiveDataBlockPointer
        Implements IComparable, IBinaryDataProvider

#Region " Member Declaration "

        Private m_index As Integer
        Private m_pointID As Integer
        Private m_startTime As TimeTag

#End Region

#Region " Public Code "

        Public Const Size As Integer = 12

        'Public Sub New()

        '    MyClass.New(-1, TimeTag.MinValue)

        'End Sub

        Public Sub New(ByVal index As Integer)

            MyClass.New(index, -1, TimeTag.MinValue)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal pointID As Integer, ByVal startTime As TimeTag)

            MyBase.New()
            m_index = index
            m_pointID = pointID
            m_startTime = startTime

        End Sub

        Public Sub New(ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            If binaryImage IsNot Nothing Then
                If binaryImage.Length - startIndex >= Size Then
                    m_index = index
                    m_pointID = BitConverter.ToInt32(binaryImage, startIndex)
                    m_startTime = New TimeTag(BitConverter.ToDouble(binaryImage, startIndex + 4))
                Else
                    Throw New ArgumentException("Binary image size from startIndex is too small.")
                End If
            Else
                Throw New ArgumentNullException("binaryImage")
            End If

        End Sub

        Public ReadOnly Property Index() As Integer
            Get
                Return m_index
            End Get
        End Property

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

        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Return CompareTo(obj) = 0

        End Function

#Region " IComparable Implementation "

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            Dim other As ArchiveDataBlockPointer = TryCast(obj, ArchiveDataBlockPointer)
            If other IsNot Nothing Then
                Dim pointIDCompare As Integer = m_pointID.CompareTo(other.PointID)
                Return IIf(pointIDCompare = 0, m_startTime.CompareTo(other.StartTime), pointIDCompare)
            Else
                Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
            End If

        End Function

#End Region

#Region " IBinaryDataProvider Implementation "

        Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
            Get
                Dim image As Byte() = CreateArray(Of Byte)(Size)

                Array.Copy(BitConverter.GetBytes(m_pointID), 0, image, 0, 4)
                Array.Copy(BitConverter.GetBytes(m_startTime.Value), 0, image, 4, 8)

                Return image
            End Get
        End Property

        Public ReadOnly Property BinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
            Get
                Return Size
            End Get
        End Property

#End Region

#End Region

    End Class

End Namespace