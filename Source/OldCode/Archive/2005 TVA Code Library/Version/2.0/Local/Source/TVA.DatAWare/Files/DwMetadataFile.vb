' 03/08/2007

Imports System.Drawing

Namespace Files

    <ToolboxBitmap(GetType(DwMetadataFile))> _
    Public Class DwMetadataFile

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
                For i As Integer = 0 To Records.Count - 1
                    If String.Compare(name, Records(i).Name) = 0 OrElse _
                            String.Compare(name, Records(i).Synonym1) = 0 OrElse _
                            String.Compare(name, Records(i).Synonym2) = 0 Then
                        Return Records(i)
                    End If
                Next

                Return Nothing
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, Name))
            End If

        End Function

        Public Overrides Sub Write(ByVal record As PointDefinition)

            If IsOpen Then
                If record IsNot Nothing Then
                    If record.PointID > Records.Count + 1 Then
                        ' We must add blank definitions as place holders before inserting the point definition.
                        For i As Integer = Records.Count + 1 To record.PointID - 1
                            Records.Add(New PointDefinition(i))
                        Next
                    End If

                    MyBase.Write(record)
                Else
                    Throw New ArgumentNullException("record")
                End If
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, Name))
            End If

        End Sub

#End Region

    End Class

End Namespace