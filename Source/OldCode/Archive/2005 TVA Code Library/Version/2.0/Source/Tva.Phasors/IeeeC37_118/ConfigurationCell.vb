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
Imports Tva.Interop
Imports Tva.Interop.Bit
Imports Tva.Collections.Common
Imports Tva.Phasors.Common
Imports Tva.Phasors.IeeeC37_118.Common

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Private m_configurationRevision As UInt16
        Private m_formatFlags As FormatFlags

        Public Sub New(ByVal parent As IConfigurationFrame, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency)

            MyBase.New(parent, idCode, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, nominalFrequency)

        End Sub

        Public Sub New(ByVal configurationCell As IConfigurationCell)

            MyBase.New(configurationCell)

        End Sub

        ' This constructor satisfies ChannelCellBase class requirement:
        '   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        Public Sub New(ByVal parent As IConfigurationFrame, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency, ByVal state As IConfigurationFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            ' Define static creation functions for all configuration cell definition types
            MyBase.New(parent, idCode, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, nominalFrequency, New ConfigurationCellParsingState(Nothing, Nothing, Nothing, Nothing), binaryImage, startIndex)

        End Sub

        ' TODO: May want to shadow all parents in final derived classes - also go through code and make sure all MustInherit class properties are overridable
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

        Public Property ConfigurationRevision() As UInt16
            Get
                Return m_configurationRevision
            End Get
            Set(ByVal value As UInt16)
                m_configurationRevision = value
            End Set
        End Property

        Public Property FormatFlags() As FormatFlags
            Get
                Return m_formatFlags
            End Get
            Set(ByVal value As FormatFlags)
                m_formatFlags = value

                ' Propogate new settings to all configuration definitions...
                For Each phasorDefinition As IPhasorDefinition In PhasorDefinitions
                    phasorDefinition.DataFormat = IIf(Of DataFormat)(value And IeeeC37_118.FormatFlags.Phasors > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
                    phasorDefinition.CoordinateFormat = IIf(Of CoordinateFormat)(value And IeeeC37_118.FormatFlags.Coordinates > 0, CoordinateFormat.Polar, CoordinateFormat.Rectangular)
                Next

                FrequencyDefinition.DataFormat = IIf(Of DataFormat)(value And IeeeC37_118.FormatFlags.Frequency > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)

                For Each analogDefinition As IAnalogDefinition In AnalogDefinitions
                    analogDefinition.DataFormat = IIf(Of DataFormat)(value And IeeeC37_118.FormatFlags.Analog > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
                Next
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As Short
            Get
                Return MyBase.HeaderLength + 10
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)
                Dim index As Integer

                CopyImage(MyBase.HeaderImage, buffer, index, MyBase.HeaderLength)
                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index)
                EndianOrder.BigEndian.CopyBytes(m_formatFlags, buffer, index + 2)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(PhasorDefinitions.Count), buffer, index + 4)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(AnalogDefinitions.Count), buffer, index + 6)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(DigitalDefinitions.Count), buffer, index + 8)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Dim parsingState As IConfigurationCellParsingState = DirectCast(state, IConfigurationCellParsingState)

            MyBase.ParseHeaderImage(state, binaryImage, startIndex)
            startIndex += MyBase.HeaderLength

            IDCode = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            m_formatFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)

            With parsingState
                .PhasorCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
                .AnalogCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6)
                .DigitalCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8)
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
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), FooterLength)
                Dim index As Integer

                CopyImage(MyBase.FooterImage, buffer, index, MyBase.FooterLength)

                If Parent.RevisionNumber > RevisionNumber.RevisionD6 Then
                    ' Add configuration revision count for version 7 and beyond
                    EndianOrder.BigEndian.CopyBytes(m_configurationRevision, buffer, index)
                End If

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            'Dim parsingState As IConfigurationCellParsingState = state

            ' Parse PHUNIT
            ' Parse ANUNIT
            ' Parse DIGUNIT

            ' Parse nominal frequency
            MyBase.ParseFooterImage(state, binaryImage, startIndex)

            If Parent.RevisionNumber > RevisionNumber.RevisionD6 Then
                ' Parse out configuration revision count for version 7 and beyond
                startIndex += 2
                m_configurationRevision = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex)
            End If

        End Sub

    End Class

End Namespace