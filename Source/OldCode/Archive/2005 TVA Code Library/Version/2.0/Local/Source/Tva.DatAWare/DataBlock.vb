'*******************************************************************************************************
'  DataBlock.vb - DatAWare Data Block Class - allows bulk point insertion
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/03/2006 - James R Carroll
'       Initial version of source imported from 1.1 code library
'
'*******************************************************************************************************

Public Class DataBlock

    Public Channels As Integer()
    Public Values As Single()
    Public TimeTags As Double()
    Public Qualities As Quality()
    Public DefaultTimeTag As Double

    Public Sub New(ByVal blockSize As Integer)

        Channels = CreateArray(Of Integer)(blockSize)
        Values = CreateArray(Of Single)(blockSize)
        TimeTags = CreateArray(Of Double)(blockSize)
        Qualities = CreateArray(Of Quality)(blockSize)
        DefaultTimeTag = (New TimeTag(Date.UtcNow)).Value

    End Sub

    Public Sub New(ByVal blockSize As Integer, ByVal defaultTimeTag As Date)

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

    Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal value As Single, ByVal qual As Quality)

        SetRow(rowIndex, databaseIndex, DefaultTimeTag, value, qual)

    End Sub

    Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal utcTimestamp As Date, ByVal value As Single, ByVal qual As Quality)

        SetRow(rowIndex, databaseIndex, (New TimeTag(utcTimestamp)).Value, value, qual)

    End Sub

    Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal ttag As TimeTag, ByVal value As Single, ByVal qual As Quality)

        SetRow(rowIndex, databaseIndex, ttag.Value, value, qual)

    End Sub

    Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal ttag As Double, ByVal value As Single, ByVal qual As Quality)

        Channels(rowIndex) = databaseIndex
        Values(rowIndex) = value
        TimeTags(rowIndex) = ttag
        Qualities(rowIndex) = qual

    End Sub

    Public Function Archive(ByVal connection As Connection) As ReturnStatus

        Dim status As ReturnStatus

        'connection.DWAPI.Archive_Put(connection.PlantCode, Channels, Values, TimeTags, Qualities, Channels.Length, status)

        Return status

    End Function

End Class
