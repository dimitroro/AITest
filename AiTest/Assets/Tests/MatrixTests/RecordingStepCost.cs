using System;
using System.Collections.Generic;

namespace Matrix.Tests
{
    /// <summary>Records GetCost calls; optionally overrides return value for invalid-cost tests.</summary>
    internal sealed class RecordingStepCost<T> : IStepCost<T>
    {
        private readonly IStepCost<T> _inner;
        private readonly float? _overrideCost;
        private readonly List<(MatrixIndex From, MatrixIndex To)> _calls =
            new List<(MatrixIndex From, MatrixIndex To)>();

        public RecordingStepCost(IStepCost<T> inner, float? overrideCost = null)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _overrideCost = overrideCost;
        }

        public IReadOnlyList<(MatrixIndex From, MatrixIndex To)> Calls =>
            _calls;

        public float GetCost(IReadOnlyMatrix<T> matrix, MatrixIndex from, MatrixIndex to)
        {
            _calls.Add((from, to));
            return _overrideCost ?? _inner.GetCost(matrix, from, to);
        }
    }
}
