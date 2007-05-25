' 03/08/2007

Imports System.Drawing
Imports System.ComponentModel

Namespace Files

    <ToolboxBitmap(GetType(StateFile)), DisplayName("DW State File")> _
    Public Class StateFile

        Public Overrides ReadOnly Property RecordSize() As Integer
            Get
                Return PointState.Size
            End Get
        End Property

        Public Overloads Overrides Function NewRecord(ByVal id As Integer) As PointState

            Return New PointState(id)

        End Function

        Public Overloads Overrides Function NewRecord(ByVal id As Integer, ByVal binaryImage() As Byte) As PointState

            Return New PointState(id, binaryImage)

        End Function

    End Class

End Namespace