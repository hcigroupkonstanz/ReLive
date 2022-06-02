﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;

namespace Cysharp.Threading.Tasks
{
    public partial struct UniTask
    {
        public static UniTask WaitUntil(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new UniTask(WaitUntilPromise.Create(predicate, timing, cancellationToken, out var token), token);
        }

        public static UniTask WaitWhile(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new UniTask(WaitWhilePromise.Create(predicate, timing, cancellationToken, out var token), token);
        }

        public static UniTask<U> WaitUntilValueChanged<T, U>(T target, Func<T, U> monitorFunction, PlayerLoopTiming monitorTiming = PlayerLoopTiming.Update, IEqualityComparer<U> equalityComparer = null, CancellationToken cancellationToken = default(CancellationToken))
          where T : class
        {
            var unityObject = target as UnityEngine.Object;
            var isUnityObject = !object.ReferenceEquals(target, null); // don't use (unityObject == null)

            return new UniTask<U>(isUnityObject
                ? WaitUntilValueChangedUnityObjectPromise<T, U>.Create(target, monitorFunction, equalityComparer, monitorTiming, cancellationToken, out var token)
                : WaitUntilValueChangedStandardObjectPromise<T, U>.Create(target, monitorFunction, equalityComparer, monitorTiming, cancellationToken, out token), token);
        }

        sealed class WaitUntilPromise : IUniTaskSource, IPlayerLoopItem, IPromisePoolItem
        {
            static readonly PromisePool<WaitUntilPromise> pool = new PromisePool<WaitUntilPromise>();

            Func<bool> predicate;
            CancellationToken cancellationToken;

            UniTaskCompletionSourceCore<object> core;

            WaitUntilPromise()
            {
            }

            public static IUniTaskSource Create(Func<bool> predicate, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                var result = pool.TryRent() ?? new WaitUntilPromise();

                result.predicate = predicate;
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

                try
                {
                    if (!predicate())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            public void Reset()
            {
                core.Reset();
                predicate = default;
                cancellationToken = default;
            }

            ~WaitUntilPromise()
            {
                if (pool.TryReturn(this))
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

        sealed class WaitWhilePromise : IUniTaskSource, IPlayerLoopItem, IPromisePoolItem
        {
            static readonly PromisePool<WaitWhilePromise> pool = new PromisePool<WaitWhilePromise>();

            Func<bool> predicate;
            CancellationToken cancellationToken;

            UniTaskCompletionSourceCore<object> core;

            WaitWhilePromise()
            {
            }

            public static IUniTaskSource Create(Func<bool> predicate, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                var result = pool.TryRent() ?? new WaitWhilePromise();

                result.predicate = predicate;
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

                try
                {
                    if (predicate())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            public void Reset()
            {
                core.Reset();
                predicate = default;
                cancellationToken = default;
            }

            ~WaitWhilePromise()
            {
                if (pool.TryReturn(this))
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

        // where T : UnityEngine.Object, can not add constraint
        sealed class WaitUntilValueChangedUnityObjectPromise<T, U> : IUniTaskSource<U>, IPlayerLoopItem, IPromisePoolItem
        {
            static readonly PromisePool<WaitUntilValueChangedUnityObjectPromise<T, U>> pool = new PromisePool<WaitUntilValueChangedUnityObjectPromise<T, U>>();

            T target;
            U currentValue;
            Func<T, U> monitorFunction;
            IEqualityComparer<U> equalityComparer;
            CancellationToken cancellationToken;

            UniTaskCompletionSourceCore<U> core;

            WaitUntilValueChangedUnityObjectPromise()
            {
            }

            public static IUniTaskSource<U> Create(T target, Func<T, U> monitorFunction, IEqualityComparer<U> equalityComparer, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<U>.CreateFromCanceled(cancellationToken, out token);
                }

                var result = pool.TryRent() ?? new WaitUntilValueChangedUnityObjectPromise<T, U>();

                result.target = target;
                result.monitorFunction = monitorFunction;
                result.currentValue = monitorFunction(target);
                result.equalityComparer = equalityComparer ?? UnityEqualityComparer.GetDefault<U>();
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public U GetResult(short token)
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
                if (cancellationToken.IsCancellationRequested || target == null) // destroyed = cancel.
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                U nextValue = default(U);
                try
                {
                    nextValue = monitorFunction(target);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(nextValue);
                return false;
            }

            public void Reset()
            {
                core.Reset();
                target = default;
                currentValue = default;
                monitorFunction = default;
                equalityComparer = default;
                cancellationToken = default;
            }

            ~WaitUntilValueChangedUnityObjectPromise()
            {
                if (pool.TryReturn(this))
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }


        sealed class WaitUntilValueChangedStandardObjectPromise<T, U> : IUniTaskSource<U>, IPlayerLoopItem, IPromisePoolItem
            where T : class
        {
            static readonly PromisePool<WaitUntilValueChangedStandardObjectPromise<T, U>> pool = new PromisePool<WaitUntilValueChangedStandardObjectPromise<T, U>>();

            WeakReference<T> target;
            U currentValue;
            Func<T, U> monitorFunction;
            IEqualityComparer<U> equalityComparer;
            CancellationToken cancellationToken;

            UniTaskCompletionSourceCore<U> core;

            WaitUntilValueChangedStandardObjectPromise()
            {
            }

            public static IUniTaskSource<U> Create(T target, Func<T, U> monitorFunction, IEqualityComparer<U> equalityComparer, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<U>.CreateFromCanceled(cancellationToken, out token);
                }

                var result = pool.TryRent() ?? new WaitUntilValueChangedStandardObjectPromise<T, U>();

                result.target = new WeakReference<T>(target, false); // wrap in WeakReference.
                result.monitorFunction = monitorFunction;
                result.currentValue = monitorFunction(target);
                result.equalityComparer = equalityComparer ?? UnityEqualityComparer.GetDefault<U>();
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public U GetResult(short token)
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
                if (cancellationToken.IsCancellationRequested || !target.TryGetTarget(out var t)) // doesn't find = cancel.
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                U nextValue = default(U);
                try
                {
                    nextValue = monitorFunction(t);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(nextValue);
                return false;
            }

            public void Reset()
            {
                core.Reset();
                target = default;
                currentValue = default;
                monitorFunction = default;
                equalityComparer = default;
                cancellationToken = default;
            }

            ~WaitUntilValueChangedStandardObjectPromise()
            {
                if (pool.TryReturn(this))
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }
    }
}
