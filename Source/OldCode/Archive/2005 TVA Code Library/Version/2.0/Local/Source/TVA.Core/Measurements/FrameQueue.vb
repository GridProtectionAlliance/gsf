'*******************************************************************************************************
'  TVA.Measurements.FrameQueue.vb - Implementation of a queue of IFrame's
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
'  11/01/2007 - J. Ritchie Carroll
'       Initial version of source generated
'  11/08/2007 - J. Ritchie Carroll
'       Optimized "Pop" call to be a no-wait operation
'
'*******************************************************************************************************

Imports System.Threading
Imports TVA.DateTime
Imports TVA.DateTime.Common

Namespace Measurements

    Public Delegate Function CreateNewFrameFunctionSignature(ByVal ticks As Long) As IFrame

    Public Class FrameQueue

        Private m_frames As List(Of IFrame)
        Private m_head As IFrame
        Private m_tail As IFrame
        Private m_ticksPerFrame As Decimal
        Private m_createNewFrameFunction As CreateNewFrameFunctionSignature

        Public Sub New(ByVal ticksPerFrame As Decimal, ByVal createNewFrameFunction As CreateNewFrameFunctionSignature)

            m_frames = New List(Of IFrame)
            m_ticksPerFrame = ticksPerFrame
            m_createNewFrameFunction = createNewFrameFunction

        End Sub

        Public Property TicksPerFrame() As Decimal
            Get
                Return m_ticksPerFrame
            End Get
            Set(ByVal value As Decimal)
                m_ticksPerFrame = value
            End Set
        End Property

        Public ReadOnly Property CreateNewFrameFunction() As CreateNewFrameFunctionSignature
            Get
                Return m_createNewFrameFunction
            End Get
        End Property

        Public Sub Pop()

            ' Frame's already been handled so there's no rush in removing it, so
            ' we just remove it a little later making this a no-wait operation
            m_head = Nothing
            ThreadPool.QueueUserWorkItem(AddressOf Pop)

        End Sub

        Private Sub Pop(ByVal state As Object)

            ' We didn't try for an immediate lock to remove top frame from original
            ' "Pop" call so now we're running on an independent thread and we'll hang
            ' around until we can get that work done...
            Do While True
                ' Try for lock - we'll wait a moment to see if we can get it...
                If Monitor.TryEnter(m_frames, 1) Then
                    Try
                        ' Now we have a lock, so remove frame
                        m_frames.RemoveAt(0)

                        If m_frames.Count > 0 Then
                            m_head = m_frames(0)
                        Else
                            m_head = Nothing
                            m_tail = Nothing
                        End If

                        Exit Do
                    Finally
                        Monitor.Exit(m_frames)
                    End Try
                Else
                    ' Snooze for a bit and try again...
                    Thread.Sleep(1)
                End If
            Loop

        End Sub

        Public ReadOnly Property Head() As IFrame
            Get
                ' We track the head separately to avoid sync-lock on collection
                ' to access item zero...
                Return m_head
            End Get
        End Property

        Public ReadOnly Property Tail() As IFrame
            Get
                Return m_tail
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return m_frames.Count
            End Get
        End Property

        Public Function GetFrame(ByVal ticks As Long) As IFrame

            Dim frame As IFrame
            Dim destinationTicks As Long = CLng(ticks / m_ticksPerFrame) * m_ticksPerFrame
            Dim frameIndex As Integer

            ' Wait for queue lock
            SyncLock m_frames
                frameIndex = m_frames.BinarySearch(New Frame(destinationTicks))

                If frameIndex < 0 Then
                    ' Didn't find frame for this timestamp so we create one
                    frame = m_createNewFrameFunction(destinationTicks)

                    m_frames.Add(frame)

                    If m_tail Is Nothing OrElse frame.CompareTo(m_tail) > 0 Then
                        m_tail = frame
                    Else
                        m_frames.Sort()
                    End If

                    If m_head Is Nothing AndAlso m_frames.Count = 1 Then m_head = m_tail
                Else
                    ' Found desired frame
                    frame = m_frames(frameIndex)
                End If
            End SyncLock

            Return frame

        End Function

    End Class

End Namespace