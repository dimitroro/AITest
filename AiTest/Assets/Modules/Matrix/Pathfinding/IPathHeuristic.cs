namespace Matrix
{
    /// <summary>
    /// Admissible heuristic for informed search. Responsibility: estimate remaining cost to the goal.
    /// Collaborators: paired with a neighbourhood/cost model (e.g. Manhattan with 4-way + unit cost).
    /// Lifetime: typically long-lived; owned by composition root.
    /// </summary>
    public interface IPathHeuristic
    {
        /// <summary>
        /// Estimated cost from <paramref name="from"/> to <paramref name="to"/>. Must be &gt;= 0 and finite.
        /// Should not overestimate when used with an admissible algorithm (A*).
        /// </summary>
        /// <param name="from">Current cell.</param>
        /// <param name="to">Goal cell.</param>
        /// <returns>Non-negative finite estimate.</returns>
        /// <remarks>Timing: O(1). Threading: pure function preferred.</remarks>
        public float Estimate(MatrixIndex from, MatrixIndex to);
    }
}
