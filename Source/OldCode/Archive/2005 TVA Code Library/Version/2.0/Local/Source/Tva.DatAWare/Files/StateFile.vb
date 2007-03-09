' 03/08/2007

Imports System.Drawing

Namespace Files

    <ToolboxBitmap(GetType(StateFile))> _
    Public Class StateFile

        Public Overloads Overrides Function NewRecord(ByVal id As Integer) As PointState

            Return New PointState(id)

        End Function

        Public Overloads Overrides Function NewRecord(ByVal id As Integer, ByVal binaryImage() As Byte) As PointState

            Return New PointState(id, binaryImage)

        End Function

        Public Overrides ReadOnly Property RecordSize() As Integer
            Get
                Return PointState.Size
            End Get
        End Property

    End Class

End Namespace