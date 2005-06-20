'***********************************************************************
'  DataCell.vb - PMU Data Cell
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
    Public Class DataCell

        Inherits DataCellBase

        Private m_flags As ChannelFlags
        Private m_sampleRate As Byte
        Private m_phasorValueCount As Byte
        Private m_sampleNumber As Int32     ' TODO: Verify sample number is a 4 byte integer

        Private Const CommonDataOffset As Integer = 6

        Public Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell, ByVal sampleNumber As Integer)

            MyBase.New(parent, configurationCell)

            Dim x As Integer

            With configurationCell
                m_sampleRate = Convert.ToByte(.SampleRate)
                ' TODO: define some default value for version??
                m_phasorValueCount = Convert.ToByte(.PhasorDefinitions.Count)
                m_sampleNumber = sampleNumber

                If m_phasorValueCount <> .PhasorDefinitions.Count Then
                    Throw New InvalidOperationException("Stream/Config File Mismatch: Phasor value count in stream (" & m_phasorValueCount & ") does not match defined count in configuration file (" & .PhasorDefinitions.Count & ")")
                End If

                ' Initialize phasor values and frequency value with an empty value
                For x = 0 To .PhasorDefinitions.Count - 1
                    PhasorValues.Add(New PhasorValue(Me, .PhasorDefinitions(x), 0, 0))
                Next

                FrequencyValue = New FrequencyValue(Me, .FrequencyDefinition, 0, 0)

                ' Initialize any digital values
                For x = 0 To .DigitalDefinitions.Count - 1
                    DigitalValues.Add(New DigitalValue(Me, .DigitalDefinitions(x), 0))
                Next
            End With

        End Sub

        Public Sub New(ByVal dataCell As IDataCell)

            MyBase.New(dataCell)

        End Sub

        Public Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            ' Parse binary common data to all data cells
            MyBase.New(parent, configurationCell, binaryImage, startIndex + CommonDataOffset, GetType(PhasorValue), GetType(FrequencyValue), Nothing, GetType(DigitalValue))

            ' Parse PDCstream specific image
            ' TODO: See if adding a "ParserHeaderImage" would be appropriate here...
            m_flags = binaryImage(startIndex)
            m_sampleRate = binaryImage(startIndex + 1)
            ' TODO: Determine what this flag is - version??
            'm_version = binaryImage(startIndex + 2)
            m_phasorValueCount = binaryImage(startIndex + 3)
            m_sampleNumber = EndianOrder.ReverseToInt32(binaryImage, startIndex + 4)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        ' Note: this is only the first byte of the channel flag word
        Public ReadOnly Property ChannelFlags() As ChannelFlags
            Get
                Return m_flags
            End Get
        End Property

        Public Property SampleRate() As Byte
            Get
                Return m_sampleRate
            End Get
            Set(ByVal Value As Byte)
                m_sampleRate = Value
            End Set
        End Property

        Public Property PhasorValueCount() As Byte
            Get
                Return m_phasorValueCount
            End Get
            Set(ByVal Value As Byte)
                m_phasorValueCount = Value
            End Set
        End Property

        Public ReadOnly Property SampleNumber() As Int32
            Get
                Return m_sampleNumber
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

        <Obsolete("This bit definition is for obsolete uses that is no longer needed.", False)> _
        Public Property DataIsSortedByTimestamp() As Boolean
            Get
                Return ((m_flags And ChannelFlags.DataSortedByTimestamp) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.DataSortedByTimestamp
                Else
                    m_flags = m_flags Or ChannelFlags.DataSortedByTimestamp
                End If
            End Set
        End Property

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

        <Obsolete("This bit definition is for obsolete uses that is no longer needed.", False)> _
        Public Property TimestampIsIncluded() As Boolean
            Get
                Return ((m_flags And ChannelFlags.TimestampIncluded) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.TimestampIncluded
                Else
                    m_flags = m_flags Or ChannelFlags.TimestampIncluded
                End If
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As Int16
            Get
                Return 10
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)
                Dim x, index As Integer

                ' Add PDCstream specific image
                buffer(0) = m_flags
                buffer(1) = m_sampleRate
                ' TODO: check this value - could be version flag??
                buffer(2) = Convert.ToByte(2)
                buffer(3) = m_phasorValueCount
                EndianOrder.SwapCopyBytes(Convert.ToInt32(m_sampleNumber), buffer, 4)

                Return buffer
            End Get
        End Property

    End Class

End Namespace