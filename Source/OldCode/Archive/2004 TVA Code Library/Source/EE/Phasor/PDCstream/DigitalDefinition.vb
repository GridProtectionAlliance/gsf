'***********************************************************************
'  DigitalDefinition.vb - Digital definition
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor.PDCstream

    Public Class DigitalDefinition

        Inherits DigitalDefinitionBase

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Integer, ByVal label As String)

            MyBase.New(parent, index, label)

        End Sub

        Public Sub New(ByVal digitalDefinition As IDigitalDefinition)

            MyBase.New(digitalDefinition)

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

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Throw New NotImplementedException("PDCstream does not include digital definition in descriptor packet - must be defined in external INI file")
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Int16
            Get
                Throw New NotImplementedException("PDCstream does not include digital definition in descriptor packet - must be defined in external INI file")
            End Get
        End Property

    End Class

End Namespace
