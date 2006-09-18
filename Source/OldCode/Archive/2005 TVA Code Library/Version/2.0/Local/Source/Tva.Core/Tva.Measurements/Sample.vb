'*******************************************************************************************************
'  Tva.Measurements.Sample.vb - Measurement sample class (frame collection over one second)
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated for Super Phasor Data Concentrator
'  02/23/2006 - J. Ritchie Carroll
'       Classes abstracted for general use and added to TVA code library
'
'*******************************************************************************************************

Imports Tva.Common

Namespace Measurements

    ''' <summary>This class represents a collection of frames over one second (i.e., a sample of data)</summary>
    Public Class Sample

        Implements IComparable

        Private m_frames As IFrame()            ' Array of frames
        Private m_ticks As Long                 ' Ticks at the beginning of sample

        Friend Sub New(ByVal parent As Concentrator, ByVal ticks As Long)

            m_ticks = ticks

            ' Create new array of frames for this sample...
            m_frames = CreateArray(Of IFrame)(parent.FramesPerSecond)

            For x As Integer = 0 To m_frames.Length - 1
                ' We precalculate frame ticks sitting in the middle of the frame
                m_frames(x) = parent.CreateNewFrameFunction.Invoke(ticks + ((x + 0.5@) * parent.FrameRate))
            Next

        End Sub

        ''' <summary>Handy instance reference to self</summary>
        Public ReadOnly Property This() As Sample
            Get
                Return Me
            End Get
        End Property

        ''' <summary>Array of frames in this sample</summary>
        Public ReadOnly Property Frames() As IFrame()
            Get
                Return m_frames
            End Get
        End Property

        ''' <summary>Gets published state of this sample (i.e., all frames published)</summary>
        Public ReadOnly Property Published() As Boolean
            Get
                Dim allPublished As Boolean = True

                ' The sample has been completely processed once all frames have been published
                For x As Integer = 0 To m_frames.Length - 1
                    If Not m_frames(x).Published Then
                        allPublished = False
                        Exit For
                    End If
                Next

                Return allPublished
            End Get
        End Property

        ''' <summary>Exact timestamp of the beginning of data sample</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Public Property Ticks() As Long
            Get
                Return m_ticks
            End Get
            Set(ByVal value As Long)
                m_ticks = value
            End Set
        End Property

        ''' <summary>Date representation of ticks of data sample</summary>
        Public ReadOnly Property Timestamp() As Date
            Get
                Return New Date(m_ticks)
            End Get
        End Property

        ''' <summary>Samples are sorted by timestamp</summary>
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is Sample Then
                Return m_ticks.CompareTo(DirectCast(obj, Sample).Ticks)
            Else
                Throw New ArgumentException("Sample can only be compared with other Samples...")
            End If

        End Function

    End Class

End Namespace
