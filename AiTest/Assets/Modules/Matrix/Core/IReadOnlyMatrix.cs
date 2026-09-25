using System;
using System.Collections.Generic;

namespace Matrix
{
    /// <summary>
    /// Read-only view of a generic 2D matrix. Responsibility: query size, cells, regions, and indices.
    /// Collaborators: <see cref="INeighbourhood"/> (neighbour listing only). Lifetime: owned by caller; not thread-safe.
    /// Module entry for read consumers (e.g. pathfinding).
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public interface IReadOnlyMatrix<T>
    {
        /// <summary>Number of columns; always &gt; 0 for a valid instance.</summary>
        public int Width { get; }

        /// <summary>Number of rows; always &gt; 0 for a valid instance.</summary>
        public int Height { get; }

        /// <summary>
        /// Whether <paramref name="index"/> lies inside [0, Width) × [0, Height).
        /// </summary>
        /// <param name="index">Cell to test.</param>
        /// <returns>True if in bounds.</returns>
        /// <remarks>Timing: O(1). Threading: not thread-safe.</remarks>
        public bool InBounds(MatrixIndex index);

        /// <summary>
        /// Element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Cell coordinate.</param>
        /// <returns>Stored value.</returns>
        /// <exception cref="MatrixOutOfBoundsException">Index outside the matrix.</exception>
        /// <remarks>Timing: O(1). Threading: not thread-safe.</remarks>
        public T Get(MatrixIndex index);

        /// <summary>
        /// Tries to read the element at <paramref name="index"/> without throwing on out-of-bounds.
        /// </summary>
        /// <param name="index">Cell coordinate.</param>
        /// <param name="value">Set to the stored value when in bounds; otherwise default.</param>
        /// <returns>True if in bounds; false otherwise.</returns>
        /// <remarks>Timing: O(1). Threading: not thread-safe.</remarks>
        public bool TryGet(MatrixIndex index, out T value);

        /// <summary>
        /// Snapshot of all elements in row <paramref name="y"/> (length = Width).
        /// </summary>
        /// <param name="y">Row index.</param>
        /// <returns>New read-only list; not a live view.</returns>
        /// <exception cref="MatrixOutOfBoundsException">Row outside [0, Height).</exception>
        /// <remarks>Timing: O(Width). Threading: not thread-safe.</remarks>
        public IReadOnlyList<T> GetRow(int y);

        /// <summary>
        /// Snapshot of all elements in column <paramref name="x"/> (length = Height).
        /// </summary>
        /// <param name="x">Column index.</param>
        /// <returns>New read-only list; not a live view.</returns>
        /// <exception cref="MatrixOutOfBoundsException">Column outside [0, Width).</exception>
        /// <remarks>Timing: O(Height). Threading: not thread-safe.</remarks>
        public IReadOnlyList<T> GetColumn(int x);

        /// <summary>
        /// Snapshot of elements in <paramref name="region"/> in row-major order.
        /// </summary>
        /// <param name="region">Sub-rectangle that must lie entirely inside the matrix.</param>
        /// <returns>New read-only list of length region.Width × region.Height.</returns>
        /// <exception cref="InvalidMatrixRegionException">Region empty, non-positive size, or outside the matrix.</exception>
        /// <remarks>Timing: O(region area). Threading: not thread-safe.</remarks>
        public IReadOnlyList<T> GetRegion(MatrixRegion region);

        /// <summary>
        /// Elements of in-bounds neighbours of <paramref name="index"/> per <paramref name="neighbourhood"/>.
        /// Order matches <see cref="INeighbourhood.GetNeighbours"/>.
        /// </summary>
        /// <param name="index">Center cell; must be in bounds.</param>
        /// <param name="neighbourhood">Adjacency rule. Must not be null.</param>
        /// <returns>New read-only list of neighbour <b>values</b> (may be empty). Does not return indices.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="neighbourhood"/> is null.</exception>
        /// <exception cref="MatrixOutOfBoundsException"><paramref name="index"/> is out of bounds.</exception>
        /// <remarks>
        /// Returns values only. Consumers that need neighbour indices must call
        /// <see cref="INeighbourhood.GetNeighbours"/> with the same neighbourhood and this matrix's Width/Height.
        /// Timing: O(degree). Threading: not thread-safe.
        /// </remarks>
        public IReadOnlyList<T> GetNeighbours(MatrixIndex index, INeighbourhood neighbourhood);

        /// <summary>
        /// Finds the first cell equal to <paramref name="value"/> (row-major scan) using
        /// <see cref="EqualityComparer{T}.Default"/>.
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <param name="index">Set to the first match when found.</param>
        /// <returns>True if at least one match exists.</returns>
        /// <remarks>Timing: O(Width×Height). Threading: not thread-safe.</remarks>
        public bool Find(T value, out MatrixIndex index);

        /// <summary>
        /// All indices whose element equals <paramref name="value"/> (row-major order).
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <returns>New list; empty if none. Never null.</returns>
        /// <remarks>Timing: O(Width×Height). Threading: not thread-safe.</remarks>
        public IReadOnlyList<MatrixIndex> FindAll(T value);

        /// <summary>
        /// All indices whose element satisfies <paramref name="predicate"/> (row-major order).
        /// </summary>
        /// <param name="predicate">Cell predicate. Must not be null.</param>
        /// <returns>New list; empty if none. Never null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
        /// <remarks>Timing: O(Width×Height). Threading: not thread-safe.</remarks>
        public IReadOnlyList<MatrixIndex> FindAll(Func<T, bool> predicate);

        /// <summary>
        /// Enumerates every cell as <see cref="MatrixCell{T}"/> in row-major order (Y ascending, then X ascending).
        /// </summary>
        /// <returns>Lazy or snapshot enumeration; never null. Order is stable.</returns>
        /// <remarks>
        /// Timing: O(1) to obtain enumerator; O(Width×Height) to fully consume.
        /// Mutation during enumeration: undefined (do not mutate). Threading: not thread-safe.
        /// </remarks>
        public IEnumerable<MatrixCell<T>> Enumerate();

        /// <summary>
        /// Number of cells whose value satisfies <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Cell predicate. Must not be null.</param>
        /// <returns>Count in [0, Width×Height].</returns>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
        /// <remarks>Timing: O(Width×Height). Threading: not thread-safe.</remarks>
        public int Count(Func<T, bool> predicate);

        /// <summary>
        /// Whether any cell satisfies <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Cell predicate. Must not be null.</param>
        /// <returns>True if at least one match.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
        /// <remarks>Timing: O(Width×Height) worst case; may short-circuit. Threading: not thread-safe.</remarks>
        public bool Any(Func<T, bool> predicate);

        /// <summary>
        /// Whether every cell satisfies <paramref name="predicate"/>. Vacuous true only if the matrix is non-empty
        /// and all cells match (matrices always have Width×Height &gt; 0).
        /// </summary>
        /// <param name="predicate">Cell predicate. Must not be null.</param>
        /// <returns>True if all cells match.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
        /// <remarks>Timing: O(Width×Height) worst case; may short-circuit. Threading: not thread-safe.</remarks>
        public bool All(Func<T, bool> predicate);
    }
}
