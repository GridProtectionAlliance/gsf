'*******************************************************************************************************
'  Tva.Collections.SynchronousProcessQueue.vb - Synchronized Processing Queue Class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/07/2006 - James R Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Threading

Namespace Collections

    ''' <summary>
    ''' <para>Processes a collection of items serially</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class SynchronousProcessQueue(Of T)

        Inherits IntervaledProcessQueueBase(Of T)

        Public Const DefaultProcessInterval As Integer = 100
        Public Const DefaultProcessTimeout As Integer = Timeout.Infinite
        Public Const DefaultRequeueOnTimeout As Boolean = False

        ''' <summary>
        ''' Create a process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature)

            Me.New(processItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Sub

        ''' <summary>
        ''' Create a process queue using the specified settings
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean)

            MyBase.New(processItemFunction, processInterval, 1, processTimeout, requeueOnTimeout)

        End Sub

        Public Shadows ReadOnly Property MaximumThreads() As Integer
            Get
                Return 1
            End Get
        End Property

    End Class

End Namespace