using System;

namespace Matrix
{
    /// <summary>
    /// Orthogonal cost 1, diagonal cost √2. Responsibility: step weights matching <see cref="OctileHeuristic"/>.
    /// Collaborators: none. Lifetime: stateless; safe to share. Thread-safe for concurrent reads.
    /// Pair with <see cref="EightWayNeighbourhood"/> + <see cref="OctileHeuristic"/>. Do not use with 4-way
    /// (use <see cref="ConstantStepCost{T}"/> with cost 1 instead).
    /// </summary>
    /// <typeparam name="T">Matrix element type (unused for cost; kept for interface match).</typeparam>
    /// <remarks>
    /// A step is diagonal when |dx| == 1 and |dy| == 1; otherwise orthogonal (|dx| + |dy| == 1).
    /// Precondition for callers: from/to are neighbourhood-adjacent;
    /// non-adjacent pairs are not in contract (pathfinder never asks).
    /// </remarks>
    public sealed class OctileStepCost<T> : IStepCost<T>
    {
        private const float ORTHOGONAL_COST = 1f;
        private static readonly float _diagonalCost = (float)Math.Sqrt(2.0);

        /// <inheritdoc />
        public float GetCost(IReadOnlyMatrix<T> matrix, MatrixIndex from, MatrixIndex to)
        {
            int dx = from.X - to.X;
            if (dx < 0)
                dx = -dx;
            int dy = from.Y - to.Y;
            if (dy < 0)
                dy = -dy;

            if (dx == 1 && dy == 1)
                return _diagonalCost;
            return ORTHOGONAL_COST;
        }
    }
}
