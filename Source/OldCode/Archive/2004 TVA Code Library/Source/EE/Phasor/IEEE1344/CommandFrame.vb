'***********************************************************************
'  CommandFrame.vb - IEEE1344 Command Frame
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

    ' This class represents a command frame that can be sent to a PMU to elicit a desired response.  ABB PMU's won't begin
    ' a data broadcast until a command has been sent to "turn on" the real-time stream.
    Public Class CommandFrame

        Public Enum PMUCommand
            DisableRealTimeData
            EnableRealTimeData
            SendHeaderFile
            SendConfigFile1
            SendConfigFile2
            ReceiveReferencePhasor
        End Enum

        Public Const BinaryLength As Integer = 16

        Private m_timetag As NtpTimeTag
        Private m_pmuIDCode As Int64
        Private m_command As PMUCommand

        ' Use this contructor to send a command to a PMU
        Public Sub New(ByVal pmuIDCode As Int64, ByVal command As PMUCommand)

            m_pmuIDCode = pmuIDCode
            m_command = command
            m_timetag = New NtpTimeTag(DateTime.Now)

        End Sub

        ' Use this constuctor to receive a command (i.e., your code is acting as a PMU)
        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create command frame")
            ElseIf binaryImage.Length - startIndex < BinaryLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create command frame")
            Else
                m_timetag = New NtpTimeTag(Convert.ToDouble(EndianOrder.ReverseToInt32(binaryImage, startIndex)))
                m_pmuIDCode = EndianOrder.ReverseToInt64(binaryImage, startIndex + 4)
                CommandWord = EndianOrder.ReverseToInt16(binaryImage, startIndex + 12)

                ' Validate buffer check sum
                If EndianOrder.ReverseToInt16(binaryImage, startIndex + 14) <> CRC16(-1, binaryImage, 0, 14) Then _
                    Throw New ArgumentException("Invalid buffer image detected - CRC16 of command frame did not match")
            End If

        End Sub

        Public Property SecondOfCentury() As NtpTimeTag
            Get
                Return m_timetag
            End Get
            Set(ByVal Value As NtpTimeTag)
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

        Public Property Command() As PMUCommand
            Get
                Return m_command
            End Get
            Set(ByVal Value As PMUCommand)
                m_command = Value
            End Set
        End Property

        Private Property CommandWord() As Int16
            Get
                Dim word As Int16

                Select Case m_command
                    Case PMUCommand.DisableRealTimeData
                        word = Bit0
                    Case PMUCommand.EnableRealTimeData
                        word = Bit1
                    Case PMUCommand.SendHeaderFile
                        word = Bit0 Or Bit1
                    Case PMUCommand.SendConfigFile1
                        word = Bit2
                    Case PMUCommand.SendConfigFile2
                        word = Bit0 Or Bit2
                    Case PMUCommand.ReceiveReferencePhasor
                        word = Bit3
                End Select

                Return word
            End Get
            Set(ByVal Value As Int16)
                If (Value And Bit0) > 0 Then
                    m_command = PMUCommand.DisableRealTimeData
                ElseIf (Value And Bit1) > 0 Then
                    m_command = PMUCommand.EnableRealTimeData
                ElseIf (Value And Bit0) > 0 And (Value And Bit1) > 0 Then
                    m_command = PMUCommand.SendHeaderFile
                ElseIf (Value And Bit2) > 0 Then
                    m_command = PMUCommand.SendConfigFile1
                ElseIf (Value And Bit0) > 0 And (Value And Bit2) > 0 Then
                    m_command = PMUCommand.SendConfigFile2
                ElseIf (Value And Bit3) > 0 Then
                    m_command = PMUCommand.ReceiveReferencePhasor
                End If
            End Set
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                EndianOrder.SwapCopy(BitConverter.GetBytes(Convert.ToUInt32(m_timetag.Value)), 0, buffer, 0, 4)
                EndianOrder.SwapCopy(BitConverter.GetBytes(m_pmuIDCode), 0, buffer, 4, 8)
                EndianOrder.SwapCopy(BitConverter.GetBytes(CommandWord), 0, buffer, 12, 2)
                EndianOrder.SwapCopy(BitConverter.GetBytes(CRC16(-1, buffer, 0, 14)), 0, buffer, 14, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace