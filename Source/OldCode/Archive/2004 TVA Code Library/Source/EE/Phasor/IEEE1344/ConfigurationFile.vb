'*******************************************************************************************************
'  ConfigurationFile.vb - IEEE1344 Configuration File
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

Imports System.Buffer
Imports System.IO
Imports System.Text
Imports TVA.Interop
Imports TVA.Shared.Bit

Namespace EE.Phasor.IEEE1344

    ' This class represents a header file that can be sent from a PMU.
    Public Class ConfigurationFile
    
        Public Frames As ConfigurationFrame()

        Protected m_stationName As String
        Protected m_pmuIDCode As Int64
        Protected m_phasorDefinitions As PhasorDefinitions
        Protected m_digitalDefinitions As DigitalDefinitions
        Protected m_freqFlags As Int16
        Protected m_period As Int16
        Protected m_frameList As ArrayList

        Public Const MaximumFrameCount As Int16 = ConfigurationFrame.MaximumFrameCount
        Public Const MaximumStationNameLength As Integer = 16

        Public Sub New()

            m_phasorDefinitions = New PhasorDefinitions
            m_digitalDefinitions = New DigitalDefinitions

        End Sub

        Public Sub New(ByVal fileName As String)

            Me.New()

            Const BufferSize As Integer = 4096
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim fileData As New MemoryStream
            Dim read As Integer

            With File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)
                read = .Read(buffer, 0, BufferSize)
                Do While read > 0
                    fileData.Write(buffer, 0, read)
                    read = .Read(buffer, 0, BufferSize)
                Loop
                .Close()
            End With

            DataImage = fileData.ToArray

        End Sub

        Public Sub AppendNextFrame(ByVal nextFrame As ConfigurationFrame)

            If m_frameList Is Nothing Then m_frameList = New ArrayList
            m_frameList.Add(nextFrame)

            If nextFrame.IsLastFrame Then
                Dim frame As ConfigurationFrame

                With New MemoryStream
                    For x As Integer = 0 To m_frameList.Count - 1
                        frame = DirectCast(m_frameList(x), ConfigurationFrame)
                        .Write(frame.DataImage, 0, frame.DataImage.Length)
                    Next

                    DataImage = .ToArray
                End With

                m_frameList = Nothing
            End If

        End Sub

        Public Sub RefreshFrames()

            DataImage = DataImage

        End Sub

        Public Property StationName() As String
            Get
                Return m_stationName
            End Get
            Set(ByVal Value As String)
                If Len(Value) > MaximumStationNameLength Then
                    Throw New OverflowException("Station name length cannot exceed " & MaximumStationNameLength)
                Else
                    m_stationName = Value
                End If
            End Set
        End Property

        Public ReadOnly Property PhasorDefinitions() As PhasorDefinitions
            Get
                Return m_phasorDefinitions
            End Get
        End Property

        Public ReadOnly Property DigitalDefinitions() As DigitalDefinitions
            Get
                Return m_digitalDefinitions
            End Get
        End Property

        Public Property FrequencyIsIncluded() As Boolean
            Get
                Return ((m_freqFlags And Bit8) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_freqFlags = m_freqFlags And Not Bit8
                Else
                    m_freqFlags = m_freqFlags Or Bit8
                End If
            End Set
        End Property

        Public Property DfDtIsIncluded() As Boolean
            Get
                Return ((m_freqFlags And Bit9) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_freqFlags = m_freqFlags And Not Bit9
                Else
                    m_freqFlags = m_freqFlags Or Bit9
                End If
            End Set
        End Property

        Public Property LineFrequency() As PMULineFrequency
            Get
                If (m_freqFlags And Bit0) > 0 Then
                    Return PMULineFrequency._50Hz
                Else
                    Return PMULineFrequency._60Hz
                End If
            End Get
            Set(ByVal Value As PMULineFrequency)
                If Value = PMULineFrequency._50Hz Then
                    m_freqFlags = m_freqFlags Or Bit0
                Else
                    m_freqFlags = m_freqFlags And Not Bit0
                End If
            End Set
        End Property

        Public Property Period() As Int16
            Get
                Return m_period
            End Get
            Set(ByVal Value As Int16)
                m_period = Value
            End Set
        End Property

        Public Overridable Property TotalFrames() As Int16
            Get
                Return Frames.Length
            End Get
            Set(ByVal Value As Int16)
                If Value > MaximumFrameCount Then
                    Throw New OverflowException("Total frame count value cannot exceed " & MaximumFrameCount)
                Else
                    Frames = Array.CreateInstance(GetType(ConfigurationFrame), Value)

                    For x As Integer = 0 To Frames.Length - 1
                        Frames(x) = New ConfigurationFrame
                        With Frames(x)
                            .IsFirstFrame = (x = 0)
                            .IsLastFrame = (x = Frames.Length - 1)
                            .FrameCount = x
                        End With
                    Next
                End If
            End Set
        End Property

        Public Property DataImage() As Byte()
            Get
                Dim image As Byte() = Array.CreateInstance(GetType(Byte), DataLength)
                Dim x, index, phasorCount, digitalCount As Integer

                BlockCopy(Encoding.ASCII.GetBytes(m_stationName.PadRight(MaximumStationNameLength)), 0, image, 0, MaximumStationNameLength)

                index = MaximumStationNameLength
                EndianOrder.SwapCopyBytes(m_pmuIDCode, image, index)
                EndianOrder.SwapCopyBytes(Convert.ToInt16(m_phasorDefinitions.Count), image, index + 8)
                EndianOrder.SwapCopyBytes(Convert.ToInt16(m_digitalDefinitions.Count), image, index + 10)

                index = 28
                phasorCount = m_phasorDefinitions.Count
                digitalCount = m_digitalDefinitions.Count

                For x = 0 To phasorCount - 1
                    BlockCopy(m_phasorDefinitions(x).LabelImage, 0, image, index + x * PhasorDefinition.MaximumLabelLength, PhasorDefinition.MaximumLabelLength)
                Next

                index += phasorCount * PhasorDefinition.MaximumLabelLength

                For x = 0 To digitalCount - 1
                    BlockCopy(m_digitalDefinitions(x).LabelImage, 0, image, index + x * DigitalDefinition.MaximumLabelLength, DigitalDefinition.MaximumLabelLength)
                Next

                index += digitalCount * DigitalDefinition.MaximumLabelLength

                For x = 0 To phasorCount - 1
                    BlockCopy(m_phasorDefinitions(x).BinaryImage, 0, image, index + x * PhasorDefinition.BinaryLength, PhasorDefinition.BinaryLength)
                Next

                index += phasorCount * PhasorDefinition.BinaryLength

                For x = 0 To digitalCount - 1
                    BlockCopy(m_digitalDefinitions(x).BinaryImage, 0, image, index + x * DigitalDefinition.BinaryLength, DigitalDefinition.BinaryLength)
                Next

                index += digitalCount * DigitalDefinition.BinaryLength

                EndianOrder.SwapCopyBytes(m_freqFlags, image, index)
                EndianOrder.SwapCopyBytes(m_period, image, index + 2)

                Return image
            End Get
            Set(ByVal Value As Byte())
                Dim phasorCount, digitalCount, phasorOffset, digitalOffset, freqOffset As Int16
                Dim x, index As Integer

                TotalFrames = Math.Ceiling(Value.Length / BaseFrame.MaximumDataLength)

                For x = 0 To Frames.Length - 1
                    If index + BaseFrame.MaximumDataLength > Value.Length Then
                        Frames(x).DataImage = Array.CreateInstance(GetType(Byte), Value.Length - index)
                    Else
                        Frames(x).DataImage = Array.CreateInstance(GetType(Byte), BaseFrame.MaximumDataLength)
                    End If

                    Buffer.BlockCopy(Value, index, Frames(x).DataImage, 0, Frames(x).DataImage.Length)
                    index += Frames(x).DataImage.Length
                Next

                m_stationName = Trim(Encoding.ASCII.GetString(Value, 0, MaximumStationNameLength))

                index = MaximumStationNameLength
                m_pmuIDCode = EndianOrder.ReverseToInt64(Value, index)
                phasorCount = EndianOrder.ReverseToInt16(Value, index + 8)
                digitalCount = EndianOrder.ReverseToInt16(Value, index + 10)

                index = 28
                phasorOffset = index + phasorCount * PhasorDefinition.MaximumLabelLength + digitalCount * DigitalDefinition.MaximumLabelLength
                digitalOffset = phasorOffset + phasorCount * PhasorDefinition.BinaryLength
                freqOffset = digitalOffset + digitalCount * DigitalDefinition.BinaryLength

                ' Load phasors
                m_phasorDefinitions = New PhasorDefinitions

                For x = 0 To phasorCount - 1
                    m_phasorDefinitions.Add( _
                        New PhasorDefinition(Encoding.ASCII.GetString(Value, index + x * PhasorDefinition.MaximumLabelLength, _
                            PhasorDefinition.MaximumLabelLength), Value, phasorOffset + x * PhasorDefinition.BinaryLength))
                Next

                index += phasorCount * PhasorDefinition.MaximumLabelLength

                ' Load digitals
                m_digitalDefinitions = New DigitalDefinitions

                For x = 0 To digitalCount - 1
                    m_digitalDefinitions.Add( _
                        New DigitalDefinition(Encoding.ASCII.GetString(Value, index + x * DigitalDefinition.MaximumLabelLength, _
                            DigitalDefinition.MaximumLabelLength), Value, digitalOffset + x * DigitalDefinition.BinaryLength))
                Next

                m_freqFlags = EndianOrder.ReverseToInt16(Value, freqOffset)
                m_period = EndianOrder.ReverseToInt16(Value, freqOffset + 2)
            End Set
        End Property

        Public ReadOnly Property DataLength() As Integer
            Get
                Dim length As Integer

                For x As Integer = 0 To Frames.Length - 1
                    length += Frames(x).DataLength
                Next

                Return length
            End Get
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
                        BlockCopy(.BinaryImage, 0, buffer, index, .FrameLength)
                        index += .FrameLength
                    End With
                Next

                Return buffer
            End Get
        End Property

    End Class

End Namespace