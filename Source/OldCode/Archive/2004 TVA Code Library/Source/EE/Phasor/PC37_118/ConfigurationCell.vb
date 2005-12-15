'*******************************************************************************************************
'  ConfigurationCell.vb - IEEE C37.118 PMU configuration cell
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports TVA.Interop
Imports TVA.EE.Phasor.Common
Imports TVA.EE.Phasor.IEEEC37_118.Common
Imports TVA.Shared.Bit

Namespace EE.Phasor.IEEEC37_118

    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Public ConfigurationCount As Int16

        Public Sub New(ByVal parent As IConfigurationFrame)

            MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

        End Sub

        Public Sub New(ByVal configurationCell As IConfigurationCell)

            MyBase.New(configurationCell)

        End Sub

        ' This constructor satisfies ChannelCellBase class requirement:
        '   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        Public Sub New(ByVal parent As IConfigurationFrame, ByVal state As IConfigurationFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, New ConfigurationCellParsingState(GetType(PhasorDefinition), GetType(FrequencyDefinition), GetType(AnalogDefinition), GetType(DigitalDefinition)), binaryImage, startIndex)

        End Sub

        ' TODO: May want to shadow all parents in final derived classes...
        Public Shadows ReadOnly Property Parent() As ConfigurationFrame
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Overrides ReadOnly Property IDLabelLength() As Integer
            Get
                Return 16
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As Short
            Get
                Return 26
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get

            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            Dim parsingState As IConfigurationCellParsingState = DirectCast(state, IConfigurationCellParsingState)
            IDLabel = Encoding.ASCII.GetString(binaryImage, startIndex, 16)
            IDCode = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 16)

            With parsingState
                .PhasorCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 18)
                .AnalogCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 20)
                .DigitalCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 22)
            End With

        End Sub

        Protected Overrides ReadOnly Property FooterLength() As Short
            Get
                If Parent.RevisionNumber = RevisionNumber.RevisionD6 Then
                    Return MyBase.FooterLength
                Else
                    Return 2 + MyBase.FooterLength
                End If
            End Get
        End Property

        Protected Overrides ReadOnly Property FooterImage() As Byte()
            Get
                ' TODO: Add cfg count increment for version 7...
            End Get
        End Property

        Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            MyBase.ParseFooterImage(state, binaryImage, startIndex)

            If Parent.RevisionNumber = RevisionNumber.RevisionV1 Then
                startIndex += MyBase.FooterLength
                ' Parse out cfg count increment for version 7...
                ConfigurationCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            End If

        End Sub

    End Class

End Namespace