'*******************************************************************************************************
'  PhasorDefinition.vb - IEEE C37.118 Phasor definition
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

Namespace EE.Phasor.IEEEC37_118

    Public Class PhasorDefinition

        Inherits PhasorDefinitionBase

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent)

            Label = Encoding.ASCII.GetString(binaryImage, startIndex, MaximumLabelLength)

        End Sub

        Public Sub New(ByVal phasorDefinition As IPhasorDefinition)

            MyBase.New(phasorDefinition)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

    End Class

End Namespace
