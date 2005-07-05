'*******************************************************************************************************
'  ConfigurationCell.vb - PMU configuration cell
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

Namespace EE.Phasor.PDCstream

    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Private m_offset As Int16

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
                Return Integer.MaxValue
            End Get
        End Property

        Public Property Offset() As Int16
            Get
                Return m_offset
            End Get
            Set(ByVal Value As Int16)
                m_offset = Value
            End Set
        End Property

    End Class

End Namespace