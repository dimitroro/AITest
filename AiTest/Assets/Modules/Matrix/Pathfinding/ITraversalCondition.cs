namespace Matrix
{
    /// <summary>
    /// Decides whether cells and edges are walkable. Responsibility: gameplay/AI traversal rules.
    /// Collaborators: reads <see cref="IReadOnlyMatrix{T}"/>. Lifetime: owned by caller; may be per-unit or shared.
    /// </summary>
    /// <typeparam name="T">Matrix element type.</typeparam>
    /// <remarks>
    /// Invocation contract (mandatory for every <see cref="IPathfinder"/> implementation):
    /// put cell occupancy / terrain rules in <see cref="IsPassable"/>; put edge-only rules
    /// (doors, one-way, height) in <see cref="CanTraverse"/>. A neighbour is expanded only when
    /// both <c>IsPassable(to)</c> and <c>CanTraverse(from, to)</c> are true. See <see cref="IPathfinder.FindPath{T}"/>.
    /// </remarks>
    public interface ITraversalCondition<T>
    {
        /// <summary>
        /// Whether <paramref name="cell"/> may be entered / occupied on a path.
        /// </summary>
        /// <param name="matrix">Grid being searched. Must not be null.</param>
        /// <param name="cell">Candidate cell (caller ensures in-bounds).</param>
        /// <returns>True if the cell is passable.</returns>
        /// <remarks>
        /// Timing: expected O(1). Threading: not assumed thread-safe unless implementation documents otherwise.
        /// Pathfinders must call this for start, goal, and every neighbour before expanding it.
        /// </remarks>
        public bool IsPassable(IReadOnlyMatrix<T> matrix, MatrixIndex cell);

        /// <summary>
        /// Whether a step from <paramref name="from"/> to an adjacent <paramref name="to"/> is allowed
        /// (e.g. doors, one-way edges, height differences). Does not replace <see cref="IsPassable"/> for <paramref name="to"/>.
        /// </summary>
        /// <param name="matrix">Grid being searched. Must not be null.</param>
        /// <param name="from">Current cell (already known passable when called by a conforming pathfinder).</param>
        /// <param name="to">Neighbour cell.</param>
        /// <returns>True if the edge may be traversed.</returns>
        /// <remarks>
        /// Timing: expected O(1). Called only for neighbourhood-adjacent pairs after <c>IsPassable(to)</c> is true
        /// (or equivalently ANDed with it — pathfinder must not expand on CanTraverse alone).
        /// </remarks>
        public bool CanTraverse(IReadOnlyMatrix<T> matrix, MatrixIndex from, MatrixIndex to);
    }
}
