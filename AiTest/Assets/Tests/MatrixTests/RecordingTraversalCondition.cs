using System;
using System.Collections.Generic;

namespace Matrix.Tests
{
    /// <summary>Records IsPassable / CanTraverse call order for FindPath invocation-rule checks.</summary>
    internal sealed class RecordingTraversalCondition<T> : ITraversalCondition<T>
    {
        private readonly ITraversalCondition<T> _inner;
        private readonly List<string> _calls = new List<string>();

        public RecordingTraversalCondition(ITraversalCondition<T> inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public IReadOnlyList<string> Calls =>
            _calls;

        public bool IsPassable(IReadOnlyMatrix<T> matrix, MatrixIndex cell)
        {
            _calls.Add($"IsPassable:{cell}");
            return _inner.IsPassable(matrix, cell);
        }

        public bool CanTraverse(IReadOnlyMatrix<T> matrix, MatrixIndex from, MatrixIndex to)
        {
            _calls.Add($"CanTraverse:{from}->{to}");
            return _inner.CanTraverse(matrix, from, to);
        }
    }
}
