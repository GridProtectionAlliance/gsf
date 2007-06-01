'*******************************************************************************************************
'  PmuInfo.vb - PMU Information Definition
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/22/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports TVA.Common
Imports TVA.Measurements

Public Class PmuInfo

    Implements IComparable(Of PmuInfo)

    Private m_id As UInt16
    Private m_acronym As String
    Private m_lastReportTime As Long
    Private m_signalSynonyms As Dictionary(Of SignalType, String())
    'Private m_activeMeasurements As Dictionary(Of String, IMeasurement)

    Public Sub New(ByVal id As UInt16, ByVal acronym As String)

        m_id = id
        m_acronym = acronym
        m_signalSynonyms = New Dictionary(Of SignalType, String())

    End Sub

    Public ReadOnly Property ID() As UInt16
        Get
            Return m_id
        End Get
    End Property

    Public ReadOnly Property Acronym() As String
        Get
            Return m_acronym
        End Get
    End Property

    Public ReadOnly Property SignalSynonym(ByVal signal As SignalType) As String
        Get
            ' We cache signal reference strings so they don't need to be generated at each mapping call
            ' This helps with performance since the mappings for each signal occur 30 times per second
            Dim signalRef As String = Nothing
            Dim synonyms As String()

            ' Looks up synonym dictionary based on signal type
            If m_signalSynonyms.TryGetValue(signal, synonyms) Then
                If synonyms(0) Is Nothing Then
                    ' Didn't find signal index, create and cache new signal reference
                    signalRef = SignalReference.ToString(m_acronym, signal)
                    synonyms(0) = signalRef
                Else
                    signalRef = synonyms(0)
                End If
            Else
                ' Create a new indexed signal reference array
                synonyms = CreateArray(Of String)(1)

                ' Create and cache new signal reference
                signalRef = SignalReference.ToString(m_acronym, signal)
                synonyms(0) = signalRef
            End If

            Return signalRef
        End Get
    End Property

    Public ReadOnly Property SignalSynonym(ByVal signal As SignalType, ByVal signalIndex As Integer, ByVal signalCount As Integer) As String
        Get
            ' We cache indexed signal reference strings so they don't need to be generated at each mapping call
            ' This helps with performance since the mappings for each signal occur 30 times per second
            Dim signalRef As String = Nothing
            Dim synonyms As String()

            ' Looks up synonym dictionary based on signal type
            If m_signalSynonyms.TryGetValue(signal, synonyms) Then
                ' Lookup signal reference "synonym" value of given signal index
                If signalIndex > -1 AndAlso signalIndex < synonyms.Length Then
                    If synonyms(signalIndex) Is Nothing Then
                        ' Didn't find signal index, create and cache new signal reference
                        signalRef = SignalReference.ToString(m_acronym, signal, signalIndex + 1)
                        synonyms(signalIndex) = signalRef
                    Else
                        signalRef = synonyms(signalIndex)
                    End If
                End If
            Else
                ' Create a new indexed signal reference array
                synonyms = CreateArray(Of String)(signalCount)

                ' Create and cache new signal reference
                signalRef = SignalReference.ToString(m_acronym, signal, signalIndex + 1)
                synonyms(signalIndex) = signalRef
            End If

            Return signalRef
        End Get
    End Property

    Public Property LastReportTime() As Long
        Get
            Return m_lastReportTime
        End Get
        Set(ByVal value As Long)
            m_lastReportTime = value
        End Set
    End Property

    'Public ReadOnly Property Measurements() As Dictionary(Of String, IMeasurement)
    '    Get
    '        If m_activeMeasurements Is Nothing Then m_activeMeasurements = New Dictionary(Of String, IMeasurement)
    '        Return m_activeMeasurements
    '    End Get
    'End Property

    Public Function CompareTo(ByVal other As PmuInfo) As Integer Implements System.IComparable(Of PmuInfo).CompareTo

        Return m_id.CompareTo(other.ID)

    End Function

End Class
