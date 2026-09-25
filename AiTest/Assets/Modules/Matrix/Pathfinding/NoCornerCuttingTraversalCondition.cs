using System;

namespace Matrix
{
    /// <summary>
    /// Decorator that forbids diagonal corner-cutting through blocked orthogonal cells.
    /// Responsibility: edge guard for 8-way; cell rules stay on the inner condition.
    /// Collaborators: inner <see cref="ITraversalCondition{T}"/>. Lifetime: owned by caller; not thread-safe unless inner is.
    /// </summary>
    /// <typeparam name="T">Matrix element type.</typeparam>
    /// <remarks>
    /// <para>
    /// <see cref="IsPassable"/> delegates to <c>inner</c>.
    /// <see cref="CanTraverse"/>: for an orthogonal step, returns <c>inner.CanTraverse</c>;
    /// for a diagonal step (dx, dy both ±1), returns false if either orthogonal sharing cell
    /// <c>from+(dx,0)</c> or <c>from+(0,dy)</c> fails <c>inner.IsPassable</c>, otherwise returns <c>inner.CanTraverse</c>.
    /// </para>
    /// Compose at the composition root, e.g.
    /// <c>new NoCornerCuttingTraversalCondition&lt;T&gt;(new PredicateTraversalCondition&lt;T&gt;(cellRules, alwaysTrueEdge))</c>
    /// with <see cref="EightWayNeighbourhood"/>, <see cref="OctileHeuristic"/>, and <see cref="OctileStepCost{T}"/>.
    /// </remarks>
    public sealed class NoCornerCuttingTraversalCondition<T> : ITraversalCondition<T>
    {
        private readonly ITraversalCondition<T> _inner;

        /// <summary>
        /// Wraps <paramref name="inner"/> with the diagonal corner-cut guard.
        /// </summary>
        /// <param name="inner">Cell/edge rules to decorate. Must not be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="inner"/> is null.</exception>
        public NoCornerCuttingTraversalCondition(ITraversalCondition<T> inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            _inner = inner;
        }

        /// <inheritdoc />
        public bool IsPassable(IReadOnlyMatrix<T> matrix, MatrixIndex cell) =>
            _inner.IsPassable(matrix, cell);

        /// <inheritdoc />
        public bool CanTraverse(IReadOnlyMatrix<T> matrix, MatrixIndex from, MatrixIndex to)
        {
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;
            int absDx = dx < 0 ? -dx : dx;
            int absDy = dy < 0 ? -dy : dy;

            if (absDx == 1 && absDy == 1)
            {
                MatrixIndex horizontalShare = new MatrixIndex(from.X + dx, from.Y);
                MatrixIndex verticalShare = new MatrixIndex(from.X, from.Y + dy);
                if (!_inner.IsPassable(matrix, horizontalShare))
                    return false;
                if (!_inner.IsPassable(matrix, verticalShare))
                    return false;
            }

            return _inner.CanTraverse(matrix, from, to);
        }
    }
}
