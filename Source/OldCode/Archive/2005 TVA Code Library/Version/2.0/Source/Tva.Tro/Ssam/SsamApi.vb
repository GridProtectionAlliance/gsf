' 04-24-06

Imports System.ComponentModel

Namespace Ssam

    <TypeConverter(GetType(ExpandableObjectConverter))> _
    Public Class SsamApi

        Private m_server As SsamServer
        Private m_connectionState As SsamConnectionStates
        Private m_connectionString As String

        Public Enum SsamServer As Integer
            Development
            Acceptance
            Production
        End Enum

        <Flags()> _
        Public Enum SsamConnectionStates As Integer
            Open = 1
            Closed = 2
            Active = 4
            Inactive = 8
        End Enum

        Public Property Server() As SsamServer
            Get
                Return m_server
            End Get
            Set(ByVal value As SsamServer)
                m_server = value
            End Set
        End Property

        Public ReadOnly Property ConnectionState() As SsamConnectionStates
            Get
                Return m_connectionState
            End Get
        End Property

        Public ReadOnly Property ConnectionString() As String
            Get
                Return m_connectionString
            End Get
        End Property

        Public Sub Connect()

        End Sub

        Public Sub Disconnect()

        End Sub

        Public Function LogEvent(ByVal newEvent As SsamEvent) As Boolean

        End Function

    End Class

End Namespace