'*******************************************************************************************************
'  PhasorDefinition.vb - IEEE C37.118 Phasor definition
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

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class PhasorDefinition

        Inherits PhasorDefinitionBase

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single, ByVal format As CoordinateFormat, ByVal type As PhasorType, ByVal voltageReference As PhasorDefinition)

            MyBase.New(parent, index, label, scale, offset, format, type, voltageReference)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal phasorDefinition As IPhasorDefinition)

            MyBase.New(phasorDefinition)

        End Sub

        Friend Shared Function CreateNewPhasorDefintion(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IPhasorDefinition

            Return New PhasorDefinition(parent, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Friend Shared ReadOnly Property ConversionFactorLength() As Int32
            Get
                Return 4
            End Get
        End Property

        Friend ReadOnly Property ConversionFactorImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(ConversionFactorLength)

                buffer(0) = IIf(Of Byte)(Type = PhasorType.Voltage, 0, 1)

                EndianOrder.BigEndian.Copy(BitConverter.GetBytes(ScalingFactor), 0, buffer, 1, 3)

                Return buffer
            End Get
        End Property

        Friend Sub ParseConversionFactor(ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            Dim buffer As Byte() = CreateArray(Of Byte)(4)

            ' Get phasor type from first byte
            Type = IIf(binaryImage(startIndex) = 0, PhasorType.Voltage, PhasorType.Current)

            ' Last three bytes represent scaling factor
            EndianOrder.BigEndian.Copy(binaryImage, startIndex + 1, buffer, 0, 3)
            ScalingFactor = BitConverter.ToInt32(buffer, 0)

        End Sub

    End Class

End Namespace
