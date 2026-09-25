using System;

namespace Matrix
{
    /// <summary>
    /// Thrown when a cell index lies outside the matrix bounds.
    /// Responsibility: signal invalid cell access. Lifetime: exception instance.
    /// </summary>
    public sealed class MatrixOutOfBoundsException : Exception
    {
        /// <summary>Index that was out of bounds, if applicable.</summary>
        public MatrixIndex Index { get; }

        /// <summary>Creates the exception for the given index.</summary>
        public MatrixOutOfBoundsException(MatrixIndex index)
            : base($"Matrix index {index} is out of bounds.")
        {
            Index = index;
        }

        /// <summary>Creates the exception with a custom message.</summary>
        public MatrixOutOfBoundsException(string message)
            : base(message)
        {
        }
    }
}
