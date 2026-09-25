using System;

namespace Matrix
{
    /// <summary>
    /// Thrown when a region is empty, has non-positive size, or does not fit inside the parent matrix.
    /// Responsibility: signal invalid crop/region arguments. Lifetime: exception instance.
    /// </summary>
    public sealed class InvalidMatrixRegionException : Exception
    {
        /// <summary>Creates the exception with a descriptive message.</summary>
        public InvalidMatrixRegionException(string message)
            : base(message)
        {
        }
    }
}
