namespace Matrix
{
    /// <summary>
    /// Per-side growth amounts for <see cref="IMatrix{T}.Expand"/>.
    /// Responsibility: describe how many cells to add on each side.
    /// Collaborators: none. Lifetime: value type; owned by caller.
    /// All components must be &gt;= 0.
    /// </summary>
    public readonly struct ExpandPadding
    {
        /// <summary>Cells added to the left (decreasing X).</summary>
        public int Left { get; }

        /// <summary>Cells added to the right (increasing X).</summary>
        public int Right { get; }

        /// <summary>Cells added above (decreasing Y).</summary>
        public int Top { get; }

        /// <summary>Cells added below (increasing Y).</summary>
        public int Bottom { get; }

        /// <summary>
        /// Creates padding. Negative values are rejected by expand APIs.
        /// </summary>
        public ExpandPadding(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }
}
