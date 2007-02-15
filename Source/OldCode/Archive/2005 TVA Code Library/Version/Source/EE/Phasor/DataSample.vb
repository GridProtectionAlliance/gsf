'*******************************************************************************************************
'  DataSample.vb - Phasor data sample class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  This class represents the protocol independent common implementation of a
'  full one second sample of phasor data frames that can be sent or received
'  from a PMU.
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports TVA.Shared.DateTime

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of a full 1 second sample of phasor data frames that can be sent or received from a PMU.
    Public Class DataSample

        Implements IComparable

        Private m_dataFrames As DataFrameCollection
        Private m_sampleRate As Int16
        Private m_timestamp As DateTime
        Private m_published As Boolean

        Public Sub New()

            ' It's not likely that the maximum number of samples per second will ever exceed 240,
            ' but we can't anticipate all uses of these protocols so we set no fixed maximum
            m_dataFrames = New DataFrameCollection(Int32.MaxValue)

        End Sub

        Public Sub New(ByVal sampleRate As Int16, ByVal timestamp As DateTime)

            Me.New()
            m_sampleRate = sampleRate
            m_timestamp = BaselinedTimestamp(timestamp)

        End Sub

        Public Overridable ReadOnly Property DataFrames() As DataFrameCollection
            Get
                Return m_dataFrames
            End Get
        End Property

        Public Overridable Property SampleRate() As Int16
            Get
                Return m_sampleRate
            End Get
            Set(ByVal Value As Int16)
                m_sampleRate = Value
            End Set
        End Property

        Public Overridable Property Timestamp() As DateTime
            Get
                Return m_timestamp
            End Get
            Set(ByVal Value As DateTime)
                m_timestamp = BaselinedTimestamp(Value)
            End Set
        End Property

        Public Overridable ReadOnly Property Published() As Boolean
            Get
                If Not m_published Then
                    If m_dataFrames.Count > 0 Then
                        Dim allPublished As Boolean = True

                        ' The sample has been completely processed once all data frames have been published
                        For x As Integer = 0 To m_dataFrames.Count - 1
                            If Not m_dataFrames(x).Published Then
                                allPublished = False
                                Exit For
                            End If
                        Next

                        If allPublished Then m_published = True
                        Return allPublished
                    Else
                        Return False
                    End If
                Else
                    Return True
                End If
            End Get
        End Property

        ' Data samples are sorted in timestamp order
        Public Overridable Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is DataSample Then
                Return m_timestamp.CompareTo(DirectCast(obj, DataSample).Timestamp)
            Else
                Throw New ArgumentException("DataSample can only be compared with other DataSamples...")
            End If

        End Function

    End Class

End Namespace