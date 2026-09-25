using System;
using System.Collections.Generic;

namespace Matrix
{
    /// <summary>
    /// A* pathfinder. Responsibility: informed search on a grid using injected neighbourhood and heuristic.
    /// Collaborators: <see cref="INeighbourhood"/>, <see cref="IPathHeuristic"/> (ctor);
    /// per-call <see cref="IReadOnlyMatrix{T}"/>, <see cref="ITraversalCondition{T}"/>, <see cref="IStepCost{T}"/>.
    /// Lifetime: owned by composition root; not thread-safe.
    /// </summary>
    public sealed class AStarPathfinder : IPathfinder
    {
        private const int NO_PARENT = -1;

        private readonly INeighbourhood _neighbourhood;
        private readonly IPathHeuristic _heuristic;

        /// <summary>
        /// Creates an A* pathfinder.
        /// </summary>
        /// <param name="neighbourhood">Adjacency rule. Must not be null.</param>
        /// <param name="heuristic">Admissible heuristic matched to neighbourhood/cost. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public AStarPathfinder(INeighbourhood neighbourhood, IPathHeuristic heuristic)
        {
            if (neighbourhood == null)
                throw new ArgumentNullException(nameof(neighbourhood));
            if (heuristic == null)
                throw new ArgumentNullException(nameof(heuristic));
            _neighbourhood = neighbourhood;
            _heuristic = heuristic;
        }

        /// <inheritdoc />
        public PathResult FindPath<T>(
            IReadOnlyMatrix<T> matrix,
            MatrixIndex start,
            MatrixIndex goal,
            ITraversalCondition<T> traversal,
            IStepCost<T> stepCost)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (traversal == null)
                throw new ArgumentNullException(nameof(traversal));
            if (stepCost == null)
                throw new ArgumentNullException(nameof(stepCost));

            if (!matrix.InBounds(start))
                throw new MatrixOutOfBoundsException(start);
            if (!matrix.InBounds(goal))
                throw new MatrixOutOfBoundsException(goal);

            if (!traversal.IsPassable(matrix, start) || !traversal.IsPassable(matrix, goal))
                return PathResult.Failure();

            if (start == goal)
                return PathResult.FromPath(new[] { start }, 0f);

            return Search(matrix, start, goal, traversal, stepCost);
        }

        private PathResult Search<T>(
            IReadOnlyMatrix<T> matrix,
            MatrixIndex start,
            MatrixIndex goal,
            ITraversalCondition<T> traversal,
            IStepCost<T> stepCost)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            int cellCount = width * height;
            int startFlat = ToFlat(start, width);
            int goalFlat = ToFlat(goal, width);

            float[] gScore = new float[cellCount];
            int[] cameFrom = new int[cellCount];
            bool[] closed = new bool[cellCount];
            for (int i = 0; i < cellCount; i++)
            {
                gScore[i] = float.PositiveInfinity;
                cameFrom[i] = NO_PARENT;
            }

            gScore[startFlat] = 0f;

            int[] heapCells = new int[cellCount];
            float[] heapFScores = new float[cellCount];
            int heapSize = 0;
            HeapPush(ref heapCells, ref heapFScores, ref heapSize, startFlat, _heuristic.Estimate(start, goal));

            while (heapSize > 0)
            {
                int currentFlat = HeapPop(heapCells, heapFScores, ref heapSize);
                if (closed[currentFlat])
                    continue;

                if (currentFlat == goalFlat)
                    return ReconstructPath(cameFrom, gScore, startFlat, goalFlat, width);

                closed[currentFlat] = true;
                MatrixIndex current = FromFlat(currentFlat, width);
                IReadOnlyList<MatrixIndex> neighbours = _neighbourhood.GetNeighbours(current, width, height);

                for (int n = 0; n < neighbours.Count; n++)
                {
                    MatrixIndex neighbour = neighbours[n];
                    int neighbourFlat = ToFlat(neighbour, width);
                    if (closed[neighbourFlat])
                        continue;
                    if (!traversal.IsPassable(matrix, neighbour))
                        continue;
                    if (!traversal.CanTraverse(matrix, current, neighbour))
                        continue;

                    float step = stepCost.GetCost(matrix, current, neighbour);
                    if (float.IsNaN(step) || float.IsInfinity(step) || step <= 0f)
                        throw new ArgumentOutOfRangeException(
                            nameof(stepCost),
                            step,
                            "Step cost must be finite and > 0.");

                    float tentativeG = gScore[currentFlat] + step;
                    if (tentativeG >= gScore[neighbourFlat])
                        continue;

                    cameFrom[neighbourFlat] = currentFlat;
                    gScore[neighbourFlat] = tentativeG;
                    float fScore = tentativeG + _heuristic.Estimate(neighbour, goal);
                    HeapPush(ref heapCells, ref heapFScores, ref heapSize, neighbourFlat, fScore);
                }
            }

            return PathResult.Failure();
        }

        private static PathResult ReconstructPath(
            int[] cameFrom,
            float[] gScore,
            int startFlat,
            int goalFlat,
            int width)
        {
            List<MatrixIndex> reverse = new List<MatrixIndex>();
            int current = goalFlat;
            while (current != startFlat)
            {
                reverse.Add(FromFlat(current, width));
                current = cameFrom[current];
            }

            reverse.Add(FromFlat(startFlat, width));
            reverse.Reverse();
            return PathResult.FromPath(reverse, gScore[goalFlat]);
        }

        private static int ToFlat(MatrixIndex index, int width) =>
            index.Y * width + index.X;

        private static MatrixIndex FromFlat(int flat, int width) =>
            new MatrixIndex(flat % width, flat / width);

        private static void HeapPush(ref int[] cells, ref float[] fScores, ref int size, int cell, float fScore)
        {
            if (size == cells.Length)
            {
                int newCapacity = cells.Length * 2;
                Array.Resize(ref cells, newCapacity);
                Array.Resize(ref fScores, newCapacity);
            }

            int i = size;
            size++;

            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (fScores[parent] <= fScore)
                    break;
                cells[i] = cells[parent];
                fScores[i] = fScores[parent];
                i = parent;
            }

            cells[i] = cell;
            fScores[i] = fScore;
        }

        private static int HeapPop(int[] cells, float[] fScores, ref int size)
        {
            int root = cells[0];
            size--;
            if (size == 0)
                return root;

            int cell = cells[size];
            float fScore = fScores[size];
            int i = 0;
            while (true)
            {
                int left = i * 2 + 1;
                if (left >= size)
                    break;
                int right = left + 1;
                int smallest = left;
                if (right < size && fScores[right] < fScores[left])
                    smallest = right;
                if (fScores[smallest] >= fScore)
                    break;
                cells[i] = cells[smallest];
                fScores[i] = fScores[smallest];
                i = smallest;
            }

            cells[i] = cell;
            fScores[i] = fScore;
            return root;
        }
    }
}
