using System;

namespace Matrix
{
    /// <summary>
    /// Traversal rules backed by caller predicates. Responsibility: adapt Func-based gameplay rules to the seam.
    /// Collaborators: none beyond the predicates. Lifetime: owned by caller.
    /// Put cell/terrain rules in the isPassable predicate and edge-only rules in canTraverse;
    /// pathfinders require both (see <see cref="IPathfinder.FindPath{T}"/>).
    /// </summary>
    /// <typeparam name="T">Matrix element type.</typeparam>
    public sealed class PredicateTraversalCondition<T> : ITraversalCondition<T>
    {
        private readonly Func<IReadOnlyMatrix<T>, MatrixIndex, bool> _isPassable;
        private readonly Func<IReadOnlyMatrix<T>, MatrixIndex, MatrixIndex, bool> _canTraverse;

        /// <summary>
        /// Creates a condition from cell and edge predicates.
        /// </summary>
        /// <param name="isPassable">Cell passability (invoked for start, goal, and every neighbour). Must not be null.</param>
        /// <param name="canTraverse">Edge passability (invoked only together with a passable neighbour). Must not be null.</param>
        /// <exception cref="ArgumentNullException">Any predicate is null.</exception>
        public PredicateTraversalCondition(
            Func<IReadOnlyMatrix<T>, MatrixIndex, bool> isPassable,
            Func<IReadOnlyMatrix<T>, MatrixIndex, MatrixIndex, bool> canTraverse)
        {
            if (isPassable == null)
                throw new ArgumentNullException(nameof(isPassable));
            if (canTraverse == null)
                throw new ArgumentNullException(nameof(canTraverse));
            _isPassable = isPassable;
            _canTraverse = canTraverse;
        }

        /// <inheritdoc />
        public bool IsPassable(IReadOnlyMatrix<T> matrix, MatrixIndex cell) =>
            _isPassable(matrix, cell);

        /// <inheritdoc />
        public bool CanTraverse(IReadOnlyMatrix<T> matrix, MatrixIndex from, MatrixIndex to) =>
            _canTraverse(matrix, from, to);
    }
}
