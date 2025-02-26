﻿using System.Threading;

namespace Cysharp.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<T> Empty<T>()
        {
            return Cysharp.Threading.Tasks.Linq.Empty<T>.Instance;
        }
    }

    internal class Empty<T> : IUniTaskAsyncEnumerable<T>
    {
        public static readonly IUniTaskAsyncEnumerable<T> Instance = new Empty<T>();

        Empty()
        {
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return Enumerator.Instance;
        }

        class Enumerator : IUniTaskAsyncEnumerator<T>
        {
            public static readonly IUniTaskAsyncEnumerator<T> Instance = new Enumerator();

            Enumerator()
            {
            }

            public T Current => default;

            public UniTask<bool> MoveNextAsync()
            {
                return CompletedTasks.False;
            }

            public UniTask DisposeAsync()
            {
                return default;
            }
        }
    }
}