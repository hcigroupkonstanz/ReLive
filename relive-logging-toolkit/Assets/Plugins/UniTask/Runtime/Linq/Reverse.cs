﻿using Cysharp.Threading.Tasks.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TSource> Reverse<TSource>(this IUniTaskAsyncEnumerable<TSource> source)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            return new Reverse<TSource>(source);
        }
    }

    internal sealed class Reverse<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public Reverse(IUniTaskAsyncEnumerable<TSource> source)
        {
            this.source = source;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(source, cancellationToken);
        }

        sealed class Enumerator : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            readonly IUniTaskAsyncEnumerable<TSource> source;
            CancellationToken cancellationToken;

            TSource[] array;
            int index;

            public Enumerator(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
            {
                this.source = source;
                this.cancellationToken = cancellationToken;
            }

            public TSource Current { get; private set; }

            // after consumed array, don't use await so allow async(not require UniTaskCompletionSourceCore).
            public async UniTask<bool> MoveNextAsync()
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (array == null)
                {
                    array = await source.ToArrayAsync(cancellationToken);
                    index = array.Length - 1;
                }

                if (index != -1)
                {
                    Current = array[index];
                    --index;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public UniTask DisposeAsync()
            {
                return default;
            }
        }
    }
}