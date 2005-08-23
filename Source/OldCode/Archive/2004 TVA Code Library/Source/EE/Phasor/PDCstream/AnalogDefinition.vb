'*******************************************************************************************************
'  AnalogDefinition.vb - PDCstream Analog definition
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

    Public Class AnalogDefinition

        Inherits AnalogDefinitionBase

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal dataFormat As DataFormat, ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double)

            MyBase.New(parent, dataFormat, index, label, 1, 0)

        End Sub

        Public Sub New(ByVal analogDefinition As IAnalogDefinition)

            MyBase.New(analogDefinition)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Overrides ReadOnly Property MaximumLabelLength() As Integer
            Get
                Return Integer.MaxValue
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyLength() As Int16
            Get
                Return 0
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                ' TODO: Ask Ken Martin about defining Analog Definitions in INI file
                Throw New NotImplementedException("PDCstream does not include analog definition in descriptor packet - must be defined in external INI file")
            End Get
        End Property

    End Class

End Namespace
