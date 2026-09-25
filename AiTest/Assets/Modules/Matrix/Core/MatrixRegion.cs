namespace Matrix
{
    /// <summary>
    /// Axis-aligned rectangular region of cells. Responsibility: describe a sub-rectangle.
    /// Collaborators: <see cref="MatrixIndex"/>. Lifetime: value type; owned by caller.
    /// Cells covered: X..X+Width-1, Y..Y+Height-1 (Width/Height must be &gt; 0 for a non-empty region).
    /// </summary>
    public readonly struct MatrixRegion
    {
        /// <summary>Left column of the region.</summary>
        public int X { get; }

        /// <summary>Top row of the region.</summary>
        public int Y { get; }

        /// <summary>Number of columns.</summary>
        public int Width { get; }

        /// <summary>Number of rows.</summary>
        public int Height { get; }

        /// <summary>
        /// Creates a region. Validity against a parent matrix is checked by matrix APIs.
        /// </summary>
        /// <param name="x">Left column.</param>
        /// <param name="y">Top row.</param>
        /// <param name="width">Column count; must be &gt; 0 when used with crop/get-region.</param>
        /// <param name="height">Row count; must be &gt; 0 when used with crop/get-region.</param>
        public MatrixRegion(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Whether <paramref name="index"/> lies inside this region (not necessarily inside a matrix).
        /// </summary>
        /// <param name="index">Cell to test.</param>
        /// <returns>True if inside the rectangle.</returns>
        public bool Contains(MatrixIndex index) =>
            index.X >= X
            && index.Y >= Y
            && index.X < X + Width
            && index.Y < Y + Height;
    }
}
