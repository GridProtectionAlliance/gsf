//******************************************************************************************************
//  UIThread.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/14/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

// Generally you should only apply this class to a single game object (e.g., Main Camera),
// multiple instances would simply compete to process queued method calls
// ReSharper disable once CheckNamespace
public class UIThread : MonoBehaviour
{
    #region [ Methods ]

    // Execute any queued methods on UI thread...
    protected void FixedUpdate()
    {
        Tuple<Action<object[]>, object[], ManualResetEventSlim> methodCall;
        Action<object[]> method;
        ManualResetEventSlim resetEvent;
        object[] args;

        while (m_methodCalls.TryDequeue(out methodCall))
        {
            method = methodCall.Item1;
            args = methodCall.Item2;
            resetEvent = methodCall.Item3;

            method(args);
            resetEvent.Set();
        }
    }

    #endregion

    #region [ Static ]

    // Static Fields

    // Queue of methods and parameters
    private static readonly ConcurrentQueue<Tuple<Action<object[]>, object[], ManualResetEventSlim>> m_methodCalls;

    // Static Constructor
    static UIThread()
    {
        m_methodCalls = new ConcurrentQueue<Tuple<Action<object[]>, object[], ManualResetEventSlim>>();
    }

    // Static Methods

    /// <summary>
    /// Invokes the specified method on the main UI thread.
    /// </summary>
    /// <param name="method">Delegate of method to invoke on main thread.</param>
    /// <returns>WaitHandle that can be used to wait for queued method to execute.</returns>
    public static WaitHandle Invoke(Action<object[]> method)
    {
        ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

        m_methodCalls.Enqueue(new Tuple<Action<object[]>, object[], ManualResetEventSlim>(method, null, resetEvent));

        return resetEvent.WaitHandle;
    }

    /// <summary>
    /// Invokes the specified method on the main UI thread.
    /// </summary>
    /// <param name="method">Delegate of method to invoke on main thread.</param>
    /// <param name="args">Method parameters, if any.</param>
    /// <returns>WaitHandle that can be used to wait for queued method to execute.</returns>
    public static WaitHandle Invoke(Action<object[]> method, params object[] args)
    {
        ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

        m_methodCalls.Enqueue(new Tuple<Action<object[]>, object[], ManualResetEventSlim>(method, args, resetEvent));

        return resetEvent.WaitHandle;
    }

    #endregion
}