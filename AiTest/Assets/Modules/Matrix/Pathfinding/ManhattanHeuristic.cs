namespace Matrix
{
    /// <summary>
    /// Manhattan distance |dx| + |dy|. Responsibility: admissible heuristic for orthogonal unit-cost grids.
    /// Collaborators: none. Lifetime: stateless; safe to share. Thread-safe for concurrent reads (no mutable state).
    /// </summary>
    public sealed class ManhattanHeuristic : IPathHeuristic
    {
        /// <inheritdoc />
        public float Estimate(MatrixIndex from, MatrixIndex to)
        {
            int dx = from.X - to.X;
            if (dx < 0)
                dx = -dx;
            int dy = from.Y - to.Y;
            if (dy < 0)
                dy = -dy;
            return dx + dy;
        }
    }
}
