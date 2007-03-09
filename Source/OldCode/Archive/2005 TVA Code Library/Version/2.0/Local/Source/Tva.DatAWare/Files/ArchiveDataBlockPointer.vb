' 02/18/2007

Namespace Files

    Public Class ArchiveDataBlockPointer
        Implements IComparable, IBinaryDataProvider

#Region " Member Declaration "

        Private m_pointIndex As Integer
        Private m_startTime As TimeTag

#End Region

#Region " Public Code "

        Public Const Size As Integer = 12

        Public Sub New()

            MyClass.New(-1, TimeTag.MinValue)

        End Sub

        Public Sub New(ByVal pointIndex As Integer, ByVal startTime As TimeTag)

            m_pointIndex = pointIndex
            m_startTime = startTime

        End Sub

        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            If binaryImage IsNot Nothing Then
                If binaryImage.Length - startIndex >= Size Then
                    m_pointIndex = BitConverter.ToInt32(binaryImage, startIndex)
                    m_startTime = New TimeTag(BitConverter.ToDouble(binaryImage, startIndex + 4))
                Else
                    Throw New ArgumentException("Binary image size from startIndex is too small.")
                End If
            Else
                Throw New ArgumentNullException("binaryImage")
            End If

        End Sub

        Public Property PointIndex() As Integer
            Get
                Return m_pointIndex
            End Get
            Set(ByVal value As Integer)
                m_pointIndex = value
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
                Dim pointIndexCompare As Integer = m_pointIndex.CompareTo(other.PointIndex)
                Return IIf(pointIndexCompare = 0, m_startTime.CompareTo(other.StartTime), pointIndexCompare)
            Else
                Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
            End If

        End Function

#End Region

#Region " IBinaryDataProvider Implementation "

        Public ReadOnly Property BinaryData() As Byte() Implements IBinaryDataProvider.BinaryData
            Get
                Dim data As Byte() = CreateArray(Of Byte)(Size)

                Array.Copy(BitConverter.GetBytes(m_pointIndex), 0, data, 0, 4)
                Array.Copy(BitConverter.GetBytes(m_startTime.Value), 0, data, 4, 8)

                Return data
            End Get
        End Property

        Public ReadOnly Property BinaryDataLength() As Integer Implements IBinaryDataProvider.BinaryDataLength
            Get
                Return Size
            End Get
        End Property

#End Region

#End Region

    End Class

End Namespace