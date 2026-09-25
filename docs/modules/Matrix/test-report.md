# Matrix module — test report (tester round 1)

## Summary

| Field | Value |
|---|---|
| RESULT | **PASS** |
| Run | `dotnet test` (NUnit **3.5.0**, net48, NUnit3TestAdapter 3.17.0, outside Unity) via throwaway project under orchestrator `test-runner/` |
| Passed | 112 |
| Failed | 0 |
| Not run | Unity Test Runner (Editor) — prior batchmode compile failed on `Does.Contain`; fixed below; re-run in Editor recommended |

## Unity NUnit 3.5 fix (answer-2)

Unity 6000.3.18f1 ships NUnit 3.5, where `Does.Contain` / `Does.Not.Contain` accept **string** only. Replaced every non-string use with `Has.Member` / `Has.No.Member` (asserts unchanged).

Runner pinned to `NUnit` **3.5.0** + `NUnit3TestAdapter` **3.17.0**. Target framework set to **net48** (NUnit 3.5 is net45-only and does not discover/run under net9.0). LangVersion remains 9.0.

```
Test Run Successful.
Total tests: 112
     Passed: 112
```

## Style pass (answer-1)

Applied team rules to test sources only (no production edits):

- TEST-001: renamed all tests to `When…_Then…` / `When…_And…_Then…`
- STYLE-001: no `var`; explicit types throughout
- NAME-004: `FloatAssert.TOLERANCE`
- STYLE-003: removed narrating what-comments
- STYLE-008: split support helpers into one type per file (`FloatAssert`, `RecordingTraversalCondition`, `RecordingStepCost`, `GridHelpers`, plus fixture splits)
- STYLE-011: recording fakes expose `IReadOnlyList` call logs

Re-run after style + NUnit 3.5 pin: **112 passed** (same count).

## How tests were executed

Unity Editor / Unity MCP was **not** available this session (per task). Tests live in
`AiTest/Assets/Tests/MatrixTests/` (`Matrix.Tests` asmdef, Editor-only, NUnit-only sources).

They were compiled and run with the .NET SDK by a throwaway project that `<Compile Include>`s:

- `AiTest/Assets/Modules/Matrix/**/*.cs`
- `AiTest/Assets/Tests/MatrixTests/**/*.cs`

Command: `dotnet test` in
`C:\Users\OLEKSANDR_DEV\.agent-orchestrator\runs\20260925-1046-Matrix-tester1\test-runner`

```
Test Run Successful.
Total tests: 112
     Passed: 112
```

## Coverage map (contract → tests)

### Coordinate / value types

| Contract | Tests |
|---|---|
| `MatrixRegion.Contains` inclusive-start exclusive-end | `WhenRegionContainsChecked_ThenInclusiveStartExclusiveEnd` |
| `PathResult.FromPath` empty / negative cost | `WhenFromPathEmpty_ThenThrowsArgumentException`, `WhenFromPathNegativeCost_ThenThrowsArgumentOutOfRange` |

### `IReadOnlyMatrix` / `IMatrix` (`Matrix<T>`)

| Contract | Tests |
|---|---|
| Get/Set OOB → `MatrixOutOfBoundsException` | `WhenGetOutOfBounds_*`, `WhenSetOutOfBounds_*` |
| TryGet/TrySet false on OOB, no throw; TrySet leaves unchanged | `WhenTryGet_*`, `WhenTrySet_*` |
| Crop invalid/empty/out-of-parent → `InvalidMatrixRegionException`; new matrix | `WhenCrop*` |
| Expand negative padding → `ArgumentOutOfRangeException`; fill new cells | `WhenExpand*` |
| ExpandTo full-containment grow; shrink / bad origin / no-fit → `ArgumentOutOfRangeException` | `WhenExpandTo*` |
| GetRow/GetColumn/GetRegion snapshots + exceptions | `WhenGetRow*`, `WhenGetColumn*`, `WhenGetRegion*` |
| GetNeighbours values only, null NH / OOB center | `WhenGetNeighbours*` |
| Find / FindAll value & predicate; null predicate | `WhenFind*`, `WhenFindAll*` |
| Enumerate row-major | `WhenEnumerate_ThenYieldsAllCellsInRowMajorOrder` |
| Count / Any / All + null predicate | `WhenCount*`, `WhenAny*`, `WhenAll*` |
| Fill / FillRegion | `WhenFill*`, `WhenFillRegion*` |
| Clone independence | `WhenCloneMutated_ThenSourceRemainsUnchanged` |

### LSP — `INeighbourhood`

| Impl | Shared suite | Specific |
|---|---|---|
| `OrthogonalNeighbourhood` | `NeighbourhoodLspTests` | `WhenOrthogonalAtCenter_*` |
| `EightWayNeighbourhood` | same | `WhenEightWay*` |

### LSP — `IPathHeuristic`

| Impl | Shared suite | Specific |
|---|---|---|
| `ManhattanHeuristic` | `WhenEstimate*` (non-negative, finite, zero, symmetric, admissible) | `WhenManhattanHorizontalMove_*` |
| `OctileHeuristic` | same | `WhenOctileDiagonalOneStep_*` |

### LSP — `IStepCost`

| Impl | Coverage |
|---|---|
| `ConstantStepCost<T>` | `WhenGetCostOrthogonalStep_*`, `WhenConstantStepCost*` |
| `OctileStepCost<T>` | `WhenOctileStepCost*` |

### LSP — `ITraversalCondition`

| Impl | Coverage |
|---|---|
| `PredicateTraversalCondition<T>` | `WhenIsPassableOnOpenCell_*`, `WhenPredicate*` |
| `NoCornerCuttingTraversalCondition<T>` | `WhenNoCornerCutting*`, `WhenWithoutDecorator*` |

### Pathfinding (`IPathfinder` / `AStarPathfinder`)

| Scenario | Test |
|---|---|
| 4-way optimal length/cost | `WhenFindPathFourWayOpenGrid_*` |
| 8-way optimal diagonal cost | `WhenFindPathEightWayOpenGrid_*` |
| Walls detour | `WhenFindPathAroundWalls_*` |
| Unreachable | `WhenFindPathUnreachableGoal_*` |
| Blocked start / goal | `WhenFindPathBlockedStart_*`, `WhenFindPathBlockedGoal_*` |
| start == goal → `[start]`, cost 0 | `WhenFindPathStartEqualsGoal_*` |
| Corner-cutting forbidden (decorator) | `WhenFindPathCornerCuttingForbidden*` |
| Corner-cutting allowed (no decorator) | `WhenFindPathCornerCuttingAllowed*` |
| Invalid step cost → `ArgumentOutOfRangeException` | `WhenFindPathInvalidStepCost*` |
| OOB start/goal → `MatrixOutOfBoundsException` | `WhenFindPath*OutOfBounds_*` |
| Null args → `ArgumentNullException` | `WhenFindPathNull*` |
| Invocation rule | `WhenFindPathExpands_ThenIsPassableBeforeCanTraverseAndGetCostOnlyOnAllowedEdges` |
| Ctor null NH / heuristic | `WhenAStarPathfinderNull*` |

Float comparisons use tolerance `FloatAssert.TOLERANCE` (`1e-5f`).

## Failures

None.

## Coverage gaps

- **Threading / concurrent-read claims** for neighbourhood/heuristic: not exercised.
- **Mutation during `Enumerate`**: contract says undefined — no test asserting undefined behavior.
- **Reentrancy / dispose lifecycle**: no dispose API; N/A.
- **Alternate `IPathfinder` implementations**: only `AStarPathfinder` shipped.
- **Heuristic/cost mis-pairing**: composition-root responsibility; not asserted as a failure mode.

## NOT_RUN

- Unity Test Runner (Edit Mode) after the `Has.Member` fix — previous Unity batchmode compile failed; sources should now compile against Unity’s NUnit 3.5. Re-run in Editor/batchmode to confirm.

## Screenshots

None (pure logic / no UI).
