'*******************************************************************************************************
'  PhasorDefinition.vb - Phasor definition
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
Imports Tva.Collections.Common

Namespace Ieee1344

    <CLSCompliant(False)> _
    Public Class PhasorDefinition

        Inherits PhasorDefinitionBase

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)
            MyBase.DataFormat = Phasors.DataFormat.FixedInteger

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal dataFormat As DataFormat, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single, ByVal format As CoordinateFormat, ByVal type As PhasorType, ByVal voltageReference As PhasorDefinition)

            MyBase.New(parent, dataFormat, index, label, scale, offset, format, type, voltageReference)
            MyBase.DataFormat = Phasors.DataFormat.FixedInteger

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, binaryImage, startIndex)
            MyBase.DataFormat = Phasors.DataFormat.FixedInteger

        End Sub

        Public Sub New(ByVal phasorDefinition As IPhasorDefinition)

            MyBase.New(phasorDefinition)
            MyBase.DataFormat = Phasors.DataFormat.FixedInteger

        End Sub

        Friend Shared Function CreateNewPhasorDefintion(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IPhasorDefinition

            Return New PhasorDefinition(parent, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        ' IEEE 1344 only supports scaled data
        Public Overrides Property DataFormat() As DataFormat
            Get
                Return Phasors.DataFormat.FixedInteger
            End Get
            Set(ByVal value As DataFormat)
                If value = Phasors.DataFormat.FixedInteger Then
                    MyBase.DataFormat = value
                Else
                    Throw New InvalidOperationException("IEEE 1344 only supports scaled integers - floating points are not allowed")
                End If
            End Set
        End Property

        Friend Shared ReadOnly Property ConversionFactorLength() As Int32
            Get
                Return 4
            End Get
        End Property

        Friend ReadOnly Property ConversionFactorImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), ConversionFactorLength)

                buffer(0) = IIf(Of Byte)(Type = PhasorType.Voltage, 0, 1)

                EndianOrder.BigEndian.Copy(BitConverter.GetBytes(ScalingFactor), 0, buffer, 1, 3)

                Return buffer
            End Get
        End Property

        Friend Sub ParseConversionFactor(ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4)

            ' Get phasor type from first byte
            Type = IIf(binaryImage(startIndex) = 0, PhasorType.Voltage, PhasorType.Current)

            ' Last three bytes represent scaling factor
            EndianOrder.BigEndian.Copy(binaryImage, startIndex + 1, buffer, 0, 3)
            ScalingFactor = BitConverter.ToInt32(buffer, 0)

        End Sub

    End Class

End Namespace
