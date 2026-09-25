namespace Matrix
{
    /// <summary>
    /// Mutable generic 2D matrix. Responsibility: mutate cells and produce reshaped copies.
    /// Collaborators: none required; neighbour queries use <see cref="INeighbourhood"/> via the read surface.
    /// Lifetime: owned by caller; not thread-safe. Module entry point for matrix consumers.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public interface IMatrix<T> : IReadOnlyMatrix<T>
    {
        /// <summary>
        /// Writes <paramref name="value"/> at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Cell coordinate.</param>
        /// <param name="value">Value to store.</param>
        /// <exception cref="MatrixOutOfBoundsException">Index outside the matrix.</exception>
        /// <remarks>Timing: O(1). Threading: not thread-safe.</remarks>
        public void Set(MatrixIndex index, T value);

        /// <summary>
        /// Tries to write <paramref name="value"/> at <paramref name="index"/> without throwing on out-of-bounds.
        /// </summary>
        /// <param name="index">Cell coordinate.</param>
        /// <param name="value">Value to store when in bounds.</param>
        /// <returns>True if written; false if out of bounds (matrix unchanged).</returns>
        /// <remarks>Timing: O(1). Threading: not thread-safe.</remarks>
        public bool TrySet(MatrixIndex index, T value);

        /// <summary>
        /// Sets every cell to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Fill value.</param>
        /// <remarks>Timing: O(Width×Height). Threading: not thread-safe.</remarks>
        public void Fill(T value);

        /// <summary>
        /// Sets every cell in <paramref name="region"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="region">Sub-rectangle that must lie entirely inside this matrix.</param>
        /// <param name="value">Fill value.</param>
        /// <exception cref="InvalidMatrixRegionException">Region empty, non-positive size, or outside this matrix.</exception>
        /// <remarks>Timing: O(region area). Threading: not thread-safe.</remarks>
        public void FillRegion(MatrixRegion region, T value);

        /// <summary>
        /// Creates an independent deep copy of this matrix (same size and cell values).
        /// Source unchanged.
        /// </summary>
        /// <returns>New matrix. Caller owns it.</returns>
        /// <remarks>Timing: O(Width×Height). Threading: not thread-safe.</remarks>
        public IMatrix<T> Clone();

        /// <summary>
        /// Creates a new matrix containing a copy of <paramref name="region"/>.
        /// Source matrix is unchanged.
        /// </summary>
        /// <param name="region">Sub-rectangle that must lie entirely inside this matrix.</param>
        /// <returns>New matrix of size region.Width × region.Height. Caller owns it.</returns>
        /// <exception cref="InvalidMatrixRegionException">Region empty, non-positive size, or outside this matrix.</exception>
        /// <remarks>Timing: O(region area). Threading: not thread-safe.</remarks>
        public IMatrix<T> Crop(MatrixRegion region);

        /// <summary>
        /// Creates a new matrix grown by <paramref name="padding"/> on each side.
        /// New cells are set to <paramref name="fillValue"/>. Source unchanged.
        /// </summary>
        /// <param name="padding">Non-negative cells to add per side.</param>
        /// <param name="fillValue">Value for newly created cells.</param>
        /// <returns>New matrix. Caller owns it.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Any padding component is negative.</exception>
        /// <remarks>Timing: O(new area). Threading: not thread-safe.</remarks>
        public IMatrix<T> Expand(ExpandPadding padding, T fillValue);

        /// <summary>
        /// Grows this matrix to exactly <paramref name="width"/> × <paramref name="height"/> (full-containment paste).
        /// Source unchanged. New cells (outside the pasted source) are <paramref name="fillValue"/>.
        /// </summary>
        /// <param name="width">Result column count; must be &gt;= this.Width.</param>
        /// <param name="height">Result row count; must be &gt;= this.Height.</param>
        /// <param name="contentOrigin">
        /// Where this matrix's (0,0) is placed in the result. Must be non-negative and leave room for the
        /// entire source: <c>contentOrigin.X &gt;= 0</c>, <c>contentOrigin.Y &gt;= 0</c>,
        /// <c>contentOrigin.X + Width &lt;= width</c>, <c>contentOrigin.Y + Height &lt;= height</c>.
        /// </param>
        /// <param name="fillValue">Value for cells not covered by the copied content.</param>
        /// <returns>New matrix. Caller owns it. Every source cell is copied exactly once (no clip, no discard).</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Width &lt; this.Width, height &lt; this.Height, contentOrigin negative, or source does not fit
        /// entirely inside the result at <paramref name="contentOrigin"/>.
        /// </exception>
        /// <remarks>
        /// Named rule: <b>full-containment grow</b>. Shrink/partial paste is out of scope — use <see cref="Crop"/> then Expand/ExpandTo.
        /// Timing: O(width×height). Threading: not thread-safe.
        /// </remarks>
        public IMatrix<T> ExpandTo(int width, int height, MatrixIndex contentOrigin, T fillValue);
    }
}
