'*******************************************************************************************************
'  BaseFrame.vb - Basic IEEE1344 Frame
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

Imports System.Buffer
Imports Tva.Interop
Imports Tva.Interop.Bit
Imports Tva.DateTime
Imports Tva.IO.Compression.Common

Namespace Ieee1344

    ' This class represents the common definition of all the IEEE1344 message frames that can be sent from a PMU.
    Public MustInherit Class BaseFrame

        Protected m_timeTag As NtpTimeTag
        Protected m_sampleCount As Int16
        Protected m_status As Int16

        Protected Const CommonBinaryLength As Int32 = 8
        Protected Const FrameTypeMask As Int16 = Bit13 Or Bit14 Or Bit15
        Protected Const TriggerMask As Int16 = Bit11 Or Bit12 Or Bit13
        Protected Const FrameLengthMask As Int16 = Not (TriggerMask Or Bit14 Or Bit15)

        Public Const MaximumFrameLength As Int16 = FrameLengthMask
        Public Const MaximumDataLength As Int16 = MaximumFrameLength - CommonBinaryLength - 2

        Protected Sub New()

            m_timeTag = New NtpTimeTag(Date.UtcNow)

        End Sub

        Protected Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create " & Name)
            ElseIf binaryImage.Length - startIndex <= CommonBinaryLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create " & Name)
            Else
                m_timeTag = New NtpTimeTag(Convert.ToDouble(EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex)))
                m_sampleCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
                m_status = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6)

                ' Validate buffer check sum
                If EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + FrameLength - 2) <> CRC16(UInt16.MaxValue, binaryImage, startIndex, FrameLength - 2) Then _
                    Throw New ArgumentException("Invalid buffer image detected - CRC16 of " & Name & " did not match")
            End If

        End Sub

        Protected Sub Clone(ByVal source As BaseFrame)

            With source
                m_timeTag = .m_timeTag
                m_sampleCount = .m_sampleCount
                m_status = .m_status
            End With

        End Sub

        Public Property TimeTag() As NtpTimeTag
            Get
                Return m_timeTag
            End Get
            Set(ByVal Value As NtpTimeTag)
                m_timeTag = Value
            End Set
        End Property

        Public ReadOnly Property FrameType() As FrameType
            Get
                Return m_sampleCount And FrameTypeMask
            End Get
        End Property

        Protected Sub SetFrameType(ByVal frameType As FrameType)

            m_sampleCount = (m_sampleCount And Not FrameTypeMask) Or frameType

        End Sub

        Public ReadOnly Property This() As BaseFrame
            Get
                Return Me
            End Get
        End Property

        Public Property SynchronizationIsValid() As Boolean
            Get
                Return ((m_status And Bit15) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_status = m_status And Not Bit15
                Else
                    m_status = m_status Or Bit15
                End If
            End Set
        End Property

        Public Property DataIsValid() As Boolean
            Get
                Return ((m_status And Bit14) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_status = m_status And Not Bit14
                Else
                    m_status = m_status Or Bit14
                End If
            End Set
        End Property

        Public Property TriggerStatus() As TriggerStatus
            Get
                Return m_status And TriggerMask
            End Get
            Set(ByVal Value As TriggerStatus)
                m_status = (m_status And Not TriggerMask) Or Value
            End Set
        End Property

        Protected Sub AppendCRC16(ByVal buffer As Byte(), ByVal startIndex As Int32)

            EndianOrder.BigEndian.CopyBytes(CRC16(UInt16.MaxValue, buffer, 0, startIndex), buffer, startIndex)

        End Sub

        Protected Overridable ReadOnly Property Name() As String
            Get
                Return "IEEE1344.BaseFrame"
            End Get
        End Property

        Public Property DataLength() As Int16
            Get
                ' Data length will be frame length minus common header length minus crc16
                Return FrameLength - CommonBinaryLength - 2
            End Get
            Set(ByVal Value As Int16)
                If Value > MaximumDataLength Then
                    Throw New OverflowException("Data length value cannot exceed " & MaximumDataLength)
                Else
                    FrameLength = Value + CommonBinaryLength + 2
                End If
            End Set
        End Property

        Public Property FrameLength() As Int16
            Get
                Return m_status And FrameLengthMask
            End Get
            Set(ByVal Value As Int16)
                If Value > MaximumFrameLength Then
                    Throw New OverflowException("Frame length value cannot exceed " & MaximumFrameLength)
                Else
                    m_status = (m_status And Not FrameLengthMask) Or Value
                End If
            End Set
        End Property

        Public MustOverride Property DataImage() As Byte()

        Public Overridable ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), FrameLength)
                Dim image As Byte() = DataImage()

                EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(m_timeTag.Value), buffer, 0)
                EndianOrder.BigEndian.CopyBytes(m_sampleCount, buffer, 4)
                EndianOrder.BigEndian.CopyBytes(m_status, buffer, 6)
                BlockCopy(image, 0, buffer, 8, image.Length)
                AppendCRC16(buffer, 8 + image.Length)

                Return buffer
            End Get
        End Property

    End Class

End Namespace