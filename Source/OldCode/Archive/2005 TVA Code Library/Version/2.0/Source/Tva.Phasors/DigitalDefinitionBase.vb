'*******************************************************************************************************
'  DigitalDefinitionBase.vb - Digital value definition base class
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.ComponentModel

' This class represents the common implementation of the protocol independent definition of a digital value.
<CLSCompliant(False)> _
Public MustInherit Class DigitalDefinitionBase

    Inherits ChannelDefinitionBase
    Implements IDigitalDefinition

    Protected Sub New(ByVal parent As IConfigurationCell)

        MyBase.New(parent)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal index As Integer, ByVal label As String)

        MyBase.New(parent, Phasors.DataFormat.FixedInteger, index, label, 1, 0)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal digitalDefinition As IDigitalDefinition)
    Protected Sub New(ByVal digitalDefinition As IDigitalDefinition)

        MyClass.New(digitalDefinition.Parent, digitalDefinition.Index, digitalDefinition.Label)

    End Sub

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotOverridable Overrides Property DataFormat() As DataFormat
        Get
            Return MyBase.DataFormat
        End Get
        Set(ByVal value As DataFormat)
            If value = DataFormat.FixedInteger Then
                MyBase.DataFormat = value
            Else
                Throw New NotImplementedException("Digital values represent bit flags and thus can only be fixed integers")
            End If
        End Set
    End Property

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotOverridable Overrides Property Offset() As Double
        Get
            Return MyBase.Offset
        End Get
        Set(ByVal value As Double)
            If value = 0 Then
                MyBase.Offset = value
            Else
                Throw New NotImplementedException("Digital values represent bit flags and thus do not support an offset")
            End If
        End Set
    End Property

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotOverridable Overrides Property ScalingFactor() As Integer
        Get
            Return MyBase.ScalingFactor
        End Get
        Set(ByVal value As Integer)
            If value = 1 Then
                MyBase.ScalingFactor = value
            Else
                Throw New NotImplementedException("Digital values represent bit flags and thus are not scaled")
            End If
        End Set
    End Property

End Class

