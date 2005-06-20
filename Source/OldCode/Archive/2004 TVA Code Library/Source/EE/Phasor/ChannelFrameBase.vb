'***********************************************************************
'  ChannelFrameBase.vb - Channel data frame base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Imports System.Buffer
Imports TVA.Interop
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common
Imports TVA.EE.Phasor.Common

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of any frame of data that can be sent or received from a PMU.
    Public MustInherit Class ChannelFrameBase

        Inherits ChannelBase
        Implements IChannelFrame

        Private m_cells As ChannelCellCollection
        Private m_timeTag As Unix.TimeTag
        Private m_milliseconds As Double
        Private m_synchronizationIsValid As Boolean
        Private m_dataIsValid As Boolean
        Private m_published As Boolean

        Protected Sub New(ByVal cells As ChannelCellCollection)

            MyBase.New()

            m_cells = cells
            m_timeTag = New Unix.TimeTag(DateTime.Now)
            m_synchronizationIsValid = True
            m_dataIsValid = True

        End Sub

        Protected Sub New(ByVal cells As ChannelCellCollection, ByVal timeTag As Unix.TimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean)

            MyBase.New()

            m_cells = cells
            m_timeTag = timeTag
            m_milliseconds = milliseconds
            m_synchronizationIsValid = synchronizationIsValid
            m_dataIsValid = dataIsValid

        End Sub

        ' Dervied classes are expected to expose a Protected Sub New(ByVal channelFrame As IChannelFrame)
        Protected Sub New(ByVal channelFrame As IChannelFrame)

            Me.New(channelFrame.Cells, channelFrame.TimeTag, channelFrame.Milliseconds, channelFrame.SynchronizationIsValid, channelFrame.DataIsValid)

        End Sub

        Public Overridable ReadOnly Property Cells() As ChannelCellCollection Implements IChannelFrame.Cells
            Get
                Return m_cells
            End Get
        End Property

        Public Overridable Property TimeTag() As Unix.TimeTag Implements IChannelFrame.TimeTag
            Get
                Return m_timeTag
            End Get
            Set(ByVal Value As Unix.TimeTag)
                m_timeTag = Value
            End Set
        End Property

        Public Overridable Property NtpTimeTag() As NtpTimeTag Implements IChannelFrame.NtpTimeTag
            Get
                Return New NtpTimeTag(m_timeTag.ToDateTime)
            End Get
            Set(ByVal Value As NtpTimeTag)
                m_timeTag = New Unix.TimeTag(Value.ToDateTime)
            End Set
        End Property

        Public Overridable Property Milliseconds() As Double Implements IChannelFrame.Milliseconds
            Get
                Return m_milliseconds
            End Get
            Set(ByVal Value As Double)
                m_milliseconds = Value
            End Set
        End Property

        Public Overridable ReadOnly Property Timestamp() As DateTime Implements IChannelFrame.Timestamp
            Get
                Return TimeTag.ToDateTime.AddMilliseconds(Milliseconds)
            End Get
        End Property

        Public Overridable Property SynchronizationIsValid() As Boolean Implements IChannelFrame.SynchronizationIsValid
            Get
                Return m_synchronizationIsValid
            End Get
            Set(ByVal Value As Boolean)
                m_synchronizationIsValid = Value
            End Set
        End Property

        Public Overridable Property DataIsValid() As Boolean Implements IChannelFrame.DataIsValid
            Get
                Return m_dataIsValid
            End Get
            Set(ByVal Value As Boolean)
                m_dataIsValid = Value
            End Set
        End Property

        Public Overridable Property Published() As Boolean Implements IChannelFrame.Published
            Get
                Return m_published
            End Get
            Set(ByVal Value As Boolean)
                m_published = Value
            End Set
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Int16
            Get
                Return 2 + MyBase.BinaryLength
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim index As Integer

                ' Copy in base image
                CopyImage(MyBase.BinaryImage, buffer, index, MyBase.BinaryLength)

                ' Add check sum
                AppendChecksum(buffer, index)

                Return buffer
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyLength() As Int16
            Get
                Return Cells.BinaryLength
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Return Cells.BinaryImage
            End Get
        End Property

        Protected Overridable Function ChecksumIsValid(ByVal buffer As Byte(), ByVal startIndex As Integer) As Boolean

            Dim length As Int16 = BinaryLength

            ' TODO: Remove this debug code..
            Dim bufferSum As Int16 = EndianOrder.ReverseToInt16(buffer, startIndex + length - 2)
            Dim calculatedSum As Int16 = CalculateChecksum(buffer, startIndex, length - 2)
            Debug.WriteLine("Buffer Sum = " & bufferSum & ", Calculated Sum = " & calculatedSum)
            Return (bufferSum = calculatedSum)
            'Return EndianOrder.ReverseToInt16(buffer, startIndex + length - 2) = CalculateChecksum(buffer, startIndex, length - 2)

        End Function

        Protected Overridable Sub AppendChecksum(ByVal buffer As Byte(), ByVal startIndex As Integer)

            EndianOrder.SwapCopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex)

        End Sub

        Protected Overridable Function CalculateChecksum(ByVal buffer As Byte(), ByVal offset As Integer, ByVal length As Integer) As Int16

            Return CRC_CCITT(-1, buffer, offset, length)

        End Function

    End Class

End Namespace