'*******************************************************************************************************
'  ConfigurationCell.vb - PDCstream PMU configuration cell
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
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

Imports TVA.Interop
Imports TVA.EE.Phasor.Common

Namespace EE.Phasor.PDCstream

    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Private m_offset As Int16
        Private m_reserved As Int16

        Public Sub New(ByVal parent As IConfigurationFrame)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal configurationCell As IConfigurationCell)

            MyBase.New(configurationCell)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Overrides ReadOnly Property MaximumStationNameLength() As Integer
            Get
                ' The station name in the PDCstream is read from an INI file, so
                ' there is no set limit
                Return Integer.MaxValue
            End Get
        End Property

        ' The PDCstream descriptor maintains offsets for cell data in data packet
        Public Property Offset() As Int16
            Get
                Return m_offset
            End Get
            Set(ByVal Value As Int16)
                m_offset = Value
            End Set
        End Property

        Public Property Reserved() As Int16
            Get
                Return m_reserved
            End Get
            Set(ByVal Value As Int16)
                m_reserved = Value
            End Set
        End Property

        ' The descriptor cell broadcasted by PDCstream only includes PMUID and offset, all
        ' other metadata is maintained in external INI file
        Protected Overrides ReadOnly Property BodyLength() As Short
            Get
                Return 8
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)
                Dim index As Integer

                ' PMUID
                CopyImage(IDLabelImage, buffer, index, IDLabelLength)

                ' Reserved
                EndianOrder.BigEndian.CopyBytes(Reserved, buffer, index)
                index += 2

                ' Offset
                EndianOrder.BigEndian.CopyBytes(Offset, buffer, index)

                Return buffer
            End Get
        End Property

    End Class

End Namespace