using System;

namespace Matrix
{
    /// <summary>
    /// Pathfinding entry point. Responsibility: find a route between two cells on a matrix.
    /// Collaborators: <see cref="IReadOnlyMatrix{T}"/>, <see cref="ITraversalCondition{T}"/>,
    /// <see cref="IStepCost{T}"/>; concrete impl also uses <see cref="INeighbourhood"/> and <see cref="IPathHeuristic"/>.
    /// Lifetime: owned by composition root; not thread-safe unless documented.
    /// </summary>
    public interface IPathfinder
    {
        /// <summary>
        /// Searches for a path from <paramref name="start"/> to <paramref name="goal"/>.
        /// </summary>
        /// <typeparam name="T">Matrix element type.</typeparam>
        /// <param name="matrix">Grid to search. Must not be null.</param>
        /// <param name="start">Start cell.</param>
        /// <param name="goal">Goal cell.</param>
        /// <param name="traversal">Passability rules. Must not be null.</param>
        /// <param name="stepCost">Edge weights. Must not be null.</param>
        /// <returns>
        /// <see cref="PathResult"/> with <c>Success</c> true and a start→goal path on success;
        /// <c>Success</c> false when unreachable or start/goal not passable (no exception).
        /// </returns>
        /// <exception cref="ArgumentNullException">Any reference argument is null.</exception>
        /// <exception cref="MatrixOutOfBoundsException">Start or goal is outside the matrix.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A step cost returned by <paramref name="stepCost"/> is not finite and &gt; 0.</exception>
        /// <remarks>
        /// <para><b>Traversal invocation rule (LSP — every implementation must follow):</b></para>
        /// <list type="number">
        /// <item>If start or goal is out of bounds → throw <see cref="MatrixOutOfBoundsException"/>.</item>
        /// <item>If <c>!traversal.IsPassable(matrix, start)</c> or <c>!traversal.IsPassable(matrix, goal)</c> → return <see cref="PathResult.Failure"/>.</item>
        /// <item>If start equals goal and step 2 passed → return Success with path <c>[start]</c> and <c>TotalCost == 0</c> (no search).</item>
        /// <item>When expanding from <c>from</c> to a neighbourhood neighbour <c>to</c>, expand only if
        /// <c>traversal.IsPassable(matrix, to)</c> <b>and</b> <c>traversal.CanTraverse(matrix, from, to)</c> are both true.
        /// Never expand on <c>CanTraverse</c> alone; never skip <c>IsPassable(to)</c>.</item>
        /// <item>Call <c>stepCost.GetCost</c> only for edges that pass step 4.</item>
        /// </list>
        /// Timing: depends on open area and algorithm (A*: typical O(E log V) with binary heap). Threading: not thread-safe.
        /// </remarks>
        public PathResult FindPath<T>(
            IReadOnlyMatrix<T> matrix,
            MatrixIndex start,
            MatrixIndex goal,
            ITraversalCondition<T> traversal,
            IStepCost<T> stepCost);
    }
}
