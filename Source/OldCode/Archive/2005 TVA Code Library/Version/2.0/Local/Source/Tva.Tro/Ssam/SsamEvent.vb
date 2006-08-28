'*******************************************************************************************************
'  Tva.Tro.Ssam.SsamEvent.vb - SSAM Event
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/24/2006 - Pinal C. Patel
'       Original version of source code generated
'  08/25/2006 - Pinal C. Patel
'       Moved SsamEntityType and SsamEventType enumerations to Enumerations.vb.
'*******************************************************************************************************

Namespace Ssam

    ''' <summary>
    ''' Defines an event that can be logged to SSAM.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class SsamEvent

        Private m_entityID As String
        Private m_entityType As SsamEntityType
        Private m_eventType As SsamEventType
        Private m_errorNumber As String
        Private m_message As String
        Private m_description As String

        ''' <summary>
        ''' Initializes a instance of Tva.Tro.Ssam.SsamEvent with the specified information.
        ''' </summary>
        ''' <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
        ''' <param name="entityType">One of the Tva.Tro.Ssam.SsamEntityType values.</param>
        ''' <param name="eventType">One of the Tva.Tro.Ssam.SsamEvent.SsamEventType values.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal entityID As String, ByVal entityType As SsamEntityType, ByVal eventType As SsamEventType)
            MyClass.New(entityID, entityType, eventType, "", "", "")
        End Sub

        ''' <summary>
        ''' Initializes a instance of Tva.Tro.Ssam.SsamEvent with the specified information.
        ''' </summary>
        ''' <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
        ''' <param name="entityType">One of the Tva.Tro.Ssam.SsamEntityType values.</param>
        ''' <param name="eventType">One of the Tva.Tro.Ssam.SsamEvent.SsamEventType values.</param>
        ''' <param name="errorNumber">The error number encountered, if any, for which the event is being logged.</param>
        ''' <param name="message">A brief description of the event (max 120 characters).</param>
        ''' <param name="description">A detailed description of the event (max 2GB).</param>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' Gets or sets the mnemonic key or the numeric value of the entity to which this event belongs.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The mnemonic key or the numeric value of the entity to which this event belongs.</returns>
        ''' <remarks></remarks>
        Public Property EntityId() As String
            Get
                Return m_entityID
            End Get
            Set(ByVal value As String)
                m_entityID = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets type of the entity to which this event belongs.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Type of the entity to which this event belongs.</returns>
        ''' <remarks></remarks>
        Public Property EntityType() As SsamEntityType
            Get
                Return m_entityType
            End Get
            Set(ByVal value As SsamEntityType)
                m_entityType = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the type of this event.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The type of this event.</returns>
        ''' <remarks></remarks>
        Public Property EventType() As SsamEventType
            Get
                Return m_eventType
            End Get
            Set(ByVal value As SsamEventType)
                m_eventType = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the error number encountered, if any, for which this event is being logged.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The error number encountered, if any, for which this event is being logged.</returns>
        ''' <remarks></remarks>
        Public Property ErrorNumber() As String
            Get
                Return m_errorNumber
            End Get
            Set(ByVal value As String)
                m_errorNumber = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the brief description of this event (max 120 characters).
        ''' </summary>
        ''' <value></value>
        ''' <returns>The brief description of this event (max 120 characters).</returns>
        ''' <remarks></remarks>
        Public Property Message() As String
            Get
                Return m_message
            End Get
            Set(ByVal value As String)
                m_message = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the detailed description of this event (max 2GB).
        ''' </summary>
        ''' <value></value>
        ''' <returns>The detailed description of this event (max 2GB).</returns>
        ''' <remarks></remarks>
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