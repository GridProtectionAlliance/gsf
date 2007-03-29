'*******************************************************************************************************
'  DigitalDefinitionBase.vb - Digital value definition base class
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
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.ComponentModel

Namespace Phasors

    ''' <summary>This class represents the common implementation of the protocol independent definition of a digital value.</summary>
    <CLSCompliant(False), Serializable()> _
    Public MustInherit Class DigitalDefinitionBase

        Inherits ChannelDefinitionBase
        Implements IDigitalDefinition

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Protected Sub New(ByVal parent As IConfigurationCell)

            MyBase.New(parent)

        End Sub

        Protected Sub New(ByVal parent As IConfigurationCell, ByVal index As Int32, ByVal label As String)

            MyBase.New(parent, index, label, 1, 0)

        End Sub

        Protected Sub New(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, binaryImage, startIndex)

        End Sub

        ' Derived classes are expected to expose a Public Sub New(ByVal digitalDefinition As IDigitalDefinition)
        Protected Sub New(ByVal digitalDefinition As IDigitalDefinition)

            MyClass.New(digitalDefinition.Parent, digitalDefinition.Index, digitalDefinition.Label)

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides ReadOnly Property DataFormat() As DataFormat
            Get
                Return TVA.Phasors.DataFormat.FixedInteger
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public NotOverridable Overrides Property Offset() As Single
            Get
                Return MyBase.Offset
            End Get
            Set(ByVal value As Single)
                If value = 0 Then
                    MyBase.Offset = value
                Else
                    Throw New NotImplementedException("Digital values represent bit flags and thus do not support an offset")
                End If
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public NotOverridable Overrides Property ScalingFactor() As Int32
            Get
                Return MyBase.ScalingFactor
            End Get
            Set(ByVal value As Int32)
                If value = 1 Then
                    MyBase.ScalingFactor = value
                Else
                    Throw New NotImplementedException("Digital values represent bit flags and thus are not scaled")
                End If
            End Set
        End Property

    End Class

End Namespace