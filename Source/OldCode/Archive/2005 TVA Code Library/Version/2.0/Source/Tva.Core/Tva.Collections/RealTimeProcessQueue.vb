'*******************************************************************************************************
'  Tva.Collections.RealTimeProcessQueue.vb - Real-time Processing Queue Class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/04/2006 - Pinal C Patel
'       Original version of source code generated
'  01/05/2006 - James R Carroll
'       Made process queue a generic collection
'  01/07/2006 - James R Carroll
'       Reworked threading architecture
'
'*******************************************************************************************************

Imports System.Threading

Namespace Collections

    ''' <summary>
    ''' <para>Processes a collection of items as fast as possible</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class RealTimeProcessQueue(Of T)

        Inherits ProcessQueueBase(Of T)

        '''' <summary>
        '''' Create a process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite
        '''' </summary>
        'Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature)

        '    Me.New(processItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout)

        'End Sub

        '''' <summary>
        '''' Create a process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite
        '''' </summary>
        'Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer)

        '    Me.New(processItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout)

        'End Sub

        '''' <summary>
        '''' Create a process queue using the specified settings
        '''' </summary>
        'Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal maximumThreads As Integer, ByVal processTimeout As Integer)

        '    MyBase.New(processItemFunction)
        '    m_maximumThreads = maximumThreads
        '    m_processTimeout = processTimeout
        '    m_processTimer = New System.Timers.Timer

        '    With m_processTimer
        '        .Interval = processInterval
        '        .AutoReset = True
        '        .Enabled = False
        '    End With

        'End Sub

        Public Overrides Sub Start()

        End Sub

        Public Overrides Sub [Stop]()

        End Sub

    End Class

End Namespace