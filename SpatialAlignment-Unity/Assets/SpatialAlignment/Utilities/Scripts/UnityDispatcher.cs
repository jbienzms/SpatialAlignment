//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using UnityEngine;


#if WINDOWS_UWP
/// <summary>
/// A helper class for dispatching actions to run on various Unity threads.
/// </summary>
static public class UnityDispatcher
{
    /// <summary>
    /// Schedules the specified action to be run on Unity's main thread.
    /// </summary>
    /// <param name="action">
    /// The action to run.
    /// </param>
    static public void InvokeOnAppThread(Action action)
    {
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            // Already on app thread, just run inline
            action();
        }
        else
        {
            // Schedule
            UnityEngine.WSA.Application.InvokeOnAppThread(() => action(), false);
        }
    }
}
#endif

#if !WINDOWS_UWP
/// <summary>
/// A helper class for dispatching actions to run on various Unity threads.
/// </summary>
public class UnityDispatcher : MonoBehaviour
{
    #region Member Variables
    static private UnityDispatcher instance;
    static private Queue<Action> queue = new Queue<Action>(8);
    static private volatile bool queued = false;
    #endregion // Member Variables

    #region Internal Methods
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static private void Initialize()
    {
        if (instance == null)
        {
            instance = new GameObject("Dispatcher").AddComponent<UnityDispatcher>();
            DontDestroyOnLoad(instance.gameObject);
        }
    }
    #endregion // Internal Methods

    #region Unity Overrides
    protected virtual void Update()
    {
        // Action placeholder
        Action action = null;

        // Do this as long as there's something in the queue
        while (queued)
        {
            // Lock only long enough to take an item
            lock (queue)
            {
                // Get the next action
                action = queue.Dequeue();

                // Have we exhausted the queue?
                if (queue.Count == 0) { queued = false; }
            }

            // Execute the action outside of the lock
            action();
        }
    }
    #endregion // Unity Overrides

    #region Public Methods
    /// <summary>
    /// Schedules the specified action to be run on Unity's main thread.
    /// </summary>
    /// <param name="action">
    /// The action to run.
    /// </param>
    static public void InvokeOnAppThread(Action action)
    {
        // Validate
        if (action == null) throw new ArgumentNullException(nameof(action));

        // Lock to be thread-safe
        lock (queue)
        {
            // Add the action
            queue.Enqueue(action);

            // Action is in the queue
            queued = true;
        }
    }
    #endregion // Public Methods
}
#endif