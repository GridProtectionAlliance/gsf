'***********************************************************************
'  DigitalDefinitionBase.vb - Digital value definition base class
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.ComponentModel

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of a digital value.
    Public MustInherit Class DigitalDefinitionBase

        Inherits ChannelDefinitionBase
        Implements IDigitalDefinition

        ' Create digital definition from other digital definition
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in digitalDefinitionType
        ' Dervied class must expose a Public Sub New(ByVal digitalDefinition As IDigitalDefinition)
        Protected Shared Shadows Function CreateFrom(ByVal digitalDefinitionType As Type, ByVal digitalDefinition As IDigitalDefinition) As IDigitalDefinition

            Return CType(Activator.CreateInstance(digitalDefinitionType, New Object() {digitalDefinition}), IDigitalDefinition)

        End Function

        Protected Sub New(ByVal index As Integer, ByVal label As String)

            MyBase.New(index, label, 1, 0)

        End Sub

        Protected Sub New(ByVal digitalDefinition As IDigitalDefinition)

            Me.New(digitalDefinition.Index, digitalDefinition.Label)

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public NotOverridable Overrides Property Offset() As Double
            Get
                Return MyBase.Offset
            End Get
            Set(ByVal Value As Double)
                If Value = 0 Then
                    MyBase.Offset = Value
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
            Set(ByVal Value As Integer)
                If Value = 1 Then
                    MyBase.ScalingFactor = Value
                Else
                    Throw New NotImplementedException("Digital values represent bit flags and thus are not scaled")
                End If
            End Set
        End Property

    End Class

End Namespace
