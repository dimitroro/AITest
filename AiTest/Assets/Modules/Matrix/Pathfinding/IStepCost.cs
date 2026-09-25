namespace Matrix
{
    /// <summary>
    /// Cost of moving between adjacent cells. Responsibility: supply non-uniform or uniform step weights.
    /// Collaborators: reads <see cref="IReadOnlyMatrix{T}"/>. Lifetime: owned by caller.
    /// </summary>
    /// <typeparam name="T">Matrix element type.</typeparam>
    public interface IStepCost<T>
    {
        /// <summary>
        /// Cost to step from <paramref name="from"/> to adjacent <paramref name="to"/>.
        /// Must return a finite value strictly greater than 0.
        /// </summary>
        /// <param name="matrix">Grid being searched. Must not be null.</param>
        /// <param name="from">Current cell.</param>
        /// <param name="to">Neighbour cell.</param>
        /// <returns>Positive finite cost.</returns>
        /// <remarks>Timing: expected O(1). Pathfinder throws <see cref="System.ArgumentOutOfRangeException"/> if the value is not finite and &gt; 0.</remarks>
        public float GetCost(IReadOnlyMatrix<T> matrix, MatrixIndex from, MatrixIndex to);
    }
}
