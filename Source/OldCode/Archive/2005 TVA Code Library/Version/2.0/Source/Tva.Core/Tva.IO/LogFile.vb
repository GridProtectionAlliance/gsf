'*******************************************************************************************************
'  LogFile.vb - Log file implementation
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
'  02/10/2006 - James R Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Threading
Imports System.Text
Imports System.IO
Imports Tva.Collections
Imports Tva.IO.FilePath

Namespace IO

    ''' <summary>
    ''' This implements a simple multi-thread-happy log file class
    ''' </summary>
    Public Class LogFile

        Private m_logFileName As String
        Private m_logFileLock As ReaderWriterLock
        Private WithEvents m_logFileDataQueue As ProcessQueue(Of String)

        ' We don't stop for exceptions in this class, but will expose them if the end user wishes
        ' to know about any issues incurred while trying to log data
        Public Event LogException(ByVal ex As Exception)

        Public Sub New(ByVal logFileName As String)

            m_logFileName = logFileName
            m_logFileLock = New ReaderWriterLock
            m_logFileDataQueue = ProcessQueue(Of String).CreateSynchronousQueue(AddressOf ProcessLogQueue)

            ' We'll requeue data if we fail to aquire writer lock or fail to write data into file...
            m_logFileDataQueue.RequeueOnException = True

        End Sub

        Public Property LogFileName() As String
            Get
                Return m_logFileName
            End Get
            Set(ByVal value As String)
                m_logFileName = value
            End Set
        End Property

        Public Sub Append(ByVal status As String)

            m_logFileDataQueue.Add(status)

        End Sub

        Public Sub AppendLine(ByVal status As String)

            Append(status & Environment.NewLine)

        End Sub

        Public Sub AppendTimestampedLine(ByVal status As String)

            Append("[" & Date.Now & "] " & status & Environment.NewLine)

        End Sub

        Public Overrides Function ToString() As String

            Dim logData As String = ""

            ' Attempt to aquire a reader lock
            m_logFileLock.AcquireReaderLock(500)

            Try
                ' Read log contents into a string
                If File.Exists(m_logFileName) Then
                    With File.OpenText(m_logFileName)
                        logData = .ReadToEnd
                        .Close()
                    End With
                End If
            Catch
                ' Rethrow any exceptions to calling procedure - just catching here so
                ' we can make sure lock gets released in finally clause
                Throw
            Finally
                ' Make sure reader lock gets released
                m_logFileLock.ReleaseReaderLock()
            End Try

            Return logData

        End Function

        ' We process all available items in the queue...
        Private Sub ProcessLogQueue(ByVal items As String())

            ' Attempt to aquire a writer lock
            m_logFileLock.AcquireWriterLock(500)

            Try
                ' Append queued data to log file
                With File.AppendText(m_logFileName)
                    For Each item As String In items
                        .Write(item)
                    Next
                    .Flush()
                    .Close()
                End With
            Catch
                ' Rethrow any exceptions to calling procedure - just catching here so
                ' we can make sure lock gets released in finally clause
                Throw
            Finally
                ' Make sure writer lock gets released
                m_logFileLock.ReleaseWriterLock()
            End Try

        End Sub

        Private Sub m_logFileDataQueue_ProcessException(ByVal ex As System.Exception) Handles m_logFileDataQueue.ProcessException

            ' We just bubble up any exceptions thrown in process queue
            RaiseEvent LogException(ex)

        End Sub

    End Class

End Namespace