' 03/08/2007

Imports System.Drawing
Imports System.ComponentModel

Namespace Files

    <ToolboxBitmap(GetType(MetadataFile)), DisplayName("DwMetadataFile")> _
    Public Class MetadataFile

#Region " Code Scope: Public "

        Public Overrides ReadOnly Property RecordSize() As Integer
            Get
                Return PointDefinition.Size
            End Get
        End Property

        Public Overloads Overrides Function NewRecord(ByVal id As Integer) As PointDefinition

            Return New PointDefinition(id)

        End Function

        Public Overloads Overrides Function NewRecord(ByVal id As Integer, ByVal binaryImage() As Byte) As PointDefinition

            Return New PointDefinition(id, binaryImage)

        End Function

        Public Overloads Function Read(ByVal name As String) As PointDefinition

            If IsOpen Then
                Dim records As List(Of PointDefinition) = Read()
                For i As Integer = 0 To records.Count - 1
                    If String.Compare(name, records(i).Name) = 0 OrElse _
                            String.Compare(name, records(i).Synonym1) = 0 OrElse _
                            String.Compare(name, records(i).Synonym2) = 0 Then
                        Return records(i)
                    End If
                Next

                Return Nothing
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, name))
            End If

        End Function

#End Region

    End Class

End Namespace