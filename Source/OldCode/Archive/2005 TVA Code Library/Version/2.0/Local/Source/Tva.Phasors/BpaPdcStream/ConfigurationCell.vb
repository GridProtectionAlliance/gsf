'*******************************************************************************************************
'  ConfigurationCell.vb - PDCstream PMU configuration cell
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

Imports System.Text
Imports Tva.Phasors.Common
Imports Tva.Phasors.BpaPdcStream.Common

Namespace BpaPdcStream

    <CLSCompliant(False)> _
    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Private m_offset As UInt16
        Private m_reserved As Int16

        Public Sub New(ByVal parent As IConfigurationFrame, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency)

            MyBase.New(parent, True, idCode, nominalFrequency, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

        End Sub

        Public Sub New(ByVal configurationCell As IConfigurationCell)

            MyBase.New(configurationCell)

        End Sub

        ' This constructor satisfies ChannelCellBase class requirement:
        '   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
        Public Sub New(ByVal parent As IConfigurationFrame, ByVal state As IConfigurationFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            ' We don't pass in a ConfigurationCellParsingState here because it is not needed for PDCstream (see ParseBodyImage below)
            MyBase.New(parent, True, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, Nothing, binaryImage, startIndex)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Overrides ReadOnly Property MaximumStationNameLength() As Int32
            Get
                ' The station name in the PDCstream is read from an INI file, so there is no set limit
                Return Int32.MaxValue
            End Get
        End Property

        ' The PDCstream descriptor maintains offsets for cell data in data packet
        Public Property Offset() As UInt16
            Get
                Return m_offset
            End Get
            Set(ByVal value As UInt16)
                m_offset = value
            End Set
        End Property

        Public Property Reserved() As Int16
            Get
                Return m_reserved
            End Get
            Set(ByVal value As Int16)
                m_reserved = value
            End Set
        End Property

        ' The descriptor cell broadcasted by PDCstream only includes PMUID and offset, all
        ' other metadata is defined in an external INI based configuration file - so we
        ' override the base class image implementations which attempt to generate and
        ' parse data based on a common nature of configuration frames
        Protected Overrides ReadOnly Property BodyLength() As UInt16
            Get
                Return 8
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)
                Dim index As Int32

                CopyImage(IDLabelImage, buffer, index, IDLabelLength)       ' PMUID
                EndianOrder.BigEndian.CopyBytes(Reserved, buffer, index)    ' Reserved
                EndianOrder.BigEndian.CopyBytes(Offset, buffer, index + 2)  ' Offset

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            IDLabel = Encoding.ASCII.GetString(binaryImage, startIndex, 4)
            Reserved = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
            Offset = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6)

        End Sub

    End Class

End Namespace