'***********************************************************************
'  ChannelFrameBase.vb - Data frame base class
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

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of any frame that can be sent or received from a PMU.
    Public MustInherit Class ChannelFrameBase

        Implements IChannelFrame

        Private m_timeTag As NtpTimeTag

        ' Create channel frame from other channel frame
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in channelFrameType
        ' Dervied class must expose a Public Sub New(ByVal channelFrame As IChannelFrame)
        Protected Shared Function CreateFrom(ByVal channelFrameType As Type, ByVal channelFrame As IChannelFrame) As IChannelFrame

            Return CType(Activator.CreateInstance(channelFrameType, New Object() {channelFrame}), IChannelFrame)

        End Function

        Protected Sub New()

            m_timeTag = New NtpTimeTag(DateTime.Now)

        End Sub

        Protected Sub New(ByVal timeTag As NtpTimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal dataImage As Byte())

            With Me
                .TimeTag = timeTag
                .Milliseconds = milliseconds
                .SynchronizationIsValid = synchronizationIsValid
                .DataIsValid = dataIsValid
                .DataImage = dataImage
            End With

        End Sub

        Protected Sub New(ByVal channelFrame As IChannelFrame)

            Me.New(channelFrame.TimeTag, channelFrame.Milliseconds, channelFrame.SynchronizationIsValid, _
                channelFrame.DataIsValid, channelFrame.DataImage)

        End Sub

        Public MustOverride ReadOnly Property InheritedType() As System.Type Implements IChannelFrame.InheritedType

        Public Overridable ReadOnly Property This() As IChannelFrame Implements IChannelFrame.This
            Get
                Return Me
            End Get
        End Property

        Public Overridable Property TimeTag() As NtpTimeTag Implements IChannelFrame.TimeTag
            Get
                Return m_timeTag
            End Get
            Set(ByVal Value As NtpTimeTag)
                m_timeTag = Value
            End Set
        End Property

        Public MustOverride Property Milliseconds() As Double Implements IChannelFrame.Milliseconds

        Public Overridable ReadOnly Property Timestamp() As DateTime Implements IChannelFrame.Timestamp
            Get
                Return TimeTag.ToDateTime.AddMilliseconds(Milliseconds)
            End Get
        End Property

        Public MustOverride Property SynchronizationIsValid() As Boolean Implements IChannelFrame.SynchronizationIsValid

        Public MustOverride Property DataIsValid() As Boolean Implements IChannelFrame.DataIsValid

        Public Overridable ReadOnly Property Name() As String Implements IChannelFrame.Name
            Get
                Return "TVA.EE.Phasor.ChannelFrameBase"
            End Get
        End Property

        Public Overridable ReadOnly Property DataLength() As Int16 Implements IChannelFrame.DataLength
            Get
                If DataImage Is Nothing Then
                    Return 0
                Else
                    Return DataImage.Length
                End If
            End Get
        End Property

        Public MustOverride Property DataImage() As Byte() Implements IChannelFrame.DataImage

        Public MustOverride ReadOnly Property BinaryLength() As Int16 Implements IChannelFrame.BinaryLength

        Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IChannelFrame.BinaryImage

        Protected Overridable Function ChecksumIsValid(ByVal buffer As Byte(), ByVal startIndex As Integer) As Boolean

            Return EndianOrder.ReverseToInt16(buffer, startIndex + DataLength - 2) = CalculateChecksum(buffer, startIndex, DataLength - 2)

        End Function

        Protected Overridable Sub AppendChecksum(ByVal buffer As Byte(), ByVal startIndex As Integer)

            EndianOrder.SwapCopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex)

        End Sub

        Protected Overridable Function CalculateChecksum(ByVal buffer As Byte(), ByVal offset As Integer, ByVal length As Integer) As Int16

            Return CRC_CCITT(-1, buffer, offset, length)

        End Function

    End Class

End Namespace