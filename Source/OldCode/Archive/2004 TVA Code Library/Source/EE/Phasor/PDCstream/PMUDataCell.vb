'***********************************************************************
'  PMUDataCell.vb - PMU Data Cell
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

    ' This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
    Public Class PMUDataCell

        Inherits DataCellBase

        Private m_pmuDefinition As PMUDefinition
        Private m_sampleNumber As Integer
        Private m_flags As ChannelFlags

        Public Shared Shadows Function CreateFrom(ByVal phasorDataCell As IDataCell) As PMUDataCell

            Return CType(Activator.CreateInstance(GetType(PMUDataCell), New Object() {phasorDataCell}), PMUDataCell)

        End Function

        Public Sub New(ByVal pmuDefinition As PMUDefinition, ByVal sampleNumber As Integer)

            MyBase.New()

            m_pmuDefinition = pmuDefinition
            m_sampleNumber = sampleNumber

            'PhasorValues = Array.CreateInstance(GetType(PhasorValue), m_pmuDefinition.Phasors.Length)

            ' Initialize phasor values and frequency value with an "empty" value
            For x As Integer = 0 To m_pmuDefinition.Phasors.Length - 1
                'Me.PhasorValues.Add(    '                PhasorValues(x) = PhasorValue.Empty(m_pmuDefinition.Phasors(x))
            Next

            FrequencyValue = New FrequencyValue(m_pmuDefinition.Frequency, 0, 0)

        End Sub

        Public Sub New(ByVal phasorDataCell As IDataCell)

            MyBase.New(phasorDataCell)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

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

                For x As Integer = 0 To PhasorValues.Count - 1
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
                Return ((m_flags And ChannelFlags.DataIsValid) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.DataIsValid
                Else
                    m_flags = m_flags Or ChannelFlags.DataIsValid
                End If
            End Set
        End Property

        Public Property TransmissionErrors() As Boolean
            Get
                Return ((m_flags And ChannelFlags.TransmissionErrors) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.TransmissionErrors
                Else
                    m_flags = m_flags And Not ChannelFlags.TransmissionErrors
                End If
            End Set
        End Property

        Public Property PMUSynchronized() As Boolean
            Get
                Return ((m_flags And ChannelFlags.PMUSynchronized) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.PMUSynchronized
                Else
                    m_flags = m_flags Or ChannelFlags.PMUSynchronized
                End If
            End Set
        End Property

        Public Property DataIsSortedByArrival() As Boolean
            Get
                Return ((m_flags And ChannelFlags.DataSortedByArrival) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.DataSortedByArrival
                Else
                    m_flags = m_flags And Not ChannelFlags.DataSortedByArrival
                End If
            End Set
        End Property

        'Public Property DataIsSortedByTimestamp() As Boolean
        '    Get
        '        Return ((m_flags And ChannelFlags.DataSortedByTimestamp) = 0)
        '    End Get
        '    Set(ByVal Value As Boolean)
        '        If Value Then
        '            m_flags = m_flags And Not ChannelFlags.DataSortedByTimestamp
        '        Else
        '            m_flags = m_flags Or ChannelFlags.DataSortedByTimestamp
        '        End If
        '    End Set
        'End Property

        Public Property UsingPDCExchangeFormat() As Boolean
            Get
                Return ((m_flags And ChannelFlags.PDCExchangeFormat) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.PDCExchangeFormat
                Else
                    m_flags = m_flags And Not ChannelFlags.PDCExchangeFormat
                End If
            End Set
        End Property

        Public Property UsingMacrodyneFormat() As Boolean
            Get
                Return ((m_flags And ChannelFlags.MacrodyneFormat) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.MacrodyneFormat
                Else
                    m_flags = m_flags And Not ChannelFlags.MacrodyneFormat
                End If
            End Set
        End Property

        Public Property UsingIEEEFormat() As Boolean
            Get
                Return ((m_flags And ChannelFlags.MacrodyneFormat) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.MacrodyneFormat
                Else
                    m_flags = m_flags Or ChannelFlags.MacrodyneFormat
                End If
            End Set
        End Property

        'Public Property TimestampIsIncluded() As Boolean
        '    Get
        '        Return ((m_flags And ChannelFlags.TimestampIncluded) = 0)
        '    End Get
        '    Set(ByVal Value As Boolean)
        '        If Value Then
        '            m_flags = m_flags And Not ChannelFlags.TimestampIncluded
        '        Else
        '            m_flags = m_flags Or ChannelFlags.TimestampIncluded
        '        End If
        '    End Set
        'End Property

        'Public ReadOnly Property BinaryLength() As Integer
        '    Get
        '        Return 12 + FrequencyValue.BinaryLength + PhasorValue.BinaryLength * PhasorValues.Length
        '    End Get
        'End Property

        'Public ReadOnly Property BinaryImage() As Byte()
        '    Get
        '        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
        '        Dim index As Integer

        '        buffer(0) = m_flags
        '        buffer(1) = Convert.ToByte(m_pmuDefinition.SampleRate)
        '        buffer(2) = Convert.ToByte(2)
        '        buffer(3) = Convert.ToByte(PhasorValues.Length)
        '        EndianOrder.SwapCopyBytes(Convert.ToInt16(m_sampleNumber), buffer, 4)
        '        EndianOrder.SwapCopyBytes(StatusFlags, buffer, 6)
        '        index = 8

        '        For x As Integer = 0 To PhasorValues.Length - 1
        '            BlockCopy(PhasorValues(x).BinaryImage, 0, buffer, index, PhasorValue.BinaryLength)
        '            index += PhasorValue.BinaryLength
        '        Next

        '        BlockCopy(FrequencyValue.BinaryImage, 0, buffer, index, FrequencyValue.BinaryLength)
        '        index += FrequencyValue.BinaryLength

        '        EndianOrder.SwapCopyBytes(Digital0, buffer, index)
        '        EndianOrder.SwapCopyBytes(Digital1, buffer, index + 2)

        '        Return buffer
        '    End Get
        'End Property

    End Class

End Namespace