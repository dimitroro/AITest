using System;
using System.Collections.Generic;

namespace Matrix
{
    /// <summary>
    /// Outcome of a pathfinding query. Responsibility: carry success flag, path, and total cost.
    /// Collaborators: none. Lifetime: immutable value returned to caller.
    /// </summary>
    public sealed class PathResult
    {
        private static readonly PathResult _failureInstance = new PathResult(false, Array.Empty<MatrixIndex>(), 0f);

        /// <summary>True when a path from start to goal was found.</summary>
        public bool Success { get; }

        /// <summary>
        /// Cells from start to goal inclusive when <see cref="Success"/>; otherwise empty.
        /// Never null.
        /// </summary>
        public IReadOnlyList<MatrixIndex> Path { get; }

        /// <summary>
        /// Sum of step costs along <see cref="Path"/> when successful; otherwise 0.
        /// </summary>
        public float TotalCost { get; }

        private PathResult(bool success, IReadOnlyList<MatrixIndex> path, float totalCost)
        {
            Success = success;
            Path = path;
            TotalCost = totalCost;
        }

        /// <summary>Failed / unreachable result (empty path, cost 0).</summary>
        public static PathResult Failure() =>
            _failureInstance;

        /// <summary>
        /// Successful result.
        /// </summary>
        /// <param name="path">Start→goal inclusive. Must not be null or empty.</param>
        /// <param name="totalCost">Must be finite and &gt;= 0 (0 allowed for a single-cell path).</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalCost"/> is invalid.</exception>
        public static PathResult FromPath(IReadOnlyList<MatrixIndex> path, float totalCost)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (path.Count == 0)
                throw new ArgumentException("Path must not be empty.", nameof(path));
            if (float.IsNaN(totalCost) || float.IsInfinity(totalCost) || totalCost < 0f)
                throw new ArgumentOutOfRangeException(nameof(totalCost), totalCost, "Total cost must be finite and >= 0.");

            MatrixIndex[] copy = new MatrixIndex[path.Count];
            for (int i = 0; i < path.Count; i++)
                copy[i] = path[i];
            return new PathResult(true, copy, totalCost);
        }
    }
}
