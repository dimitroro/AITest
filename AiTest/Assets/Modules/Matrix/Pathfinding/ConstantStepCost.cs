using System;

namespace Matrix
{
    /// <summary>
    /// Uniform positive step cost. Responsibility: constant weight for every edge.
    /// Collaborators: none. Lifetime: owned by caller; safe to share if immutable.
    /// Pair with <see cref="OrthogonalNeighbourhood"/> + <see cref="ManhattanHeuristic"/> (typically cost 1).
    /// Do not pair with <see cref="OctileHeuristic"/> on 8-way (use <see cref="OctileStepCost{T}"/>).
    /// </summary>
    /// <typeparam name="T">Matrix element type (unused for cost; kept for interface match).</typeparam>
    public sealed class ConstantStepCost<T> : IStepCost<T>
    {
        private readonly float _cost;

        /// <summary>
        /// Creates a constant cost provider.
        /// </summary>
        /// <param name="cost">Must be finite and &gt; 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">Cost is not finite or not &gt; 0.</exception>
        public ConstantStepCost(float cost)
        {
            if (float.IsNaN(cost) || float.IsInfinity(cost) || cost <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cost), cost, "Cost must be finite and > 0.");
            _cost = cost;
        }

        /// <inheritdoc />
        public float GetCost(IReadOnlyMatrix<T> matrix, MatrixIndex from, MatrixIndex to) =>
            _cost;
    }
}
