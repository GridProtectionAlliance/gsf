'*******************************************************************************************************
'  FrequencyValue.vb - IEEE 1344 Frequency value
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

Namespace FNet

    <CLSCompliant(False), Serializable()> _
    Public Class FrequencyValue

        Inherits FrequencyValueBase

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Single, ByVal dfdt As Single)

            MyBase.New(parent, frequencyDefinition, frequency, dfdt)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)

            MyBase.New(parent, frequencyDefinition, unscaledFrequency, unscaledDfDt)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As FrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, frequencyDefinition, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal frequencyValue As IFrequencyValue)

            MyBase.New(frequencyValue)

        End Sub

        Friend Shared Function CreateNewFrequencyValue(ByVal parent As IDataCell, ByVal definition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IFrequencyValue

            Return New FrequencyValue(parent, definition, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Parent() As DataCell
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Shadows Property Definition() As FrequencyDefinition
            Get
                Return MyBase.Definition
            End Get
            Set(ByVal value As FrequencyDefinition)
                MyBase.Definition = value
            End Set
        End Property

        Protected Overrides ReadOnly Property BodyLength() As UInt16
            Get
                Dim length As UInt16

                If Definition.FrequencyAvailable Then length += 2
                If Definition.DfDtAvailable Then length += 2

                Return length
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)

                If Definition.FrequencyAvailable Then EndianOrder.BigEndian.CopyBytes(UnscaledFrequency, buffer, 0)
                If Definition.DfDtAvailable Then EndianOrder.BigEndian.CopyBytes(UnscaledDfDt, buffer, 2)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            ' Note that IEEE 1344 only supports scaled integers (no need to worry about floating points)
            If Definition.FrequencyAvailable Then
                UnscaledFrequency = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
                startIndex += 2
            End If

            If Definition.DfDtAvailable Then UnscaledDfDt = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)

        End Sub

    End Class

End Namespace