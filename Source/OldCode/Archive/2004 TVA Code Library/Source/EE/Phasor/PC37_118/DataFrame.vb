'***********************************************************************
'  DataFrame.vb - PC37_118 Data Frame
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
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor.PC37_118

    ' This class represents a data frame that will be sent from a PMU during its real time data transmission.
    Public Class DataFrame

        Inherits BaseFrame

        Protected m_configFile As ConfigurationFile
        Protected m_phasorFormat As PhasorFormat
        Protected m_sampleRate As Integer
        Protected m_sampleStep As Integer
        Protected m_phasorValues As PhasorValues
        Protected m_digitalValues As DigitalValues
        Protected m_frequency As Int16
        Protected m_dfdt As Int16

        Protected Const SampleCountMask As Int16 = Not FrameTypeMask

        Public Const MaximumSampleCount As Int16 = SampleCountMask

        Public Sub New(ByVal configFile2 As ConfigurationFile, ByVal phasorFormat As PhasorFormat)

            MyBase.New()
            SetFrameType(PMUFrameType.DataFrame)
            If configFile2 Is Nothing Then Throw New ArgumentNullException("Cannot parse " & Name & " without receiving configuration file 2 first")
            m_configFile = configFile2
            m_phasorFormat = phasorFormat
            m_sampleRate = 30
            m_sampleStep = 40
            m_phasorValues = New PhasorValues
            m_digitalValues = New DigitalValues

        End Sub

        Protected Friend Sub New(ByVal parsedImage As BaseFrame, ByVal configFile2 As ConfigurationFile, ByVal binaryImage As Byte(), ByVal startIndex As Integer, ByVal phasorFormat As PhasorFormat)

            Me.New(configFile2, phasorFormat)

            Dim x, index As Integer
            Dim value As PhasorValue

            ' No need to reparse data, so we pickup what's already been parsed...
            Clone(parsedImage)

            ' Load data frame image
            ParseDataImage(binaryImage, startIndex)

        End Sub

        ' Combine time tag and sample count to create usable timestamp valid at a millisecond level
        Public Overridable ReadOnly Property Timestamp() As DateTime
            Get
                Return TimeTag.ToDateTime.AddMilliseconds(1000 * (SampleCount / m_sampleStep / m_sampleRate))
            End Get
        End Property

        Public Overridable Property SampleCount() As Int16
            Get
                Return m_sampleCount And SampleCountMask
            End Get
            Set(ByVal Value As Int16)
                If Value > MaximumSampleCount Then
                    Throw New OverflowException("Sample count value cannot exceed " & MaximumSampleCount)
                Else
                    m_sampleCount = (m_sampleCount And Not SampleCountMask) Or Value
                End If
            End Set
        End Property

        Public Property SampleRate() As Integer
            Get
                Return m_sampleRate
            End Get
            Set(ByVal Value As Integer)
                m_sampleRate = Value
            End Set
        End Property

        Public Property SampleStep() As Integer
            Get
                Return m_sampleStep
            End Get
            Set(ByVal Value As Integer)
                m_sampleStep = Value
            End Set
        End Property

        Public Property PhasorFormat() As PhasorFormat
            Get
                Return m_phasorFormat
            End Get
            Set(ByVal Value As PhasorFormat)
                m_phasorFormat = Value
            End Set
        End Property

        Public ReadOnly Property PhasorValues() As PhasorValues
            Get
                Return m_phasorValues
            End Get
        End Property

        Public ReadOnly Property DigitalValues() As DigitalValues
            Get
                Return m_digitalValues
            End Get
        End Property

        Public ReadOnly Property NominalFrequency() As Double
            Get
                If m_configFile Is Nothing Then
                    Return 60
                Else
                    If m_configFile.LineFrequency = PMULineFrequency._50Hz Then
                        Return 50
                    Else
                        Return 60
                    End If
                End If
            End Get
        End Property

        Public Property Frequency() As Double
            Get
                Return NominalFrequency + m_frequency / 1000
            End Get
            Set(ByVal Value As Double)
                m_frequency = Convert.ToInt16((Value - NominalFrequency) * 1000)
            End Set
        End Property

        Public Property DfDt() As Double
            Get
                Return m_dfdt / 100
            End Get
            Set(ByVal Value As Double)
                m_dfdt = Convert.ToInt16(Value * 100)
            End Set
        End Property

        Protected Overrides ReadOnly Property Name() As String
            Get
                Return "PC37_118.DataFrame"
            End Get
        End Property

        Public Overrides Property DataImage() As Byte()
            Get
                Dim image As Byte() = Array.CreateInstance(GetType(Byte), DataLength)
                Dim x, index As Integer

                For x = 0 To m_phasorValues.Count - 1
                    System.Buffer.BlockCopy(m_phasorValues(x).BinaryImage(m_phasorFormat), 0, image, x * PhasorValue.BinaryLength, PhasorValue.BinaryLength)
                Next

                index = m_phasorValues.Count * PhasorValue.BinaryLength

                EndianOrder.SwapCopyBytes(m_frequency, image, index)
                EndianOrder.SwapCopyBytes(m_dfdt, image, index + 2)

                index += 4

                For x = 0 To m_digitalValues.Count - 1
                    EndianOrder.SwapCopyBytes(m_digitalValues(x), image, index + x * 2)
                Next

                Return image
            End Get
            Set(ByVal Value As Byte())
                ParseDataImage(Value, 0)
            End Set
        End Property

        Private Sub ParseDataImage(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Dim x, index As Integer

            m_phasorValues.Clear()

            For x = 0 To m_configFile.PhasorDefinitions.Count - 1
                m_phasorValues.Add(New PhasorValue(m_configFile.PhasorDefinitions(x), binaryImage, startIndex + x * PhasorValue.BinaryLength, PhasorFormat))
            Next

            index = startIndex + m_configFile.PhasorDefinitions.Count * PhasorValue.BinaryLength

            m_frequency = EndianOrder.ReverseToInt16(binaryImage, index)
            m_dfdt = EndianOrder.ReverseToInt16(binaryImage, index + 2)

            index += 4

            m_digitalValues.Clear()

            For x = 0 To m_digitalValues.Count - 1
                m_digitalValues.Add(EndianOrder.ReverseToInt16(binaryImage, index + x * 2))
            Next

        End Sub

    End Class

End Namespace