﻿
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Cysharp.Threading.Tasks.CompilerServices
{
    [StructLayout(LayoutKind.Auto)]
    public struct AsyncUniTaskMethodBuilder
    {
        // cache items.
        AutoResetUniTaskCompletionSource promise;
        IMoveNextRunner runner;

        // 1. Static Create method.
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncUniTaskMethodBuilder Create()
        {
            return default;
        }

        // 2. TaskLike Task property.
        public UniTask Task
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (promise != null)
                {
                    return promise.Task;
                }

                if (runner == null)
                {
                    return UniTask.CompletedTask;
                }

                promise = AutoResetUniTaskCompletionSource.Create();
                return promise.Task;
            }
        }

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            // runner is finished, return first.
            if (runner != null)
            {
                runner.Return();
                runner = null;
            }

            if (promise != null)
            {
                promise.TrySetException(exception);
            }
            else
            {
                promise = AutoResetUniTaskCompletionSource.CreateFromException(exception, out _);
            }
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult()
        {
            // runner is finished, return first.
            if (runner != null)
            {
                runner.Return();
                runner = null;
            }

            if (promise != null)
            {
                promise.TrySetResult();
            }
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (promise == null)
            {
                promise = AutoResetUniTaskCompletionSource.Create();
            }
            if (runner == null)
            {
                runner = MoveNextRunner<TStateMachine>.Create(ref stateMachine);
            }

            awaiter.OnCompleted(runner.CallMoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (promise == null)
            {
                promise = AutoResetUniTaskCompletionSource.Create();
            }
            if (runner == null)
            {
                runner = MoveNextRunner<TStateMachine>.Create(ref stateMachine);
            }

            awaiter.OnCompleted(runner.CallMoveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // don't use boxed stateMachine.
        }

#if DEBUG || !UNITY_2018_3_OR_NEWER
        // Important for IDE debugger.
        object debuggingId;
        private object ObjectIdForDebugger
        {
            get
            {
                if (debuggingId == null)
                {
                    debuggingId = new object();
                }
                return debuggingId;
            }
        }
#endif
    }

    [StructLayout(LayoutKind.Auto)]
    public struct AsyncUniTaskMethodBuilder<T>
    {
        // cache items.
        AutoResetUniTaskCompletionSource<T> promise;
        IMoveNextRunner runner;
        T result;

        // 1. Static Create method.
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncUniTaskMethodBuilder<T> Create()
        {
            return default;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public UniTask<T> Task
        {
            get
            {
                if (promise != null)
                {
                    return promise.Task;
                }

                if (runner == null)
                {
                    return UniTask.FromResult(result);
                }

                promise = AutoResetUniTaskCompletionSource<T>.Create();
                return promise.Task;
            }
        }

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            // runner is finished, return first.
            if (runner != null)
            {
                runner.Return();
                runner = null;
            }

            if (promise == null)
            {
                promise = AutoResetUniTaskCompletionSource<T>.CreateFromException(exception, out _);
            }
            else
            {
                promise.TrySetException(exception);
            }
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult(T result)
        {
            // runner is finished, return first.
            if (runner != null)
            {
                runner.Return();
                runner = null;
            }

            if (promise == null)
            {
                this.result = result;
                return;
            }

            promise.TrySetResult(result);
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (promise == null)
            {
                promise = AutoResetUniTaskCompletionSource<T>.Create();
            }
            if (runner == null)
            {
                runner = MoveNextRunner<TStateMachine>.Create(ref stateMachine);
            }

            awaiter.OnCompleted(runner.CallMoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (promise == null)
            {
                promise = AutoResetUniTaskCompletionSource<T>.Create();
            }
            if (runner == null)
            {
                runner = MoveNextRunner<TStateMachine>.Create(ref stateMachine);
            }

            awaiter.OnCompleted(runner.CallMoveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // don't use boxed stateMachine.
        }

#if DEBUG || !UNITY_2018_3_OR_NEWER
        // Important for IDE debugger.
        object debuggingId;
        private object ObjectIdForDebugger
        {
            get
            {
                if (debuggingId == null)
                {
                    debuggingId = new object();
                }
                return debuggingId;
            }
        }
#endif

    }
}