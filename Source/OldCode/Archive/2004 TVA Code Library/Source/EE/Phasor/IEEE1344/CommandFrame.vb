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

        Private m_pmuIDCode As Int64
        Private m_command As PMUCommand

        Public Sub New(ByVal pmuIDCode As Int64, ByVal command As PMUCommand)

            m_pmuIDCode = pmuIDCode
            m_command = command

        End Sub

        Public ReadOnly Property PMUIDCode() As Int64
            Get
                Return m_pmuIDCode
            End Get
        End Property

        Public ReadOnly Property Command() As PMUCommand
            Get
                Return m_command
            End Get
        End Property

        Private ReadOnly Property CommandWord() As Int16
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
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim timeTag As New NtpTimeTag(DateTime.Now)

                EndianOrder.SwapCopy(BitConverter.GetBytes(Convert.ToUInt32(timeTag.Value)), 0, buffer, 0, 4)
                EndianOrder.SwapCopy(BitConverter.GetBytes(m_pmuIDCode), 0, buffer, 4, 8)
                EndianOrder.SwapCopy(BitConverter.GetBytes(CommandWord), 0, buffer, 12, 2)
                EndianOrder.SwapCopy(BitConverter.GetBytes(CRC16(-1, buffer, 0, 14)), 0, buffer, 14, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace