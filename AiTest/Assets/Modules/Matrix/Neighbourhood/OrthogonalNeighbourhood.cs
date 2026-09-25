using System;
using System.Collections.Generic;

namespace Matrix
{
    /// <summary>
    /// Four-way (orthogonal) neighbourhood: N, E, S, W. Responsibility: adjacency without diagonals.
    /// Collaborators: none. Lifetime: stateless; safe to share. Thread-safe for concurrent reads (no mutable state).
    /// </summary>
    public sealed class OrthogonalNeighbourhood : INeighbourhood
    {
        private const int MAX_DEGREE = 4;

        /// <inheritdoc />
        public IReadOnlyList<MatrixIndex> GetNeighbours(MatrixIndex center, int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than 0.");

            List<MatrixIndex> neighbours = new List<MatrixIndex>(MAX_DEGREE);
            TryAdd(neighbours, center.X, center.Y - 1, width, height);
            TryAdd(neighbours, center.X + 1, center.Y, width, height);
            TryAdd(neighbours, center.X, center.Y + 1, width, height);
            TryAdd(neighbours, center.X - 1, center.Y, width, height);
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
