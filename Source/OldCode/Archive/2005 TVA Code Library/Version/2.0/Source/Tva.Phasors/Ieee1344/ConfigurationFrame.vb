'*******************************************************************************************************
'  ConfigurationFrame.vb - IEEE1344 Configuration Frame
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
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

Imports System.Text
Imports Tva.Interop
Imports Tva.Interop.Bit
Imports Tva.DateTime

Namespace Ieee1344

    ' This class represents a configuration frame that can be sent from a PMU.
    Public Class ConfigurationFrame

        Inherits BaseFrame

        Protected m_data As Byte()

        Protected Const FrameCountMask As Int16 = Not (FrameTypeMask Or Bit11 Or Bit12)

        Public Const MaximumFrameCount As Int16 = FrameCountMask

        Public Sub New()

            MyBase.New()
            SetFrameType(FrameType.ConfigurationFrame)

        End Sub

        Protected Friend Sub New(ByVal parsedImage As BaseFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyClass.New()

            ' No need to reparse data, so we pickup what's already been parsed...
            Clone(parsedImage)

            ' Get configuration data
            m_data = Array.CreateInstance(GetType(Byte), DataLength)
            Buffer.BlockCopy(binaryImage, startIndex, m_data, 0, m_data.Length)

        End Sub

        Public Overrides Property DataImage() As Byte()
            Get
                Return m_data
            End Get
            Set(ByVal Value As Byte())
                If Value.Length > MaximumDataLength Then
                    Throw New OverflowException("Data length cannot exceed " & MaximumDataLength & " per frame")
                Else
                    m_data = Value
                    DataLength = Value.Length
                End If
            End Set
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
                Return ((m_sampleCount And Bit11) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_sampleCount = m_sampleCount And Not Bit11
                Else
                    m_sampleCount = m_sampleCount Or Bit11
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

        Protected Overrides ReadOnly Property Name() As String
            Get
                Return "IEEE1344.ConfigurationFrame"
            End Get
        End Property

    End Class

End Namespace

'Public Property DataImage() As Byte()
'    Get
'        Dim image As Byte() = Array.CreateInstance(GetType(Byte), DataLength)
'        Dim x, index, phasorCount, digitalCount As Int32

'        BlockCopy(Encoding.ASCII.GetBytes(m_stationName.PadRight(MaximumStationNameLength)), 0, image, 0, MaximumStationNameLength)

'        index = MaximumStationNameLength
'        EndianOrder.BigEndian.CopyBytes(m_pmuIDCode, image, index)
'        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_phasorDefinitions.Count), image, index + 8)
'        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_digitalDefinitions.Count), image, index + 10)

'        index = 28
'        phasorCount = m_phasorDefinitions.Count
'        digitalCount = m_digitalDefinitions.Count

'        For x = 0 To phasorCount - 1
'            BlockCopy(m_phasorDefinitions(x).LabelImage, 0, image, index + x * PhasorDefinition.MaximumLabelLength, PhasorDefinition.MaximumLabelLength)
'        Next

'        index += phasorCount * PhasorDefinition.MaximumLabelLength

'        For x = 0 To digitalCount - 1
'            BlockCopy(m_digitalDefinitions(x).LabelImage, 0, image, index + x * DigitalDefinition.MaximumLabelLength, DigitalDefinition.MaximumLabelLength)
'        Next

'        index += digitalCount * DigitalDefinition.MaximumLabelLength

'        For x = 0 To phasorCount - 1
'            BlockCopy(m_phasorDefinitions(x).BinaryImage, 0, image, index + x * PhasorDefinition.BinaryLength, PhasorDefinition.BinaryLength)
'        Next

'        index += phasorCount * PhasorDefinition.BinaryLength

'        For x = 0 To digitalCount - 1
'            BlockCopy(m_digitalDefinitions(x).BinaryImage, 0, image, index + x * DigitalDefinition.BinaryLength, DigitalDefinition.BinaryLength)
'        Next

'        index += digitalCount * DigitalDefinition.BinaryLength

'        EndianOrder.BigEndian.CopyBytes(m_freqFlags, image, index)
'        EndianOrder.BigEndian.CopyBytes(m_period, image, index + 2)

'        Return image
'    End Get
'    Set(ByVal Value As Byte())
'        Dim phasorCount, digitalCount, phasorOffset, digitalOffset, freqOffset As Int16
'        Dim x, index As Int32

'        TotalFrames = System.Math.Ceiling(Value.Length / BaseFrame.MaximumDataLength)

'        For x = 0 To Frames.Length - 1
'            If index + BaseFrame.MaximumDataLength > Value.Length Then
'                Frames(x).DataImage = Array.CreateInstance(GetType(Byte), Value.Length - index)
'            Else
'                Frames(x).DataImage = Array.CreateInstance(GetType(Byte), BaseFrame.MaximumDataLength)
'            End If

'            Buffer.BlockCopy(Value, index, Frames(x).DataImage, 0, Frames(x).DataImage.Length)
'            index += Frames(x).DataImage.Length
'        Next

'        m_stationName = Trim(Encoding.ASCII.GetString(Value, 0, MaximumStationNameLength))

'        index = MaximumStationNameLength
'        m_pmuIDCode = EndianOrder.BigEndian.ToInt64(Value, index)
'        phasorCount = EndianOrder.BigEndian.ToInt16(Value, index + 8)
'        digitalCount = EndianOrder.BigEndian.ToInt16(Value, index + 10)

'        index = 28
'        phasorOffset = index + phasorCount * PhasorDefinition.MaximumLabelLength + digitalCount * DigitalDefinition.MaximumLabelLength
'        digitalOffset = phasorOffset + phasorCount * PhasorDefinition.BinaryLength
'        freqOffset = digitalOffset + digitalCount * DigitalDefinition.BinaryLength

'        ' Load phasors
'        m_phasorDefinitions = New PhasorDefinitions

'        For x = 0 To phasorCount - 1
'            m_phasorDefinitions.Add( _
'                New PhasorDefinition(Encoding.ASCII.GetString(Value, index + x * PhasorDefinition.MaximumLabelLength, _
'                    PhasorDefinition.MaximumLabelLength), Value, phasorOffset + x * PhasorDefinition.BinaryLength))
'        Next

'        index += phasorCount * PhasorDefinition.MaximumLabelLength

'        ' Load digitals
'        m_digitalDefinitions = New DigitalDefinitions

'        For x = 0 To digitalCount - 1
'            m_digitalDefinitions.Add( _
'                New DigitalDefinition(Encoding.ASCII.GetString(Value, index + x * DigitalDefinition.MaximumLabelLength, _
'                    DigitalDefinition.MaximumLabelLength), Value, digitalOffset + x * DigitalDefinition.BinaryLength))
'        Next

'        m_freqFlags = EndianOrder.BigEndian.ToInt16(Value, freqOffset)
'        m_period = EndianOrder.BigEndian.ToInt16(Value, freqOffset + 2)
'    End Set
'End Property
