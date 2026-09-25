using System;
using System.Collections.Generic;

namespace Matrix
{
    /// <summary>
    /// Adjacency rule for a grid. Responsibility: list neighbouring cell indices (bounds-aware).
    /// Collaborators: none. Lifetime: typically long-lived singleton-style instance owned by composition root.
    /// Does not apply walkability — only geometry. Blocked-cell and corner-cutting rules belong on
    /// <see cref="ITraversalCondition{T}"/> (see <see cref="EightWayNeighbourhood"/> remarks).
    /// </summary>
    public interface INeighbourhood
    {
        /// <summary>
        /// In-bounds neighbours of <paramref name="center"/> inside a matrix of the given size.
        /// </summary>
        /// <param name="center">Center cell. Need not be validated here; out-of-bounds center may yield empty or partial results — matrix APIs validate before calling.</param>
        /// <param name="width">Matrix width; must be &gt; 0.</param>
        /// <param name="height">Matrix height; must be &gt; 0.</param>
        /// <returns>New list of neighbour indices; never null. Order is implementation-defined but stable.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Width or height is &lt;= 0.</exception>
        /// <remarks>Timing: O(degree). Threading: implementations must be safe for concurrent read if immutable.</remarks>
        public IReadOnlyList<MatrixIndex> GetNeighbours(MatrixIndex center, int width, int height);
    }
}
