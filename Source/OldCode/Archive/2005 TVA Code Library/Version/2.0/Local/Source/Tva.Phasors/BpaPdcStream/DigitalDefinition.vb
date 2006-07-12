'*******************************************************************************************************
'  DigitalDefinition.vb - PDCstream Digital definition
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

Namespace BpaPdcStream

    <CLSCompliant(False)> _
    Public Class DigitalDefinition

        Inherits DigitalDefinitionBase

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal label As String)

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
                Throw New NotImplementedException("PDCstream does not include digital definition in descriptor packet - must be defined in external INI file")
            End Get
        End Property

    End Class

End Namespace
