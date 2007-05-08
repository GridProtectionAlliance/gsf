' 03/09/2007

Imports System.Drawing

Namespace Files

    <ToolboxBitmap(GetType(DwIntercomFile))> _
    Public Class DwIntercomFile

#Region " Code Scope: Public "

        Public Overrides ReadOnly Property RecordSize() As Integer
            Get
                Return EnvironmentData.Size
            End Get
        End Property

        Public Overloads Overrides Function NewRecord(ByVal id As Integer) As EnvironmentData

            Return New EnvironmentData(id)

        End Function

        Public Overloads Overrides Function NewRecord(ByVal id As Integer, ByVal binaryImage() As Byte) As EnvironmentData

            Return New EnvironmentData(id, binaryImage)

        End Function

#End Region

    End Class

End Namespace