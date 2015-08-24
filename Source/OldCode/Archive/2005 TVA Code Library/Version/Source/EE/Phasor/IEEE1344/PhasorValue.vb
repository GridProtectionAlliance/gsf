'*******************************************************************************************************
'  PhasorValue.vb - Phasor value
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Imports TVA.Interop
Imports TVA.Shared.Math

Namespace EE.Phasor.IEEE1344

    Public Class PhasorValue

        Private m_phasorDefinition As PhasorDefinition
        Private m_real As Int16
        Private m_imaginary As Int16
        Private m_compositeValues As CompositeValues

        Private Enum CompositeValue
            Angle
            Magnitude
        End Enum

        Public Const BinaryLength As Integer = 4

        Public Shared ReadOnly Property Empty(ByVal phasorDefinition As PhasorDefinition) As PhasorValue
            Get
                Return New PhasorValue(phasorDefinition, 0, 0)
            End Get
        End Property

        'Create phasor from polar coordinates (angle expected in Degrees)
        Public Shared Function CreateFromPolarValues(ByVal phasorDefinition As PhasorDefinition, ByVal angle As Double, ByVal magnitude As Double) As EE.Phasor.IEEE1344.PhasorValue

            Return CreateFromScaledRectangularValues(phasorDefinition, CalculateRealComponent(angle, magnitude), CalculateImaginaryComponent(angle, magnitude))

        End Function

        'Create phasor from scaled rectangular coordinates
        Public Shared Function CreateFromScaledRectangularValues(ByVal phasorDefinition As PhasorDefinition, ByVal real As Double, ByVal imaginary As Double) As EE.Phasor.IEEE1344.PhasorValue

            Dim scale As Double = phasorDefinition.CalFactor
            Return CreateFromUnscaledRectangularValues(phasorDefinition, real / scale, imaginary / scale)

        End Function

        'Create phasor from unscaled rectangular coordinates
        Public Shared Function CreateFromUnscaledRectangularValues(ByVal phasorDefinition As PhasorDefinition, ByVal real As Int16, ByVal imaginary As Int16) As EE.Phasor.IEEE1344.PhasorValue

            Return New PhasorValue(phasorDefinition, real, imaginary)

        End Function

        ' Gets real component from angle (in Degrees) and magnitude
        Public Shared Function CalculateRealComponent(ByVal angle As Double, ByVal magnitude As Double) As Double

            Return magnitude * Math.Cos(angle * Math.PI / 180)

        End Function

        ' Gets imaginary component from angle (in Degrees) and magnitude
        Public Shared Function CalculateImaginaryComponent(ByVal angle As Double, ByVal magnitude As Double) As Double

            Return magnitude * Math.Sin(angle * Math.PI / 180)

        End Function

        ' Calculate watts from imaginary and real components of two phasors
        Public Shared Function CalculatePower(ByVal voltage As PhasorValue, ByVal current As PhasorValue) As Double

            'Return 3 * (voltage.ScaledReal * current.ScaledReal + voltage.ScaledImaginary * current.ScaledImaginary)
            Return Math.Sqrt(3) * voltage.Magnitude * current.Magnitude * Math.Cos((voltage.Angle - current.Angle) * Math.PI / 180)

        End Function

        ' Calculate vars from imaginary and real components of two phasors
        Public Shared Function CalculateVars(ByVal voltage As PhasorValue, ByVal current As PhasorValue) As Double

            'Return 3 * (voltage.ScaledImaginary * current.ScaledReal - voltage.ScaledReal * current.ScaledImaginary)
            Return Math.Sqrt(3) * voltage.Magnitude * current.Magnitude * Math.Sin((voltage.Angle - current.Angle) * Math.PI / 180)

        End Function

        Public Sub New(ByVal phasorDefinition As PhasorDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer, ByVal phasorFormat As CoordinateFormat)

            m_phasorDefinition = phasorDefinition

            If phasorFormat = phasorFormat.Rectangular Then
                m_real = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
                m_imaginary = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
            Else
                Dim magnitude As Double = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
                Dim angle As Double = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2) * 180 / Math.PI / 10000

                With CreateFromPolarValues(phasorDefinition, angle, magnitude)
                    m_real = .Real
                    m_imaginary = .Imaginary
                End With
            End If

            m_compositeValues = New CompositeValues(2)

        End Sub

        Private Sub New(ByVal phasorDefinition As PhasorDefinition, ByVal real As Int16, ByVal imaginary As Int16)

            m_phasorDefinition = phasorDefinition
            m_real = real
            m_imaginary = imaginary
            m_compositeValues = New CompositeValues(2)

        End Sub

        Public ReadOnly Property PhasorDefinition() As PhasorDefinition
            Get
                Return m_phasorDefinition
            End Get
        End Property

        Public ReadOnly Property ScalingFactor() As Double
            Get
                Return m_phasorDefinition.CalFactor
            End Get
        End Property

        Public Property Angle() As Double
            Get
                Return Math.Atan2(ScaledImaginary, ScaledReal) * 180 / Math.PI
            End Get
            Set(ByVal Value As Double)
                ' We store angle as one of our required composite values
                m_compositeValues(CompositeValue.Angle) = Value

                ' If all composite values have been received, we can calculate phasor's real and imaginary values
                CalculatePhasorValue()
            End Set
        End Property

        Public Property Magnitude() As Double
            Get
                Return Math.Sqrt(ScaledReal * ScaledReal + ScaledImaginary * ScaledImaginary)
            End Get
            Set(ByVal Value As Double)
                ' We store magnitude as one of our required composite values
                m_compositeValues(CompositeValue.Magnitude) = Value

                ' If all composite values have been received, we can calculate phasor's real and imaginary values
                CalculatePhasorValue()
            End Set
        End Property

        Public ReadOnly Property Real() As Int16
            Get
                Return m_real
            End Get
        End Property

        Public ReadOnly Property Imaginary() As Int16
            Get
                Return m_imaginary
            End Get
        End Property

        Public ReadOnly Property ScaledReal() As Double
            Get
                Return Convert.ToDouble(m_real) * ScalingFactor
            End Get
        End Property

        Public ReadOnly Property ScaledImaginary() As Double
            Get
                Return Convert.ToDouble(m_imaginary) * ScalingFactor
            End Get
        End Property

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return (m_real = 0 And m_imaginary = 0)
            End Get
        End Property

        Private Sub CalculatePhasorValue()

            If m_compositeValues.AllReceived Then
                Dim angle, magnitude, scale As Double

                ' All values received, create a new phasor value from composite values
                angle = m_compositeValues(CompositeValue.Angle)
                magnitude = m_compositeValues(CompositeValue.Magnitude)
                scale = ScalingFactor

                m_real = CalculateRealComponent(angle, magnitude) / scale
                m_imaginary = CalculateImaginaryComponent(angle, magnitude) / scale
            End If

        End Sub

        Public ReadOnly Property BinaryImage(ByVal phasorFormat As CoordinateFormat) As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                If phasorFormat = phasorFormat.Rectangular Then
                    EndianOrder.BigEndian.CopyBytes(m_real, buffer, 0)
                    EndianOrder.BigEndian.CopyBytes(m_imaginary, buffer, 2)
                Else
                    EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Magnitude), buffer, 0)
                    EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Angle * Math.PI / 180 * 10000), buffer, 2)
                End If

                Return buffer
            End Get
        End Property

    End Class

End Namespace