'***********************************************************************
'  PDCstream.PMUDataCell.vb - PMU Data Cell
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
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

Imports DatAWarePDC.Interop
Imports DatAWarePDC.PDCstream.Common

Namespace PDCstream

    ' This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
    Public Class PMUDataCell

        Private m_pmuDefinition As PMUDefinition
        Private m_sampleNumber As Integer
        Private m_flags As ChannelFlags

        Public StatusFlags As Int16
        Public PhasorValues As PhasorValue()
        Public FrequencyValue As FrequencyValue
        Public Digital0 As Int16
        Public Digital1 As Int16

        Public Sub New(ByVal pmuDefinition As PMUDefinition, ByVal sampleNumber As Integer)

            m_pmuDefinition = pmuDefinition
            m_sampleNumber = sampleNumber

            PhasorValues = Array.CreateInstance(GetType(PhasorValue), m_pmuDefinition.Phasors.Length)

            ' Initialize phasor values and frequency value with an "empty" value
            For x As Integer = 0 To PhasorValues.Length - 1
                PhasorValues(x) = PhasorValue.Empty(m_pmuDefinition.Phasors(x))
            Next

            FrequencyValue = FrequencyValue.Empty(m_pmuDefinition.Frequency)

        End Sub

        Public ReadOnly Property PMUDefinition() As PMUDefinition
            Get
                Return m_pmuDefinition
            End Get
        End Property

        Public ReadOnly Property SampleNumber() As Integer
            Get
                Return m_sampleNumber
            End Get
        End Property

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Dim empty As Boolean

                For x As Integer = 0 To PhasorValues.Length - 1
                    If PhasorValues(x).IsEmpty Then
                        empty = True
                        Exit For
                    End If
                Next

                If Not empty Then empty = FrequencyValue.IsEmpty

                Return empty
            End Get
        End Property

        ' Note: this is only the first byte of the channel flag word
        Public ReadOnly Property ChannelFlags() As ChannelFlags
            Get
                Return m_flags
            End Get
        End Property

        Public Property DataIsValid() As Boolean
            Get
                Return Not (m_flags And ChannelFlags.DataIsValid)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or Not ChannelFlags.DataIsValid
                Else
                    m_flags = m_flags Or ChannelFlags.DataIsValid
                End If
            End Set
        End Property

        Public Property TransmissionErrors() As Boolean
            Get
                Return (m_flags And ChannelFlags.TransmissionErrors)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.TransmissionErrors
                Else
                    m_flags = m_flags Or Not ChannelFlags.TransmissionErrors
                End If
            End Set
        End Property

        Public Property PMUSynchronized() As Boolean
            Get
                Return Not (m_flags And ChannelFlags.PMUSynchronized)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or Not ChannelFlags.PMUSynchronized
                Else
                    m_flags = m_flags Or ChannelFlags.PMUSynchronized
                End If
            End Set
        End Property

        Public Property DataIsSortedByArrival() As Boolean
            Get
                Return (m_flags And ChannelFlags.DataSortedByArrival)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.DataSortedByArrival
                Else
                    m_flags = m_flags Or Not ChannelFlags.DataSortedByArrival
                End If
            End Set
        End Property

        Public Property DataIsSortedByTimestamp() As Boolean
            Get
                Return Not (m_flags And ChannelFlags.DataSortedByTimestamp)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or Not ChannelFlags.DataSortedByTimestamp
                Else
                    m_flags = m_flags Or ChannelFlags.DataSortedByTimestamp
                End If
            End Set
        End Property

        Public Property UsingPDCExchangeFormat() As Boolean
            Get
                Return (m_flags And ChannelFlags.PDCExchangeFormat)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.PDCExchangeFormat
                Else
                    m_flags = m_flags Or Not ChannelFlags.PDCExchangeFormat
                End If
            End Set
        End Property

        Public Property UsingMacrodyneFormat() As Boolean
            Get
                Return (m_flags And ChannelFlags.MacrodyneFormat)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.MacrodyneFormat
                Else
                    m_flags = m_flags Or Not ChannelFlags.MacrodyneFormat
                End If
            End Set
        End Property

        Public Property UsingIEEEFormat() As Boolean
            Get
                Return Not (m_flags And ChannelFlags.MacrodyneFormat)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or Not ChannelFlags.MacrodyneFormat
                Else
                    m_flags = m_flags Or ChannelFlags.MacrodyneFormat
                End If
            End Set
        End Property

        Public Property TimestampIsIncluded() As Boolean
            Get
                Return Not (m_flags And ChannelFlags.TimestampIncluded)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or Not ChannelFlags.TimestampIncluded
                Else
                    m_flags = m_flags Or ChannelFlags.TimestampIncluded
                End If
            End Set
        End Property

        Public ReadOnly Property BinaryLength() As Integer
            Get
                Return 12 + FrequencyValue.BinaryLength + PhasorValue.BinaryLength * PhasorValues.Length
            End Get
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim index As Integer

                buffer(0) = m_flags
                buffer(1) = Convert.ToByte(m_pmuDefinition.SampleRate)
                buffer(2) = Convert.ToByte(2)
                buffer(3) = Convert.ToByte(PhasorValues.Length)
                EndianOrder.SwapCopy(BitConverter.GetBytes(Convert.ToInt16(m_sampleNumber)), 0, buffer, 4, 2)
                EndianOrder.SwapCopy(BitConverter.GetBytes(StatusFlags), 0, buffer, 6, 2)
                index = 8

                For x As Integer = 0 To PhasorValues.Length - 1
                    Array.Copy(PhasorValues(x).BinaryImage, 0, buffer, index, PhasorValue.BinaryLength)
                    index += PhasorValue.BinaryLength
                Next

                Array.Copy(FrequencyValue.BinaryImage, 0, buffer, index, FrequencyValue.BinaryLength)
                index += FrequencyValue.BinaryLength

                EndianOrder.SwapCopy(BitConverter.GetBytes(Digital0), 0, buffer, index, 2)
                EndianOrder.SwapCopy(BitConverter.GetBytes(Digital1), 0, buffer, index + 2, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace