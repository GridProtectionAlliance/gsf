' 02/14/2007

Imports System.Data

Namespace Services

    Public MustInherit Class MessageBase

        Private m_messageName As String
        Private m_messageData As DataSet

        Public MustOverride Function GetData() As DataSet

        Protected MustOverride Function GetExportInformation() As DataTable
        Protected MustOverride Function GetSourceInformation() As DataTable
        Protected MustOverride Function GetExceptions() As DataTable

        Public Sub New(ByVal message As String)
            Me.m_messageName = message
            m_messageData = New DataSet(message)

        End Sub

        Public Function BuildMessageData() As String

            m_messageData.Merge(GetData())
            m_messageData.Tables.Add(GetExceptions())
            m_messageData.Tables.Add(GetExportInformation())
            m_messageData.Tables.Add(GetSourceInformation())

            Return m_messageData.GetXml()

        End Function


    End Class

End Namespace