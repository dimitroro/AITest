using System;

namespace Matrix
{
    /// <summary>
    /// Cell coordinate in a 2D matrix. Responsibility: identify one cell by column/row.
    /// Collaborators: none. Lifetime: value type; owned by caller.
    /// X = column in [0, Width), Y = row in [0, Height); origin (0,0) is top-left.
    /// </summary>
    public readonly struct MatrixIndex : IEquatable<MatrixIndex>
    {
        private const int HASH_COMBINE = 397;

        /// <summary>Column index.</summary>
        public int X { get; }

        /// <summary>Row index.</summary>
        public int Y { get; }

        /// <summary>
        /// Creates an index. Does not validate against a matrix; bounds are checked by matrix APIs.
        /// </summary>
        /// <param name="x">Column.</param>
        /// <param name="y">Row.</param>
        public MatrixIndex(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <inheritdoc />
        public bool Equals(MatrixIndex other) =>
            X == other.X && Y == other.Y;

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is MatrixIndex other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * HASH_COMBINE) ^ Y;
            }
        }

        /// <summary>Value equality.</summary>
        public static bool operator ==(MatrixIndex left, MatrixIndex right) =>
            left.Equals(right);

        /// <summary>Value inequality.</summary>
        public static bool operator !=(MatrixIndex left, MatrixIndex right) =>
            !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() =>
            $"({X}, {Y})";
    }
}
