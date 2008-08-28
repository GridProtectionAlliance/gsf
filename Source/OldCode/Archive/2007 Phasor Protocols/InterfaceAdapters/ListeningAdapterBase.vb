'*******************************************************************************************************
'  ListeningAdapterBase.vb - Incoming data adpater base class
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/01/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports System.Runtime.CompilerServices
Imports TVA.Measurements

Public MustInherit Class ListeningAdapterBase

    Inherits AdapterBase
    Implements IListeningAdapter

    ''' <summary>This event will be raised when there are new measurements available from the data source</summary>
    Public Event NewMeasurements(ByVal measurements As ICollection(Of IMeasurement)) Implements IListeningAdapter.NewMeasurements

    Private m_processedMeasurements As Long
    Private WithEvents m_connectionTimer As Timers.Timer
    Private m_disposed As Boolean

    Private Const ProcessedMeasurementInterval As Integer = 100000

    Public Sub New()

        m_connectionTimer = New Timers.Timer

        With m_connectionTimer
            .AutoReset = False
            .Interval = 2000
            .Enabled = False
        End With

    End Sub

    Public MustOverride Sub Initialize(ByVal connectionString As String) Implements IListeningAdapter.Initialize

    Public Sub Connect() Implements IListeningAdapter.Connect

        ' Make sure we are disconnected before attempting a connection
        Disconnect()

        ' Start the connection cycle
        m_connectionTimer.Enabled = True

    End Sub

    Protected MustOverride Sub AttemptConnection()

    Private Sub m_connectionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_connectionTimer.Elapsed

        Try
            UpdateStatus(String.Format("Starting connection attempt to {0}...", Name))

            ' Attempt connection to data source (consumer to call API connect functions)
            AttemptConnection()

            UpdateStatus(String.Format("Connection to {0} established.", Name))
        Catch ex As Exception
            UpdateStatus(String.Format("WARNING: Connection to {0} failed: {1}", Name, ex.Message))
            Connect()
        End Try

    End Sub

    Protected MustOverride Sub AttemptDisconnection()

    Public Sub Disconnect() Implements IListeningAdapter.Disconnect

        Try
            ' Attempt disconnection from data source (consumer to call API disconnect functions)
            AttemptDisconnection()

            UpdateStatus(String.Format("Disconnected from {0}", Name))
        Catch ex As Exception
            UpdateStatus(String.Format("Exception occured during disconnect from {0}: {1}", Name, ex.Message))
        End Try

    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)

        If Not m_disposed Then
            If disposing Then
                If m_connectionTimer IsNot Nothing Then m_connectionTimer.Dispose()
                m_connectionTimer = Nothing
                Disconnect()
            End If
        End If

        m_disposed = True

    End Sub

    Protected MustOverride ReadOnly Property IsConnected() As Boolean

    <MethodImpl(MethodImplOptions.Synchronized)> _
    Private Sub IncrementProcessedMeasurements(ByVal totalAdded As Long)

        ' Check to see if total number of added points will exceed process interval used to show periodic
        ' messages of how many points have been archived so far...
        Dim showMessage As Boolean = (m_processedMeasurements + totalAdded >= (m_processedMeasurements \ ProcessedMeasurementInterval + 1) * ProcessedMeasurementInterval)

        m_processedMeasurements += totalAdded

        If showMessage Then UpdateStatus(String.Format("{0:N0} measurements have been parsed so far...", m_processedMeasurements))

    End Sub

    Protected Property ConnectionAttemptInterval() As Integer
        Get
            Return m_connectionTimer.Interval
        End Get
        Set(ByVal value As Integer)
            m_connectionTimer.Interval = value
        End Set
    End Property

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("    Data source connection: ")
                If IsConnected() Then
                    .Append("Inactive - not connected")
                Else
                    .Append("Active")
                End If
                .AppendLine()
                .Append("    Processed measurements: ")
                .Append(m_processedMeasurements)
                .AppendLine()
                Return .ToString()
            End With
        End Get
    End Property

    Protected Sub PublishNewEvents(ByVal measurements As ICollection(Of IMeasurement))

        RaiseEvent NewMeasurements(measurements)
        IncrementProcessedMeasurements(measurements.Count)

    End Sub

End Class
