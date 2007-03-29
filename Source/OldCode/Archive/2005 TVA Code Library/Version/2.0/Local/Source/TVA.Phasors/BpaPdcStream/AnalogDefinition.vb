'*******************************************************************************************************
'  AnalogDefinition.vb - PDCstream Analog definition
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

Namespace Phasors.BpaPdcStream

    <CLSCompliant(False), Serializable()> _
    Public Class AnalogDefinition

        Inherits AnalogDefinitionBase

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single)

            MyBase.New(parent, index, label, 1, 0)

        End Sub

        Public Sub New(ByVal analogDefinition As IAnalogDefinition)

            MyBase.New(analogDefinition)

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Parent() As ConfigurationCell
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Overrides ReadOnly Property MaximumLabelLength() As Int32
            Get
                Return Int32.MaxValue
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyLength() As UInt16
            Get
                Return 0
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Throw New NotImplementedException("BPA PDCstream does not include analog definition in descriptor packet.  Only a count of available values is defined in the data frame.")
            End Get
        End Property

    End Class

End Namespace
