'***********************************************************************
'  BaseFrame.vb - Basic IEEE1344 Frame
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

Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor.IEEE1344

    ' This class represents the common definition of all the IEEE1344 message frames that can be sent from a PMU.
    Public MustInherit Class BaseFrame

        Protected m_timeTag As NtpTimeTag
        Protected m_sampleCount As Int16
        Protected m_status As Int16

        Protected Const CommonBinaryLength As Integer = 8
        Protected Const FrameTypeMask As Int16 = Bit13 Or Bit14 Or Bit15
        Protected Const TriggerMask As Int16 = Bit11 Or Bit12 Or Bit13
        Protected Const FrameLengthMask As Int16 = Not (TriggerMask Or Bit14 Or Bit15)

        Protected Sub New()

            m_timeTag = New NtpTimeTag(DateTime.Now)

        End Sub

        Protected Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create " & Name)
            ElseIf binaryImage.Length - startIndex <= CommonBinaryLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create " & Name)
            Else
                m_timeTag = New NtpTimeTag(Convert.ToDouble(EndianOrder.ReverseToInt32(binaryImage, startIndex)))
                m_sampleCount = EndianOrder.ReverseToInt16(binaryImage, startIndex + 4)
                m_status = EndianOrder.ReverseToInt16(binaryImage, startIndex + 6)

                ' Validate buffer check sum
                If EndianOrder.ReverseToInt16(binaryImage, startIndex + FrameLength - 2) <> CRC16(-1, binaryImage, startIndex, FrameLength - 2) Then _
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

        Public ReadOnly Property FrameType() As PMUFrameType
            Get
                Return m_sampleCount And FrameTypeMask
            End Get
        End Property

        Protected Sub SetFrameType(ByVal frameType As PMUFrameType)

            m_sampleCount = (m_sampleCount And Not FrameTypeMask) Or frameType

        End Sub

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

        Public Property TriggerStatus() As PMUTriggerStatus
            Get
                Return m_status And TriggerMask
            End Get
            Set(ByVal Value As PMUTriggerStatus)
                m_status = (m_status And Not TriggerMask) Or Value
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

        Public ReadOnly Property DataLength() As Int16
            Get
                ' Data length will be frame length minus common header length minus crc16
                Return FrameLength - CommonBinaryLength - 2
            End Get
        End Property

        Public ReadOnly Property MaximumFrameLength() As Int16
            Get
                Return FrameLengthMask
            End Get
        End Property

        Public ReadOnly Property MaximumDataLength() As Int16
            Get
                Return MaximumFrameLength - CommonBinaryLength - 2
            End Get
        End Property

        Protected Sub AppendCRC16(ByVal dataBuffer As Byte(), ByVal dataStartIndex As Integer, ByVal dataLength As Integer)

            EndianOrder.SwapCopy(BitConverter.GetBytes(CRC16(-1, dataBuffer, dataStartIndex, dataLength)), dataStartIndex, dataBuffer, dataLength, 2)

        End Sub

        Protected Overridable ReadOnly Property Name() As String
            Get
                Return "IEEE1344.BaseFrame"
            End Get
        End Property

        Public MustOverride ReadOnly Property BinaryLength() As Integer
        Public MustOverride ReadOnly Property BinaryImage() As Byte()

        Protected ReadOnly Property CommonBinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), CommonBinaryLength)

                EndianOrder.SwapCopy(BitConverter.GetBytes(Convert.ToUInt32(m_timeTag.Value)), 0, buffer, 0, 4)
                EndianOrder.SwapCopy(BitConverter.GetBytes(m_sampleCount), 0, buffer, 4, 2)
                EndianOrder.SwapCopy(BitConverter.GetBytes(m_status), 0, buffer, 6, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace