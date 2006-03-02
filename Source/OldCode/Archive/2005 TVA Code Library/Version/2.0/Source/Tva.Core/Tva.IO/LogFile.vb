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

    ''' <summary>This implements a simple multi-thread-happy log file class</summary>
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

            ' We implement a synchronized process queue for log entries such that all entries will
            ' be processed in order.  Additionally, we use a "many-at-once" function signature so
            ' that we will get all current items to be processed at each processing interval
            m_logFileDataQueue = ProcessQueue(Of String).CreateSynchronousQueue(AddressOf ProcessLogEntries)

            ' We'll requeue data if we fail to aquire writer lock or fail to write data into file...
            m_logFileDataQueue.RequeueOnException = True
            m_logFileDataQueue.Start()

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

            ' Add new log entry to the queue.  Note that as soon as the item is added to the queue
            ' the function will return so that no time is wasted on the calling thread.  The process
            ' queue will automatically enable its processing threads when it sees there is new data
            ' in the queue to be processed.  Processing occurs on a set interval (the default is 100
            ' milliseconds) - so any more log entires added in this time will be processed as well.
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

            ' Attempt to aquire a reader lock - we're only competing with writer locks as items are added
            ' to the log file which should happen rather quickly, but to be safe we'll wait for up to one
            ' second to allow plenty of time for logging
            m_logFileLock.AcquireReaderLock(1000)

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

        ' We process all available items in the queue - note that this function processes as "array" of
        ' items which creates a "many-at-once" processing queue
        Private Sub ProcessLogEntries(ByVal items As String())

            ' Attempt to aquire a writer lock - the only thing we will be competing with is the ToString
            ' method which reads the contents of the log file.  Since data will automatically be requeued
            ' if we can't aquire a reader lock (i.e., writer lock aquisition will time out and throw an
            ' exception and the process queue is setup to requeue data on exception), we don't wait long
            m_logFileLock.AcquireWriterLock(100)

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
                ' we can make sure lock gets released in finally clause.  Remember
                ' exceptions thrown by this function cause queue items to be requeued
                ' in their original location because RequeueOnException = True.  So
                ' failure to process items here means they'll get another chance.
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