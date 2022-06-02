﻿// asmdef Version Defines, enabled when com.unity.addressables is imported.

#if UNITASK_ADDRESSABLE_SUPPORT

using Cysharp.Threading.Tasks.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Cysharp.Threading.Tasks
{
    public static class AddressableAsyncExtensions
    {
#region AsyncOperationHandle

        public static AsyncOperationHandleAwaiter GetAwaiter(this AsyncOperationHandle handle)
        {
            return new AsyncOperationHandleAwaiter(handle);
        }

        public static UniTask ToUniTask(this AsyncOperationHandle handle)
        {
            return new UniTask(AsyncOperationHandleConfiguredSource.Create(handle, PlayerLoopTiming.Update, null, CancellationToken.None, out var token), token);
        }

        public static UniTask ConfigureAwait(this AsyncOperationHandle handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellation = default(CancellationToken))
        {
            return new UniTask(AsyncOperationHandleConfiguredSource.Create(handle, timing, progress, cancellation, out var token), token);
        }

        public struct AsyncOperationHandleAwaiter : ICriticalNotifyCompletion
        {
            AsyncOperationHandle handle;
            Action<AsyncOperationHandle> continuationAction;

            public AsyncOperationHandleAwaiter(AsyncOperationHandle handle)
            {
                this.handle = handle;
                this.continuationAction = null;
            }

            public bool IsCompleted => handle.IsDone;

            public void GetResult()
            {
                if (continuationAction != null)
                {
                    handle.Completed -= continuationAction;
                    continuationAction = null;
                }

                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    var e = handle.OperationException;
                    handle = default;
                    ExceptionDispatchInfo.Capture(e).Throw();
                }

                var result = handle.Result;
                handle = default;
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperationHandle>(); // allocate delegate.
                handle.Completed += continuationAction;
            }
        }

        class AsyncOperationHandleConfiguredSource : IUniTaskSource, IPlayerLoopItem, IPromisePoolItem
        {
            static readonly PromisePool<AsyncOperationHandleConfiguredSource> pool = new PromisePool<AsyncOperationHandleConfiguredSource>();

            AsyncOperationHandle handle;
            IProgress<float> progress;
            CancellationToken cancellationToken;

            UniTaskCompletionSourceCore<AsyncUnit> core;

            AsyncOperationHandleConfiguredSource()
            {

            }

            public static IUniTaskSource Create(AsyncOperationHandle handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                var result = pool.TryRent() ?? new AsyncOperationHandleConfiguredSource();

                result.handle = handle;
                result.progress = progress;
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    TaskTracker.RemoveTracking(this);
                    core.GetResult(token);
                }
                finally
                {
                    pool.TryReturn(this);
                }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report(handle.PercentComplete);
                }

                if (handle.IsDone)
                {
                    if (handle.Status == AsyncOperationStatus.Failed)
                    {
                        core.TrySetException(handle.OperationException);
                    }
                    else
                    {
                        core.TrySetResult(AsyncUnit.Default);
                    }
                    return false;
                }

                return true;
            }

            public void Reset()
            {
                core.Reset();
                handle = default;
                progress = default;
                cancellationToken = default;
            }

            ~AsyncOperationHandleConfiguredSource()
            {
                if (pool.TryReturn(this))
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

#endregion

#region AsyncOperationHandle_T

        public static AsyncOperationHandleAwaiter<T> GetAwaiter<T>(this AsyncOperationHandle<T> handle)
        {
            return new AsyncOperationHandleAwaiter<T>(handle);
        }

        public static UniTask<T> ToUniTask<T>(this AsyncOperationHandle<T> handle)
        {
            return new UniTask<T>(AsyncOperationHandleConfiguredSource<T>.Create(handle, PlayerLoopTiming.Update, null, CancellationToken.None, out var token), token);
        }

        public static UniTask<T> ConfigureAwait<T>(this AsyncOperationHandle<T> handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellation = default(CancellationToken))
        {
            return new UniTask<T>(AsyncOperationHandleConfiguredSource<T>.Create(handle, timing, progress, cancellation, out var token), token);
        }

        public struct AsyncOperationHandleAwaiter<T> : ICriticalNotifyCompletion
        {
            AsyncOperationHandle<T> handle;
            Action<AsyncOperationHandle> continuationAction;

            public AsyncOperationHandleAwaiter(AsyncOperationHandle<T> handle)
            {
                this.handle = handle;
                this.continuationAction = null;
            }

            public bool IsCompleted => handle.IsDone;

            public T GetResult()
            {
                if (continuationAction != null)
                {
                    handle.CompletedTypeless -= continuationAction;
                    continuationAction = null;
                }

                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    var e = handle.OperationException;
                    handle = default;
                    ExceptionDispatchInfo.Capture(e).Throw();
                }

                var result = handle.Result;
                handle = default;
                return result;
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperationHandle>(); // allocate delegate.
                handle.CompletedTypeless += continuationAction;
            }
        }

        class AsyncOperationHandleConfiguredSource<T> : IUniTaskSource<T>, IPlayerLoopItem, IPromisePoolItem
        {
            static readonly PromisePool<AsyncOperationHandleConfiguredSource<T>> pool = new PromisePool<AsyncOperationHandleConfiguredSource<T>>();

            AsyncOperationHandle<T> handle;
            IProgress<float> progress;
            CancellationToken cancellationToken;

            UniTaskCompletionSourceCore<T> core;

            AsyncOperationHandleConfiguredSource()
            {

            }

            public static IUniTaskSource<T> Create(AsyncOperationHandle<T> handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<T>.CreateFromCanceled(cancellationToken, out token);
                }

                var result = pool.TryRent() ?? new AsyncOperationHandleConfiguredSource<T>();

                result.handle = handle;
                result.progress = progress;
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public T GetResult(short token)
            {
                try
                {
                    TaskTracker.RemoveTracking(this);

                    return core.GetResult(token);
                }
                finally
                {
                    pool.TryReturn(this);
                }
            }

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report(handle.PercentComplete);
                }

                if (handle.IsDone)
                {
                    if (handle.Status == AsyncOperationStatus.Failed)
                    {
                        core.TrySetException(handle.OperationException);
                    }
                    else
                    {
                        core.TrySetResult(handle.Result);
                    }
                    return false;
                }

                return true;
            }

            public void Reset()
            {
                core.Reset();
                handle = default;
                progress = default;
                cancellationToken = default;
            }

            ~AsyncOperationHandleConfiguredSource()
            {
                if (pool.TryReturn(this))
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

#endregion
    }
}

#endif