'*******************************************************************************************************
'  FrequencyValue.vb - IEEE 1344 Frequency value
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Interop

Namespace Ieee1344

    <CLSCompliant(False)> _
    Public Class FrequencyValue

        Inherits FrequencyValueBase

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Single, ByVal dfdt As Single)

            MyBase.New(parent, frequencyDefinition, frequency, dfdt)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)

            MyBase.New(parent, frequencyDefinition, unscaledFrequency, unscaledDfDt)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, frequencyDefinition, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal frequencyValue As IFrequencyValue)

            MyBase.New(frequencyValue)

        End Sub

        Friend Shared Function CreateNewFrequencyValue(ByVal parent As IDataCell, ByVal definition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IFrequencyValue

            Return New FrequencyValue(parent, definition, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

    End Class

End Namespace