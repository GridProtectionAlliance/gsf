'***********************************************************************
'  ConfigurationFrame.vb - IEEE1344 Configuration Frame
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Text
Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor.IEEE1344

    ' This class represents a configuration frame that can be sent from a PMU.
    Public Class ConfigurationFrame

        Inherits BaseFrame

        Protected m_stationName As String
        Protected m_pmuIDCode As Int64
        Protected m_phasors As PhasorDefinitions
        Protected m_digitals As DigitalDefinitions
        Protected m_freqFlags As Int16
        Protected m_period As Int16

        Protected Const FrameCountMask As Int16 = Not (FrameTypeMask Or Bit11 Or Bit12)

        Public Sub New()

            MyBase.New()
            SetFrameType(PMUFrameType.ConfigurationFrame)

            m_phasors = New PhasorDefinitions
            m_digitals = New DigitalDefinitions

        End Sub

        Protected Friend Sub New(ByVal parsedImage As BaseFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Me.New()

            ' No need to reparse data, so we pickup what's already been parsed...
            Clone(parsedImage)

            Dim phasorCount, digitalCount, phasorOffset, digitalOffset, freqOffset, x As Int16

            m_stationName = Trim(Encoding.ASCII.GetString(binaryImage, startIndex, 16))
            m_pmuIDCode = EndianOrder.ReverseToInt64(binaryImage, startIndex + 16)
            phasorCount = EndianOrder.ReverseToInt16(binaryImage, startIndex + 24)
            digitalCount = EndianOrder.ReverseToInt16(binaryImage, startIndex + 26)

            startIndex += 28
            phasorOffset = startIndex + (phasorCount + digitalCount) * 16
            digitalOffset = phasorOffset + phasorCount * 4
            freqOffset = digitalOffset + digitalCount * 2

            ' Load phasors
            For x = 0 To phasorCount - 1
                m_phasors.Add(New PhasorDefinition(Encoding.ASCII.GetString(binaryImage, startIndex + x * 16, 16), binaryImage, phasorOffset + x * 4))
            Next

            startIndex += phasorCount * 16

            ' Load digitals
            For x = 0 To digitalCount - 1
                m_digitals.Add(New DigitalDefinition(Encoding.ASCII.GetString(binaryImage, startIndex + x * 16, 16), binaryImage, phasorOffset + x * 2))
            Next

            m_freqFlags = EndianOrder.ReverseToInt16(binaryImage, freqOffset)
            m_period = EndianOrder.ReverseToInt16(binaryImage, freqOffset + 2)

        End Sub

        Public ReadOnly Property Phasors() As PhasorDefinitions
            Get
                Return m_phasors
            End Get
        End Property

        Public ReadOnly Property Digitals() As DigitalDefinitions
            Get
                Return m_digitals
            End Get
        End Property

        Public Overridable Property IsFirstFrame() As Boolean
            Get
                Return ((m_sampleCount And Bit12) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_sampleCount = m_sampleCount And Not Bit12
                Else
                    m_sampleCount = m_sampleCount Or Bit12
                End If
            End Set
        End Property

        Public Overridable Property IsLastFrame() As Boolean
            Get
                Return ((m_sampleCount And Bit11) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_sampleCount = m_sampleCount Or Bit11
                Else
                    m_sampleCount = m_sampleCount And Not Bit11
                End If
            End Set
        End Property

        Public Overridable Property FrameCount() As Int16
            Get
                Return m_sampleCount And FrameCountMask
            End Get
            Set(ByVal Value As Int16)
                If Value > MaximumFrameCount Then
                    Throw New OverflowException("Frame count value cannot exceed " & MaximumFrameCount)
                Else
                    m_sampleCount = (m_sampleCount And Not FrameCountMask) Or Value
                End If
            End Set
        End Property

        Public ReadOnly Property MaximumFrameCount() As Int16
            Get
                Return FrameCountMask
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

        Protected Overrides ReadOnly Property Name() As String
            Get
                Return "IEEE1344.ConfigurationFrame"
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                Dim length As Integer = CommonBinaryLength + 2

                Return length
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                Array.Copy(MyBase.CommonBinaryImage, 0, buffer, 0, CommonBinaryLength)

                'AppendCRC16(buffer, 0, CommonBinaryLength + m_data.Length)

                Return buffer
            End Get
        End Property

    End Class

End Namespace