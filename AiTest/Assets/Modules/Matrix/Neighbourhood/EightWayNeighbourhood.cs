using System;
using System.Collections.Generic;

namespace Matrix
{
    /// <summary>
    /// Eight-way neighbourhood: N, NE, E, SE, S, SW, W, NW.
    /// Responsibility: adjacency including diagonals (geometry only).
    /// Collaborators: none. Lifetime: stateless; safe to share. Thread-safe for concurrent reads.
    /// </summary>
    /// <remarks>
    /// <para><b>Diagonal corner-cutting rule:</b></para>
    /// <see cref="INeighbourhood"/> has no matrix or traversal access, so this type cannot suppress diagonals
    /// based on blocked orthogonal cells. It always returns every in-bounds neighbour among the eight directions.
    /// <para>
    /// Use shipped <see cref="NoCornerCuttingTraversalCondition{T}"/> around cell/edge rules to forbid cutting a corner
    /// through a blocked orthogonal cell. Omit that decorator if corner-cutting is allowed.
    /// </para>
    /// Pair with <see cref="OctileHeuristic"/>, <see cref="OctileStepCost{T}"/>, and
    /// <see cref="NoCornerCuttingTraversalCondition{T}"/> (wrap cell rules) for a complete 8-way stack.
    /// </remarks>
    public sealed class EightWayNeighbourhood : INeighbourhood
    {
        private const int MAX_DEGREE = 8;

        /// <inheritdoc />
        public IReadOnlyList<MatrixIndex> GetNeighbours(MatrixIndex center, int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than 0.");

            List<MatrixIndex> neighbours = new List<MatrixIndex>(MAX_DEGREE);
            TryAdd(neighbours, center.X, center.Y - 1, width, height);
            TryAdd(neighbours, center.X + 1, center.Y - 1, width, height);
            TryAdd(neighbours, center.X + 1, center.Y, width, height);
            TryAdd(neighbours, center.X + 1, center.Y + 1, width, height);
            TryAdd(neighbours, center.X, center.Y + 1, width, height);
            TryAdd(neighbours, center.X - 1, center.Y + 1, width, height);
            TryAdd(neighbours, center.X - 1, center.Y, width, height);
            TryAdd(neighbours, center.X - 1, center.Y - 1, width, height);
            return neighbours;
        }

        private static void TryAdd(List<MatrixIndex> neighbours, int x, int y, int width, int height)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;
            neighbours.Add(new MatrixIndex(x, y));
        }
    }
}
