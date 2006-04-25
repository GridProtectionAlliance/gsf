' 04-24-06

Namespace Ssam

    <Serializable()> _
    Public Class SsamEvent

        Private m_entityID As String
        Private m_entityType As SsamEntityType
        Private m_eventType As SsamEventType
        Private m_errorNumber As String
        Private m_message As String
        Private m_description As String

        Public Enum SsamEventType As Integer
            ''' <summary>
            ''' This event reports a successful action on some entity.
            ''' </summary>
            ''' <remarks></remarks>
            Success = 1
            ''' <summary>
            ''' This event is a warning that something may be going wrong soon.
            ''' </summary>
            ''' <remarks></remarks>
            Warning = 2
            ''' <summary>
            ''' This event is an alarm that something has already gone wrong.
            ''' </summary>
            ''' <remarks></remarks>
            Alarm = 3
            ''' <summary>
            ''' This event reports an unexpected error in an application that may or may not matter.
            ''' </summary>
            ''' <remarks></remarks>
            [Error] = 4
            ''' <summary>
            ''' This event reports information that may be of interest to someone.
            ''' </summary>
            ''' <remarks></remarks>
            Information = 5
            ''' <summary>
            ''' This event reports an alarm notification that was sent that has not been acknowledged.
            ''' </summary>
            ''' <remarks></remarks>
            Escalation = 6
            ''' <summary>
            ''' This event reports a cluster failover on some process (informational).
            ''' </summary>
            ''' <remarks></remarks>
            Failover = 7
            ''' <summary>
            ''' This event halts the Ssam monitoring/dispatching process - remove later? [fixme]?
            ''' </summary>
            ''' <remarks></remarks>
            Quit = 8
            ''' <summary>
            ''' This action handles a synchronize-Ssam notification by synchronizing the monitor database with 
            ''' the system-configuration database.
            ''' </summary>
            ''' <remarks></remarks>
            Synchronize = 9
            ''' <summary>
            ''' This action handles a terminate-Ssam notification by rescheduling all events.
            ''' </summary>
            ''' <remarks></remarks>
            Sched = 10
            ''' <summary>
            ''' This action makes the monitor skip old events, reschedule, and return to real-time processing.
            ''' </summary>
            ''' <remarks></remarks>
            CatchUp = 11
        End Enum

        Public Enum SsamEntityType As Integer
            ''' <summary>
            ''' This entity type represents a data-flow.
            ''' </summary>
            ''' <remarks></remarks>
            Flow = 1
            ''' <summary>
            ''' This entity type represents a piece of equipment.
            ''' </summary>
            ''' <remarks></remarks>
            Equipment = 2
            ''' <summary>
            ''' This entity type represents a Process.
            ''' </summary>
            ''' <remarks></remarks>
            Process = 3
            ''' <summary>
            ''' This entity type represents a System.
            ''' </summary>
            ''' <remarks></remarks>
            System = 4
            ''' <summary>
            ''' This entity type represents a data item like a file or table.
            ''' </summary>
            ''' <remarks></remarks>
            Data = 5
        End Enum

        Public Sub New(ByVal entityID As String, ByVal entityType As SsamEntityType, ByVal eventType As SsamEventType)
            MyClass.New(entityID, entityType, eventType, "", "", "")
        End Sub

        Public Sub New(ByVal entityID As String, ByVal entityType As SsamEntityType, ByVal eventType As SsamEventType, _
                ByVal errorNumber As String, ByVal message As String, ByVal description As String)
            MyBase.New()
            m_entityID = entityID
            m_entityType = entityType
            m_eventType = eventType
            m_errorNumber = errorNumber
            m_message = message
            m_description = description
        End Sub

        Public Property EntityId() As String
            Get
                Return m_entityID
            End Get
            Set(ByVal value As String)
                m_entityID = value
            End Set
        End Property

        Public Property EntityType() As SsamEntityType
            Get
                Return m_entityType
            End Get
            Set(ByVal value As SsamEntityType)
                m_entityType = value
            End Set
        End Property

        Public Property EventType() As SsamEventType
            Get
                Return m_eventType
            End Get
            Set(ByVal value As SsamEventType)
                m_eventType = value
            End Set
        End Property

        Public Property ErrorNumber() As String
            Get
                Return m_errorNumber
            End Get
            Set(ByVal value As String)
                m_errorNumber = value
            End Set
        End Property

        Public Property Message() As String
            Get
                Return m_message
            End Get
            Set(ByVal value As String)
                m_message = value
            End Set
        End Property

        Public Property Description() As String
            Get
                Return m_description
            End Get
            Set(ByVal value As String)
                m_description = value
            End Set
        End Property

    End Class

End Namespace