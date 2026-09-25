namespace Matrix
{
    /// <summary>
    /// One cell in an enumeration. Responsibility: carry index + value together.
    /// Collaborators: none. Lifetime: value type; owned by caller / enumerator.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public readonly struct MatrixCell<T>
    {
        /// <summary>Cell coordinate.</summary>
        public MatrixIndex Index { get; }

        /// <summary>Value at <see cref="Index"/>.</summary>
        public T Value { get; }

        /// <summary>Creates a cell entry.</summary>
        /// <param name="index">Coordinate.</param>
        /// <param name="value">Stored value.</param>
        public MatrixCell(MatrixIndex index, T value)
        {
            Index = index;
            Value = value;
        }
    }
}
