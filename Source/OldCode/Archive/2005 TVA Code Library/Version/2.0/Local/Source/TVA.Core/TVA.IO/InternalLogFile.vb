'*******************************************************************************************************
'  LogFile.vb - Log file implementation
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/10/2006 - J. Ritchie Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Threading
Imports System.Text
Imports System.IO
Imports TVA.Collections
Imports TVA.IO.FilePath

Namespace IO

    ' UPGRADE: A nice enhancement for this class would be to add a property to "limit" maximum file size with an option
    ' to either create a "scrolling status list" (i.e., pop old items off the top when adding new items to a file that
    ' has reached maximum size), or to be able to "roll-over" to a new file once maximum file size has been reached

    ''' <summary>This implements a simple multi-thread-happy log file class</summary>
    Public Class InternalLogFile

        Private m_logFileName As String
        Private m_logFileLock As ReaderWriterLock
        Private WithEvents m_logFileDataQueue As ProcessQueue(Of String)

        ''' <summary>Exception notification event</summary>
        ''' <param name="ex">Exception thrown during logging attempt</param>
        ''' <remarks>
        ''' We don't stop for exceptions in this class, but will expose them if the end user wishes
        ''' to know about any issues incurred while trying to log data
        ''' </remarks>
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

        ''' <summary>Add new log entry to the queue.</summary>
        ''' <param name="status">Message to add to the log</param>
        ''' <remarks>
        ''' <para>
        ''' Note that as soon as the item is added to the queue the function will return so that no
        ''' time is wasted on the calling thread.
        ''' </para>
        ''' <para>
        ''' Processing occurs on a set interval (the default is 100 milliseconds) - so any more log
        ''' entires added in this time will be processed as well.
        ''' </para>
        ''' </remarks>
        Public Sub Append(ByVal status As String)

            m_logFileDataQueue.Add(status)

        End Sub

        ''' <summary>Add new log entry to the queue.</summary>
        ''' <param name="status">Message to add to the log</param>
        ''' <remarks>A "newline" character will automatically be appended to the specified message</remarks>
        Public Sub AppendLine(ByVal status As String)

            Append(status & Environment.NewLine)

        End Sub

        ''' <summary>Add new log entry to the queue.</summary>
        ''' <param name="status">Message to add to the log</param>
        ''' <remarks>
        ''' <para>A timestamp will automatically be preprended to the specified message</para>
        ''' <para>A "newline" character will automatically be appended to the specified message</para>
        ''' </remarks>
        Public Sub AppendTimestampedLine(ByVal status As String)

            Append("[" & Date.Now & "] " & status & Environment.NewLine)

        End Sub

        ' UPGRADE: Add some ToString overloads that will take maximum length and relative file position (i.e., top, bottom, middle, etc.)
        ' as parameters

        ''' <summary>Reads entire log file into a string</summary>
        ''' <returns>Log file contents</returns>
        ''' <remarks>NOTE: This should only be called when the log file is known to be of reasonable size</remarks>
        Public Overrides Function ToString() As String

            Dim logData As String = ""

            ' Attempt to aquire a reader lock - we're only competing with writer locks as items are added
            ' to the log file which should happen rather quickly, but to be safe we'll wait for up to one
            ' second to allow plenty of time for logging
            m_logFileLock.AcquireReaderLock(1000)

            Try
                ' Read log contents into a string
                Dim fileName As String = AbsolutePath(m_logFileName)
                If File.Exists(fileName) Then
                    With File.OpenText(fileName)
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
                With File.AppendText(AbsolutePath(m_logFileName))
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