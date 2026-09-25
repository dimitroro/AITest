# Matrix module — architecture

## Responsibilities

- Own a generic 2D matrix container (`IMatrix<T>` / `IReadOnlyMatrix<T>`): size, bounds, get/set, reshape (crop / expand), element and index queries, neighbour listing via an injected neighbourhood, plus checkpoint-accepted query/mutate helpers.
- Own pathfinding over a matrix (`IPathfinder`): find a route between two cells using caller-supplied traversal and step-cost rules, plus injected neighbourhood and heuristic.
- Stay pure C# (no UnityEngine). No scenes, MonoBehaviours, or visualisation.

## Dependency direction

```
Gameplay / AI  →  IMatrix<T>, IPathfinder, seam interfaces
IPathfinder    →  IReadOnlyMatrix<T>, INeighbourhood, IPathHeuristic, ITraversalCondition<T>, IStepCost<T>
IMatrix<T>     →  INeighbourhood (only for neighbour queries)
Implementations → their interface contracts only (no cycles)
```

High-level consumers depend only on abstractions. Concrete types are constructed at the composition root (gameplay bootstrap), not inside module logic.

## Entry points

| Seam / entry | Interface | Implementations |
|---|---|---|
| Matrix container | `IMatrix<T>` (extends `IReadOnlyMatrix<T>`) | `Matrix<T>` |
| Pathfinding | `IPathfinder` | `AStarPathfinder` |
| Traversal condition | `ITraversalCondition<T>` | `PredicateTraversalCondition<T>`; `NoCornerCuttingTraversalCondition<T>` (decorator for 8-way) |
| Step cost | `IStepCost<T>` | `ConstantStepCost<T>` (4-way); `OctileStepCost<T>` (8-way: ortho 1 / diag √2) |
| Neighbourhood | `INeighbourhood` | `OrthogonalNeighbourhood` (4-way), `EightWayNeighbourhood` (8-way) |
| Heuristic | `IPathHeuristic` | `ManhattanHeuristic` (pair with 4-way), `OctileHeuristic` (pair with 8-way) |

Shipped consistent combinations (composition root):

| Mode | Neighbourhood | Heuristic | Step cost | Traversal |
|---|---|---|---|---|
| 4-way | `OrthogonalNeighbourhood` | `ManhattanHeuristic` | `ConstantStepCost<T>(1)` | `PredicateTraversalCondition<T>` (or any) |
| 8-way | `EightWayNeighbourhood` | `OctileHeuristic` | `OctileStepCost<T>` | `NoCornerCuttingTraversalCondition<T>(inner)` wrapping cell/edge rules |

## Contracts (summary)

### Coordinate model

- `MatrixIndex(X, Y)`: column `X` in `[0, Width)`, row `Y` in `[0, Height)`. Origin `(0,0)` is the top-left cell.
- `MatrixRegion(X, Y, Width, Height)`: axis-aligned inclusive-start, exclusive-end style region whose cells must lie inside the parent matrix.
- `ExpandPadding(Left, Right, Top, Bottom)`: non-negative cells added on each side.
- `MatrixCell<T>(Index, Value)`: one enumerated cell.

### `IReadOnlyMatrix<T>` / `IMatrix<T>`

- **Get / Set**: out-of-bounds → `MatrixOutOfBoundsException`.
- **TryGet / TrySet**: return false on out-of-bounds (no throw); TrySet leaves matrix unchanged when false.
- **Crop**: invalid / empty / out-of-parent region → `InvalidMatrixRegionException`. Returns a **new** matrix (copy); source unchanged.
- **Expand**: negative padding → `ArgumentOutOfRangeException`. New cells filled with `fillValue`. Returns a **new** matrix.
- **ExpandTo** (named rule: **full-containment grow**):
  - Requires `width >= this.Width` and `height >= this.Height`.
  - Requires `contentOrigin.X >= 0`, `contentOrigin.Y >= 0`, and the entire source fits:
    `contentOrigin.X + Width <= width` and `contentOrigin.Y + Height <= height`.
  - Every source cell is copied exactly once; remaining result cells are `fillValue`.
  - Violations → `ArgumentOutOfRangeException`. Shrink / partial / clipped paste is out of scope (use Crop, then Expand/ExpandTo).
- **GetRow / GetColumn / GetRegion**: invalid index/region → same exceptions as above. Returned collections are snapshots (not live views).
- **GetNeighbours**: returns neighbour **values** only (same order as `INeighbourhood.GetNeighbours`). Consumers that need indices call `INeighbourhood.GetNeighbours` themselves. Null neighbourhood → `ArgumentNullException`.
- **Find / FindAll**: by value (`EqualityComparer<T>.Default`) or predicate; empty list if none. Null predicate → `ArgumentNullException`.
- Lifetime: caller owns the matrix instance; crop/expand/clone produce independent instances.

### Checkpoint-accepted API (accepted by user at checkpoint)

| API | On | Contract |
|---|---|---|
| `Fill(value)` | `IMatrix<T>` | Sets every cell. O(Width×Height). |
| `FillRegion(region, value)` | `IMatrix<T>` | Region must fit; else `InvalidMatrixRegionException`. |
| `Clone()` | `IMatrix<T>` | New independent matrix, same size/values. Source unchanged. |
| `TryGet` / `TrySet` | read / write | Bounds-safe; false on OOB; no throw. |
| `Enumerate()` | `IReadOnlyMatrix<T>` | `IEnumerable<MatrixCell<T>>`, row-major; do not mutate during enumeration. |
| `Count(predicate)` / `Any` / `All` | `IReadOnlyMatrix<T>` | Null predicate → `ArgumentNullException`. Any/All may short-circuit. |

### Pathfinding seams

- **`ITraversalCondition<T>`**: `IsPassable` = cell rules; `CanTraverse` = edge-only rules (including diagonal corner-cutting). Pathfinder does not interpret cell values itself.
- **`IStepCost<T>`**: `GetCost(matrix, from, to)` must return a finite value `> 0`. Non-positive / NaN / infinity → pathfinder throws `ArgumentOutOfRangeException` when encountered.
- **`INeighbourhood`**: adjacency only (bounds-aware). Does not apply walkability or corner-cutting against blocked cells.
- **`IPathHeuristic`**: admissible estimate for the paired neighbourhood/cost. Must be `>= 0` and finite.
- **Diagonal corner-cutting**: `INeighbourhood` cannot see blocked cells. `EightWayNeighbourhood` always yields all in-bounds 8 neighbours. Shipped guard: wrap any `ITraversalCondition<T>` in `NoCornerCuttingTraversalCondition<T>` (rejects diagonal when either orthogonal sharing cell fails `IsPassable`; then delegates to inner `CanTraverse`). Consumers who want corner-cutting allowed omit the decorator.
- **`IPathfinder.FindPath` traversal invocation rule (every impl must follow)**:
  1. Start/goal out of bounds → `MatrixOutOfBoundsException`.
  2. `!IsPassable(start)` or `!IsPassable(goal)` → `PathResult.Failure`.
  3. If start equals goal and step 2 passed → Success, path `[start]`, `TotalCost == 0`.
  4. Expand neighbour `to` from `from` only if `IsPassable(to)` **and** `CanTraverse(from, to)`; never on `CanTraverse` alone; never skip `IsPassable(to)`.
  5. Call `GetCost` only for edges that pass step 4.
  6. Unreachable → `PathResult.Failure`. Success → path start→goal inclusive; `TotalCost` = sum of step costs.
- Null args → `ArgumentNullException`.

### Threading

Matrix and pathfinder types are **not** thread-safe (call from one thread). Stateless sealed neighbourhood/heuristic impls are safe for concurrent reads.

## Decisions

| Decision | Why |
|---|---|
| Split `IReadOnlyMatrix<T>` / `IMatrix<T>` | Pathfinding and queries must not require mutation (ISP). |
| Crop/Expand return new matrices | Reshape is a transform, not in-place mutation; safer for undo/snapshots. |
| ExpandTo = full-containment grow | Matches brief “grow”; unambiguous vs clip/discard; shrink stays on Crop. |
| Neighbour queries take `INeighbourhood` | Same seam as pathfinder; 4- vs 8-way without editing matrix. |
| GetNeighbours returns values only | Keeps matrix API focused; indices already exposed by `INeighbourhood`. |
| Corner-cutting via `CanTraverse` decorator | `INeighbourhood` stays geometry-only; `NoCornerCuttingTraversalCondition<T>` is the shipped recipe. |
| Pathfinder ctor gets neighbourhood + heuristic; FindPath gets matrix + traversal + cost | Algorithm geometry is stable; gameplay rules often vary per unit/call. |
| Mandatory IsPassable∧CanTraverse on every expand | LSP: all pathfinder impls share one contract so cell rules in IsPassable always apply. |
| start==goal → Success `[start]` / cost 0 | Explicit LSP; avoids Failure vs trivial-path divergence. |
| A* as the one algorithm | Standard for grids with non-uniform cost; Dijkstra/BFS can be later impls of `IPathfinder`. |
| Predicate-based traversal / constant step cost | Lets consumers supply rules without new types; constant cost covers uniform 4-way grids. |
| `OctileStepCost` matching `OctileHeuristic` | Makes 8-way admissible out of the box; avoids ConstantStepCost(1) + Octile mismatch. |
| Both 4-way and 8-way complete shipped stacks | Checkpoint + OPEN_RISKS fix; consumers pick a row from the combination table. |
| Fail-with-`PathResult` for unreachable | Normal gameplay outcome, not exceptional control flow. |
| `noEngineReferences` on asmdef | Pure C#; keeps the module usable from Edit Mode tests without Unity runtime. |
| Public sealed implementations, no factory | BASE-4: composition root constructs them directly. |

## Changes since review

- **[B1]** Specified ExpandTo as **full-containment grow**: target size must be >= source; `contentOrigin` non-negative and entire source must fit; no clip/discard; violations → `ArgumentOutOfRangeException`. Updated `IMatrix.cs` and this doc (shrink remains Crop).
- **[B2]** Documented mandatory traversal invocation rule on `IPathfinder.FindPath` and `ITraversalCondition` remarks: expand only when `IsPassable(to) ∧ CanTraverse(from, to)`; start/goal require `IsPassable`. Updated sequence diagram to call `IsPassable` on each neighbour before `CanTraverse`.
- **[N1] (review-1)** Dropped proposed `Map` from the additional-API list (weakest typical-grid item).
- **[N2]** Documented that `GetNeighbours` returns values only; indices via `INeighbourhood.GetNeighbours`.
- **[N3]** Clarified `OrthogonalNeighbourhood` as thread-safe for concurrent reads (stateless).

### Checkpoint additions

- Accepted all proposed APIs into contracts/skeleton: `Fill`/`FillRegion`, `Clone`, `TryGet`/`TrySet`, `Enumerate`/`MatrixCell<T>`, `Count`/`Any`/`All` (read vs write split as above).
- Added `EightWayNeighbourhood` + `OctileHeuristic`; corner-cutting stays off the neighbourhood seam.
- Applied review-2 **[N1]**: start == goal and passable → Success `[start]`, TotalCost 0.
- Closed 8-way usability gaps: added `OctileStepCost<T>` (matches Octile heuristic) and `NoCornerCuttingTraversalCondition<T>` (composable decorator); documented full 4-way and 8-way shipped combinations.

## Assembly layout

```
Assets/Modules/Matrix/
  Matrix.asmdef
  Core/           — indices, region, cell, matrix interfaces + Matrix<T>, exceptions
  Neighbourhood/  — INeighbourhood + OrthogonalNeighbourhood + EightWayNeighbourhood
  Pathfinding/    — pathfinder, traversal, cost, heuristics, PathResult
```

Docs: `docs/modules/Matrix/` (repo root).

## Out of scope (confirmed)

MonoBehaviours, scenes, visualisation, tests, algorithm bodies, alternate path algorithms (Dijkstra/BFS), factories/registries.
