using System;

namespace Matrix
{
    /// <summary>
    /// Octile distance: D·max(dx,dy) + (D2−D)·min(dx,dy) with D = 1 and D2 = √2.
    /// Responsibility: admissible heuristic for 8-way grids with unit orthogonal and √2 diagonal step costs.
    /// Collaborators: none. Lifetime: stateless; safe to share. Thread-safe for concurrent reads.
    /// Pair with <see cref="EightWayNeighbourhood"/>, <see cref="OctileStepCost{T}"/>, and typically
    /// <see cref="NoCornerCuttingTraversalCondition{T}"/>. Do not pair with <see cref="OrthogonalNeighbourhood"/>
    /// (use <see cref="ManhattanHeuristic"/> there).
    /// </summary>
    public sealed class OctileHeuristic : IPathHeuristic
    {
        private const float ORTHOGONAL_COST = 1f;
        private static readonly float _diagonalCost = (float)Math.Sqrt(2.0);
        private static readonly float _diagonalExtra = _diagonalCost - ORTHOGONAL_COST;

        /// <inheritdoc />
        public float Estimate(MatrixIndex from, MatrixIndex to)
        {
            int dx = from.X - to.X;
            if (dx < 0)
                dx = -dx;
            int dy = from.Y - to.Y;
            if (dy < 0)
                dy = -dy;

            int max = dx > dy ? dx : dy;
            int min = dx < dy ? dx : dy;
            return ORTHOGONAL_COST * max + _diagonalExtra * min;
        }
    }
}
