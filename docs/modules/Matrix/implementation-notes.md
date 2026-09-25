# Matrix module — implementation notes

## Decisions

- **Storage:** `Matrix<T>` keeps a private row-major `T[]` (`index = y * Width + x`). Constructors copy input arrays so callers cannot alias internal storage.
- **Region validation:** Shared private `EnsureRegionFits` for `GetRegion` / `FillRegion` / `Crop`: non-positive size or out-of-parent → `InvalidMatrixRegionException`.
- **Expand:** Implemented via `ExpandTo` with `contentOrigin = (Left, Top)` after rejecting negative padding. Avoids duplicating paste logic.
- **Neighbourhood order:** Orthogonal N→E→S→W; eight-way N→NE→E→SE→S→SW→W→NW. Stable; documented as implementation-defined.
- **A\* open set:** Binary min-heap of `(flatIndex, fScore)` with duplicate entries allowed; stale entries skipped when a closed cell is popped. Heap grows if reopen duplicates exceed initial capacity. No LINQ in the search loop.
- **A\* indexing:** Flat `int` keys (`y * width + x`) for g-score / came-from / closed arrays to avoid dictionary allocations per search.
- **Octile constants:** `D = 1`, `D2 = √2` shared conceptually between `OctileHeuristic` and `OctileStepCost<T>` (each holds its own static field; no shared static coupling).
- **PathResult.Failure:** Cached singleton empty failure instance; `FromPath` validates and returns a new success instance.

## Deviations

None. Public API and documented contracts match the approved architecture/skeleton.

## Limits

- Not thread-safe (per architecture).
- A\* allocates per call (g/cameFrom/closed arrays, neighbour lists from neighbourhood, path list). Acceptable for non–per-frame use; no LINQ in the loop.
- `GetRow` / `GetColumn` / `GetRegion` / `GetNeighbours` / `FindAll` return new snapshots each call.
- Heuristic admissibility is the composition root’s responsibility; the pathfinder does not validate heuristic/cost pairing.
- Compile verified with a throwaway `netstandard2.1` class library (Unity Editor not available in this run).
