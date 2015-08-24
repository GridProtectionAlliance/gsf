'*******************************************************************************************************
'  CommandFrame.vb - IEEE C37.118 Command Frame
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
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

Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor.IEEEC37_118

    ' TODO: Create a base class for the command frame that will suffice for both C37.118 and 1344...
    ' This class represents a command frame that can be sent to a PMU to elicit a desired response.
    ' Most PMU's won't begin a data broadcast until a command has been sent to "turn on" the real-time stream.
    Public Class CommandFrame

        ' TODO: Extended frames will have variable length
        Public Const FrameLength As Integer = 18

        Private m_timetag As Unix.TimeTag
        Private m_idCode As Int16
        Private m_command As Command

        ' Use this contructor to send a command to a PMU
        Public Sub New(ByVal idCode As Int16, ByVal command As Command)

            m_idCode = idCode
            m_command = command
            m_timetag = New Unix.TimeTag(DateTime.Now)

        End Sub

        ' Use this constuctor to receive a command (i.e., your code is acting as a PMU)
        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            'If binaryImage Is Nothing Then
            '    Throw New ArgumentNullException("BinaryImage was null - could not create command frame")
            'ElseIf binaryImage.Length - startIndex < FrameLength Then
            '    Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create command frame")
            'Else
            '    m_timetag = New Unix.TimeTag(Convert.ToDouble(EndianOrder.BigEndian.ToInt32(binaryImage, startIndex)))
            '    m_idCode = EndianOrder.BigEndian.ToInt64(binaryImage, startIndex + 4)
            '    m_command = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 12)

            '    ' Validate buffer check sum
            '    If EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + FrameLength - 2) <> CRC16(-1, binaryImage, startIndex, FrameLength - 2) Then _
            '        Throw New ArgumentException("Invalid buffer image detected - CRC16 of command frame did not match")
            'End If

        End Sub

        Public Property TimeTag() As Unix.TimeTag
            Get
                Return m_timetag
            End Get
            Set(ByVal Value As Unix.TimeTag)
                m_timetag = Value
            End Set
        End Property

        Public Property IDCode() As Int16
            Get
                Return m_idCode
            End Get
            Set(ByVal Value As Int16)
                m_idCode = Value
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

                ' TODO: MODIFIED FOR PC37.118 TEST - RESTORE LATER...
                buffer(0) = &HAA
                buffer(1) = Bit0 Or FrameType.CommandFrame
                EndianOrder.BigEndian.CopyBytes(Convert.ToUInt16(FrameLength), buffer, 2)
                EndianOrder.BigEndian.CopyBytes(m_idCode, buffer, 4)
                EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(m_timetag.Value), buffer, 6)
                EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(0), buffer, 10)
                'System.Buffer.BlockCopy(New Byte() {&HF, &HB, &HBF, &HD0}, 0, buffer, 10, 4)
                EndianOrder.BigEndian.CopyBytes(m_command, buffer, 14)
                EndianOrder.BigEndian.CopyBytes(CRC_CCITT(-1, buffer, 0, 16), buffer, 16)
                'EndianOrder.BigEndian.CopyBytes(CRC16(-1, buffer, 0, 16), buffer, 16)

                Return buffer
            End Get
        End Property

    End Class

End Namespace