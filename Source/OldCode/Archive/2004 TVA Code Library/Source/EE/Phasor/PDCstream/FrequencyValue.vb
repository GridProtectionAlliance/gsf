'***********************************************************************
'  FrequencyValue.vb - Frequency value
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

Imports TVA.Interop

Namespace EE.Phasor.PDCstream

    Public Class FrequencyValue

        Inherits FrequencyValueBase

        'Public Shared ReadOnly Property Empty(ByVal frequencyDefinition As FrequencyDefinition) As FrequencyValue
        '    Get
        '        Return New FrequencyValue(frequencyDefinition, 0, 0)
        '    End Get
        'End Property

        'Public Shared Function CreateFromScaledValues(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As Double, ByVal dfdt As Double) As FrequencyValue

        '    With frequencyDefinition
        '        Return CreateFromUnscaledValues(frequencyDefinition, (frequency - .Offset) * .Scale, (dfdt - .DfDtOffset) * .DfDtScale)
        '    End With

        'End Function

        'Public Shared Function CreateFromUnscaledValues(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As Int16, ByVal dfdt As Int16) As FrequencyValue

        '    Return New FrequencyValue(frequencyDefinition, frequency, dfdt)

        'End Function

        Public Shared Function CalculateBinaryLength(ByVal frequencyDefinition As FrequencyDefinition) As Integer

            Return (New FrequencyValue(frequencyDefinition, 0, 0)).BinaryLength

        End Function

        Public Sub New(ByVal frequencyDefinition As FrequencyDefinition, ByVal frequency As Double, ByVal dfdt As Double)

            'MyBase.New(, frequencyDefinition, frequency, dfdt)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

    End Class

End Namespace