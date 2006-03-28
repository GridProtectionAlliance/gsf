'*******************************************************************************************************
'  CommandFrame.vb - IEEE1344 Command Frame
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

Imports Tva.Interop
Imports Tva.Interop.Bit
Imports Tva.DateTime
Imports Tva.IO.Compression.Common

Namespace Ieee1344

    ' This class represents a command frame that can be sent to a PMU to elicit a desired response.  ABB PMU's won't begin
    ' a data broadcast until a command has been sent to "turn on" the real-time stream.
    ' Note: Command frame in IEEE 1344 does not share "common frame header" with other frames
    Public Class CommandFrame

        Public Const FrameLength As Int32 = 18

        Private m_timetag As UnixTimeTag
        Private m_pmuIDCode As Int64
        Private m_command As Command

        ' Use this contructor to send a command to a PMU
        Public Sub New(ByVal pmuIDCode As Int64, ByVal command As Command)

            m_pmuIDCode = pmuIDCode
            m_command = command
            m_timetag = New UnixTimeTag(Date.UtcNow)

        End Sub

        ' Use this constuctor to receive a command (i.e., your code is acting as a PMU)
        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create command frame")
            ElseIf binaryImage.Length - startIndex < FrameLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create command frame")
            Else
                m_timetag = New UnixTimeTag(Convert.ToDouble(EndianOrder.BigEndian.ToInt32(binaryImage, startIndex)))
                m_pmuIDCode = EndianOrder.BigEndian.ToInt64(binaryImage, startIndex + 4)
                m_command = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 12)

                ' Validate buffer check sum
                If EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + FrameLength - 2) <> CRC16(UInt16.MaxValue, binaryImage, startIndex, FrameLength - 2) Then _
                    Throw New ArgumentException("Invalid buffer image detected - CRC16 of command frame did not match")
            End If

        End Sub

        Public Property TimeTag() As UnixTimeTag
            Get
                Return m_timetag
            End Get
            Set(ByVal Value As UnixTimeTag)
                m_timetag = Value
            End Set
        End Property

        Public Property PMUIDCode() As Int64
            Get
                Return m_pmuIDCode
            End Get
            Set(ByVal Value As Int64)
                m_pmuIDCode = Value
            End Set
        End Property

        Public Property Command() As Command
            Get
                Return m_command
            End Get
            Set(ByVal Value As Command)
                m_command = Value
            End Set
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), FrameLength)

                EndianOrder.BigEndian.Copy(BitConverter.GetBytes(Convert.ToUInt32(m_timetag.Value)), 0, buffer, 0, 4)
                EndianOrder.BigEndian.Copy(BitConverter.GetBytes(m_pmuIDCode), 0, buffer, 4, 8)
                EndianOrder.BigEndian.Copy(BitConverter.GetBytes(m_command), 0, buffer, 12, 2)
                EndianOrder.BigEndian.Copy(BitConverter.GetBytes(CRC16(UInt16.MaxValue, buffer, 0, 14)), 0, buffer, 14, 2)

                Return buffer

                'Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), FrameLength)
                ''Const flags As Byte = Bit0 Or Bit6

                '' TODO: MODIFIED FOR PC37.118 TEST - RESTORE FOR REAL IEEE1344 CODE...
                'buffer(0) = &HAA
                'buffer(1) = Bit0 Or Bit6
                'EndianOrder.BigEndian.CopyBytes(Convert.ToUInt16(FrameLength), buffer, 2)
                'EndianOrder.BigEndian.CopyBytes(Convert.ToUInt16(1), buffer, 4)
                'EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(m_timetag.Value), buffer, 6)
                ''EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(0), buffer, 10)
                'System.Buffer.BlockCopy(New Byte() {&HF, &HB, &HBF, &HD0}, 0, buffer, 10, 4)
                'EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_command), buffer, 14)
                'EndianOrder.BigEndian.CopyBytes(CRC_CCITT(-1, buffer, 0, 16), buffer, 16)

                'Return buffer
            End Get
        End Property

        Public ReadOnly Property BinaryLength() As Int32
            Get
                Return FrameLength
            End Get
        End Property

    End Class

End Namespace