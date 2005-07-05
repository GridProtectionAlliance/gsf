'*******************************************************************************************************
'  HeaderFile.vb - PC37_118 Header File
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/24/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text

Namespace EE.Phasor.PC37_118

    ' This class represents a header file that can be sent from a PMU.
    Public Class HeaderFile

        Public Frames As HeaderFrame()

        Protected m_frameList As ArrayList

        Public Const MaximumFrameCount As Int16 = HeaderFrame.MaximumFrameCount

        Public Sub New()

        End Sub

        Public Sub New(ByVal fileName As String)

            Const BufferSize As Integer = 4096
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim fileText As New StringBuilder
            Dim read As Integer

            With File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)
                read = .Read(buffer, 0, BufferSize)
                Do While read > 0
                    fileText.Append(Encoding.Default.GetString(buffer, 0, read))
                    read = .Read(buffer, 0, BufferSize)
                Loop
                .Close()
            End With

            Data = fileText.ToString

        End Sub

        Public Sub AppendNextFrame(ByVal nextFrame As HeaderFrame)

            If m_frameList Is Nothing Then m_frameList = New ArrayList
            m_frameList.Add(nextFrame)

            If nextFrame.IsLastFrame Then
                TotalFrames = m_frameList.Count
                m_frameList.CopyTo(Frames)
                m_frameList = Nothing
            End If

        End Sub

        Public Overridable Property TotalFrames() As Int16
            Get
                Return Frames.Length
            End Get
            Set(ByVal Value As Int16)
                If Value > MaximumFrameCount Then
                    Throw New OverflowException("Total frame count value cannot exceed " & MaximumFrameCount)
                Else
                    Frames = Array.CreateInstance(GetType(HeaderFrame), Value)

                    For x As Integer = 0 To Frames.Length - 1
                        Frames(x) = New HeaderFrame
                        With Frames(x)
                            .IsFirstFrame = (x = 0)
                            .IsLastFrame = (x = Frames.Length - 1)
                            .FrameCount = x
                        End With
                    Next
                End If
            End Set
        End Property

        Public Property Data() As String
            Get
                With New StringBuilder
                    For x As Integer = 0 To Frames.Length - 1
                        .Append(Frames(x).Data)
                    Next

                    Return .ToString
                End With
            End Get
            Set(ByVal Value As String)
                Dim index As Integer

                TotalFrames = Math.Ceiling(Len(Value) / BaseFrame.MaximumDataLength)

                For x As Integer = 0 To Frames.Length - 1
                    If index + BaseFrame.MaximumDataLength > Value.Length Then
                        Frames(x).Data = Value.Substring(index)
                    Else
                        Frames(x).Data = Value.Substring(index, BaseFrame.MaximumDataLength)
                    End If

                    index += Frames(x).DataImage.Length
                Next
            End Set
        End Property

        Public ReadOnly Property BinaryLength() As Integer
            Get
                Dim length As Integer

                For x As Integer = 0 To Frames.Length - 1
                    length += Frames(x).FrameLength
                Next

                Return length
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim index As Integer

                For x As Integer = 0 To Frames.Length - 1
                    With Frames(x)
                        System.Buffer.BlockCopy(.BinaryImage, 0, buffer, index, .FrameLength)
                        index += .FrameLength
                    End With
                Next

                Return buffer
            End Get
        End Property

    End Class

End Namespace