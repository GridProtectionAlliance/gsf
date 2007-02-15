'***********************************************************************
'  DataBlocl.vb - DatAWare Data Block - allows bulk point insertion
'  Copyright © 2005 - TVA, all rights reserved
'  
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  10/1/2004 - James R Carroll
'       Initial version of source created
'
'***********************************************************************
Option Explicit On 

Namespace DatAWare

    Public Class DataBlock

        Public Channels As Integer()
        Public Values As Single()
        Public TimeTags As Double()
        Public Qualities As Quality()
        Public DefaultTimeTag As Double

        Public Sub New(ByVal blockSize As Integer)

            Channels = Array.CreateInstance(GetType(Integer), blockSize)
            Values = Array.CreateInstance(GetType(Single), blockSize)
            TimeTags = Array.CreateInstance(GetType(Double), blockSize)
            Qualities = Array.CreateInstance(GetType(Quality), blockSize)
            DefaultTimeTag = (New TimeTag(DateTime.Now.ToUniversalTime)).Value

        End Sub

        Public Sub New(ByVal blockSize As Integer, ByVal defaultTimeTag As DateTime)

            Me.New(blockSize)
            Me.DefaultTimeTag = (New TimeTag(defaultTimeTag)).Value

        End Sub

        Public Sub New(ByVal blockSize As Integer, ByVal defaultTimeTag As TimeTag)

            Me.New(blockSize)
            Me.DefaultTimeTag = defaultTimeTag.Value

        End Sub

        Public Sub New(ByVal blockSize As Integer, ByVal defaultTimeTag As Double)

            Me.New(blockSize)
            Me.DefaultTimeTag = defaultTimeTag

        End Sub

        Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal value As Single, Optional ByVal qual As Quality = Quality.Good)

            SetRow(rowIndex, databaseIndex, DefaultTimeTag, value, qual)

        End Sub

        Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal utcTimestamp As DateTime, ByVal value As Single, Optional ByVal qual As Quality = Quality.Good)

            SetRow(rowIndex, databaseIndex, (New TimeTag(utcTimestamp)).Value, value, qual)

        End Sub

        Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal ttag As TimeTag, ByVal value As Single, Optional ByVal qual As Quality = Quality.Good)

            SetRow(rowIndex, databaseIndex, ttag.Value, value, qual)

        End Sub

        Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal ttag As Double, ByVal value As Single, Optional ByVal qual As Quality = Quality.Good)

            Channels(rowIndex) = databaseIndex
            Values(rowIndex) = value
            TimeTags(rowIndex) = ttag
            Qualities(rowIndex) = qual

        End Sub

        Public Function Archive(ByVal connection As Connection) As ReturnStatus

            Dim status As ReturnStatus

            connection.DWAPI.Archive_Put(connection.PlantCode, Channels, Values, TimeTags, Qualities, Channels.Length, status)

            Return status

        End Function

    End Class

End Namespace