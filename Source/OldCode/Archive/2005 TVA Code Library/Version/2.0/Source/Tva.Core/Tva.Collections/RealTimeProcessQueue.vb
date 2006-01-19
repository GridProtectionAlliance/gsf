'*******************************************************************************************************
'  Tva.Collections.RealTimeProcessList.vb - Real-time Processing List Class
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
    ''' <para>Processes a list of items as fast as possible</para>
    ''' </summary>
    ''' <typeparam name="T">Type of object to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed collection of objects to be processed.</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class RealTimeProcessList(Of T)

        Inherits ProcessListBase(Of T)

        ''' <summary>
        ''' Create a process queue using the specified settings
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature)

            MyBase.New(processItemFunction, New List(Of T))

        End Sub

        Public Overrides Sub Start()

            MyBase.Start()

        End Sub

        Public Overrides Sub [Stop]()

            MyBase.Stop()

        End Sub

    End Class

End Namespace