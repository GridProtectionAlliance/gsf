' 03/07/2007

Namespace Interop

    Public Class VBArrayDescriptor
        Implements IBinaryDataProvider

#Region " Member Declaration "

        Private m_arrayDimensionDescriptors As List(Of DimensionDescriptor)

#End Region

#Region " Public Code "

        Public Sub New(ByVal arrayLengths As Integer(), ByVal arrayLowerBounds As Integer())

            MyBase.New()
            If arrayLengths.Length = arrayLowerBounds.Length Then
                m_arrayDimensionDescriptors = New List(Of DimensionDescriptor)()
                For i As Integer = 0 To arrayLengths.Length - 1
                    m_arrayDimensionDescriptors.Add(New DimensionDescriptor(arrayLengths(i), arrayLowerBounds(i)))
                Next
            Else
                Throw New ArgumentException("Number of lengths and lower bounds must be the same.")
            End If

        End Sub

#Region " Shared Code "

        Public Shared ReadOnly Property ZeroBasedOneDimensionalArray(ByVal arrayLength As Integer) As VBArrayDescriptor
            Get
                Return New VBArrayDescriptor(New Integer() {arrayLength}, New Integer() {0})
            End Get
        End Property

        Public Shared ReadOnly Property OneBasedOneDimensionalArray(ByVal arrayLength As Integer) As VBArrayDescriptor
            Get
                Return New VBArrayDescriptor(New Integer() {arrayLength}, New Integer() {1})
            End Get
        End Property

        Public Shared ReadOnly Property ZeroBasedTwoDimensionalArray(ByVal dimensionOneLength As Integer, ByVal dimensionTwoLength As Integer) As VBArrayDescriptor
            Get
                Return New VBArrayDescriptor(New Integer() {dimensionOneLength, dimensionTwoLength}, New Integer() {0, 0})
            End Get
        End Property

        Public Shared ReadOnly Property OneBasedTwoDimensionalArray(ByVal dimensionOneLength As Integer, ByVal dimensionTwoLength As Integer) As VBArrayDescriptor
            Get
                Return New VBArrayDescriptor(New Integer() {dimensionOneLength, dimensionTwoLength}, New Integer() {1, 1})
            End Get
        End Property

#End Region

#Region " IBinaryDataProvider Implementation "

        Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
            Get
                Dim image As Byte() = Tva.Common.CreateArray(Of Byte)(Me.BinaryLength)

                Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors.Count), 0, image, 0, 2)
                For i As Integer = 0 To m_arrayDimensionDescriptors.Count - 1
                    Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors(i).Length), 0, image, _
                        (i * DimensionDescriptor.BinaryLength) + 2, 4)
                    Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors(i).LowerBound), 0, image, _
                        (i * DimensionDescriptor.BinaryLength) + 6, 4)
                Next

                Return image
            End Get
        End Property

        Public ReadOnly Property BinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
            Get
                Return 2 + 8 * m_arrayDimensionDescriptors.Count
            End Get
        End Property

#End Region

#End Region

#Region " Private Code "

        Private Class DimensionDescriptor

            Public Sub New(ByVal dimensionLength As Integer, ByVal dimensionLowerBound As Integer)

                Length = dimensionLength
                LowerBound = dimensionLowerBound

            End Sub

            Public Length As Integer

            Public LowerBound As Integer

            Public Const BinaryLength As Integer = 8

        End Class

#End Region

    End Class

End Namespace