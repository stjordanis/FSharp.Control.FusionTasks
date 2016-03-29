﻿/////////////////////////////////////////////////////////////////////////////////////////////////
//
// FSharp.Control.FusionTasks - F# Async computation <--> .NET Task easy seamless interoperability library.
// Copyright (c) 2016 Kouji Matsui (@kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Control.FusionTasksTests
{
    [TestClass]
    public class TaskExtensionsTests
    {
        #region Task.AsAsync
        private static async Task DelayAsync()
        {
            await Task.Delay(500);
            Console.WriteLine("AAA");
        }

        [TestMethod]
        public void TaskAsAsyncTest()
        {
            var task = DelayAsync();
            var asy = task.AsAsync();

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
        }

#if PCL7 || PCL47
#else
        [TestMethod]
        public void ConfiguredTaskAwaiterAsAsyncTest()
        {
            var task = DelayAsync().ConfigureAwait(false);
            var asy = task.AsAsync();

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
        }
#endif

        private static async Task<int> DelayAndReturnAsync()
        {
            await Task.Delay(500);
            return 123;
        }

        [TestMethod]
        public void TaskTAsAsyncTest()
        {
            var task = DelayAndReturnAsync();
            var asy = task.AsAsync();

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
        }

#if PCL7 || PCL47
#else
        [TestMethod]
        public void ConfiguredTaskAwaiterTAsAsyncTest()
        {
            var task = DelayAndReturnAsync().ConfigureAwait(false);
            var asy = task.AsAsync();

            // MSTest not supported FSharpAsync based tests, so run synchronously here. 
            FSharpAsync.RunSynchronously(asy, FSharpOption<int>.None, FSharpOption<CancellationToken>.None);
        }
#endif
        #endregion

        #region FSharpAsync<'T>.AsTask
        [TestMethod]
        public async Task AsyncAsTaskTestAsync()
        {
            var asy = FSharpAsync.Sleep(500);
            await asy.AsTask();
        }

        [TestMethod]
        public async Task AsyncTAsTaskTestAsync()
        {
            // C# cannot create FSharpAsync<'T>, so create C#'ed Task<T> and convert to FSharpAsync<'T>.
            var task = DelayAndReturnAsync();
            var asy = task.AsAsync();
            var result = await asy.AsTask();

            Assert.AreEqual(123, result);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task AsyncAsTaskWithCancellationTestAsync()
        {
            var cts = new CancellationTokenSource();

            var asy = FSharpAsync.Sleep(500);
            var outerTask = asy.AsTask(cts.Token);

            // Force hard wait.
            Thread.Sleep(100);

            // Task cancelled.
            cts.Cancel();

            // Continuation point. (Will raise exception)
            await outerTask;

            Assert.Fail();
        }

        private static async Task<int> DelayAndReturnAsync(CancellationToken token)
        {
            await Task.Delay(500, token); /* explicitly */
            return 123;
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task AsyncTAsTaskWithCancellationTestAsync()
        {
            // Information:
            //   F#'s CancellationToken is managing contextual token in async computation expression.
            //   But .NET CancellationToken is no contextual/default token, in C# too.
            //   So no automatic implicit derive tokens, token just explicit set to any async operation:
            //     Task.Delay(int, token) <-- DelayAndReturnAsync(token) <-- cts.Token

            var cts = new CancellationTokenSource();

            // C# cannot create FSharpAsync<'T>, so create C#'ed Task<T> and convert to FSharpAsync<'T>.
            var task = (Task)DelayAndReturnAsync(/* explicitly */ cts.Token);
            var asy = task.AsAsync();
            var outerTask = asy.AsTask(cts.Token); /* explicitly, but not derived into DelayAndReturnAsync() */

            // Force hard wait.
            Thread.Sleep(100);

            // Task cancelled.
            cts.Cancel();

            // Continuation point. (Will raise exception)
            await outerTask;

            Assert.Fail();
        }
        #endregion

        #region FSharpAsync<'T>.GetAwaiter
        [TestMethod]
        public async Task AsyncGetAwaiterTestAsync()
        {
            var asy = FSharpAsync.Sleep(500);
            await asy;
        }

        [TestMethod]
        public async Task AsyncTGetAwaiterTestAsync()
        {
            // C# cannot create FSharpAsync<'T>, so create C#'ed Task<T> and convert to FSharpAsync<'T>.
            var task = DelayAndReturnAsync();
            var asy = task.AsAsync();
            var result = await asy.AsTask();

            Assert.AreEqual(123, result);
        }
        #endregion
    }
}