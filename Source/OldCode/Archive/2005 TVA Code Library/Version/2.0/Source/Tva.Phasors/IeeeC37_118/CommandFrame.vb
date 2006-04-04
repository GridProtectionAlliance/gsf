'*******************************************************************************************************
'  CommandFrame.vb - IEEE C37.118 command frame
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Common
Imports Tva.Phasors.IeeeC37_118.Common

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class CommandFrame

        Inherits CommandFrameBase
        Implements ICommonFrameHeader

        Private m_revisionNumber As RevisionNumber
        Private m_version As Byte

        Public Sub New(ByVal command As Command)

            MyClass.New(IeeeC37_118.RevisionNumber.RevisionV1, command)

        End Sub

        Public Sub New(ByVal revisionNumber As RevisionNumber, ByVal command As Command)

            MyBase.New(New CommandCellCollection(MaximumExtendedDataLength), command)
            m_revisionNumber = revisionNumber
            m_version = IIf(Of Byte)(m_revisionNumber <= IeeeC37_118.RevisionNumber.RevisionV1, 1, 2)

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New CommandFrameParsingState(New CommandCellCollection(MaximumExtendedDataLength), parsedFrameHeader.FrameLength, _
                parsedFrameHeader.FrameLength - CommonFrameHeader.BinaryLength - 4), binaryImage, startIndex)

            CommonFrameHeader.Clone(parsedFrameHeader, Me)

        End Sub

        Public Sub New(ByVal commandFrame As ICommandFrame)

            MyBase.New(commandFrame)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Property RevisionNumber() As RevisionNumber Implements ICommonFrameHeader.RevisionNumber
            Get
                Return m_revisionNumber
            End Get
            Set(ByVal Value As RevisionNumber)
                m_revisionNumber = Value
            End Set
        End Property

        Public Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
            Get
                Return IeeeC37_118.FrameType.CommandFrame
            End Get
            Private Set(ByVal value As FrameType)
                ' Frame type is readonly for command frames - we don't throw an exception here if someone attempts to change
                ' the frame type on a command frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public Property Version() As Byte Implements ICommonFrameHeader.Version
            Get
                Return m_version
            End Get
            Set(ByVal value As Byte)
                m_version = CommonFrameHeader.Version(Me, value)
            End Set
        End Property

        Public Property FrameLength() As Int16 Implements ICommonFrameHeader.FrameLength
            Get
                Return MyBase.BinaryLength
            End Get
            Set(ByVal value As Int16)
                MyBase.ParsedBinaryLength = value
            End Set
        End Property

        Private ReadOnly Property TimeBase() As Int32 Implements ICommonFrameHeader.TimeBase
            Get
                ' Command frame doesn't need subsecond time resolution - so this factor is just defaulted to max...
                Return Int32.MaxValue And Not TimeQualityFlagsMask
            End Get
        End Property

        Private Property InternalTimeQualityFlags() As Int32 Implements ICommonFrameHeader.InternalTimeQualityFlags
            Get
                Return 0
            End Get
            Set(ByVal value As Int32)
                ' Time quality flags are readonly for command frames - we don't throw an exception here if someone attempts to change
                ' the time quality on a command frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                ' but we don't do anything with the value either.
            End Set
        End Property

        Public ReadOnly Property SecondOfCentury() As UInt32 Implements ICommonFrameHeader.SecondOfCentury
            Get
                Return CommonFrameHeader.SecondOfCentury(Me)
            End Get
        End Property

        Public ReadOnly Property FractionOfSecond() As Int32 Implements ICommonFrameHeader.FractionOfSecond
            Get
                Return CommonFrameHeader.FractionOfSecond(Me)
            End Get
        End Property

        Public Property TimeQualityFlags() As TimeQualityFlags Implements ICommonFrameHeader.TimeQualityFlags
            Get
                Return CommonFrameHeader.TimeQualityFlags(Me)
            End Get
            Friend Set(ByVal value As TimeQualityFlags)
                ' Nothing to do - time quality flags is readonly for command frames
            End Set
        End Property

        Public Property TimeQualityIndicatorCode() As TimeQualityIndicatorCode Implements ICommonFrameHeader.TimeQualityIndicatorCode
            Get
                Return CommonFrameHeader.TimeQualityIndicatorCode(Me)
            End Get
            Friend Set(ByVal value As TimeQualityIndicatorCode)
                ' Nothing to do - time quality flags is readonly for command frames
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                Return CommonFrameHeader.BinaryLength
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Return CommonFrameHeader.BinaryImage(Me)
            End Get
        End Property

    End Class

End Namespace