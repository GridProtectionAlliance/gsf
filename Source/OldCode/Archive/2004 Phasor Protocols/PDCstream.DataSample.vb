'***********************************************************************
'  PDCstream.DataSample.vb - PDC stream data sample
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

Namespace PDCstream

    ' This class represents a complete sample of data for a given second - a time indexed sub-second set of PMU data rows.
    ' Don't confuse this with a DataPacket even though the config file defines a "sample rate".  The sample rate actually
    ' defines how many DataPacket's (i.e., rows) there will be in each sample.  The sample rate defined in the config file
    ' is really the "DataPacket" rate.  Note also that the nomenclature I used for the class names here match what is in
    ' the PDC stream specification to help make things easier to understand.
    Public Class DataSample

        Implements IComparable

        Private m_configFile As ConfigFile
        Private m_timeStamp As DateTime
        Private m_published As Boolean

        Public Rows As DataPacket()

        Public Sub New(ByVal configFile As ConfigFile, ByVal timeStamp As DateTime)

            m_configFile = configFile
            m_timeStamp = DataQueue.BaselinedTimestamp(timeStamp)
            Rows = Array.CreateInstance(GetType(DataPacket), m_configFile.SampleRate)

            For x As Integer = 0 To Rows.Length - 1
                Rows(x) = New DataPacket(m_configFile, timeStamp, x)
            Next

        End Sub

        Public ReadOnly Property Timestamp() As DateTime
            Get
                Return m_timeStamp
            End Get
        End Property

        Public ReadOnly Property Published() As Boolean
            Get
                If Not m_published Then
                    Dim allPublished As Boolean = True

                    ' The sample has been completely processed once all data packets have been published - once all
                    ' the packets have been published, the data can be collated and sent to the permanent archive or
                    ' whatever else...
                    For x As Integer = 0 To Rows.Length - 1
                        If Not Rows(x).Published Then
                            allPublished = False
                            Exit For
                        End If
                    Next

                    If allPublished Then m_published = True
                    Return allPublished
                Else
                    Return True
                End If
            End Get
        End Property

        ' Data samples are sorted in timestamp order
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is DataSample Then
                Return m_timeStamp.CompareTo(DirectCast(obj, DataSample).Timestamp)
            Else
                Throw New ArgumentException("DataSample can only be compared with other DataSamples...")
            End If

        End Function

    End Class

End Namespace