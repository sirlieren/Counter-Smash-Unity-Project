# Procedural Level Generation

Toy Room Smash builds levels from small, hand-authored brick modules. The generator uses a level
seed so the same configuration can reproduce the same module sequence and layout.

## Current implementation

- Each module is a prefab containing an assembled brick structure. Its root has a `ModuleWidth`
  component with its width in studs and its relative selection weight.
- `LevelGenerator` chooses a target width from a configurable range, then repeatedly selects a
  weighted-random module that fits the remaining budget.
- The generator stops when no module fits. It does not reroll to fill the budget exactly.
- The resulting row is centered on `originPoint`; the point's Y and Z coordinates are preserved.
- `SeededRandom` wraps `System.Random` and derives independent child seeds for budget and placement.
- `LevelSession` derives a stable seed for each level index, so retries reproduce the same layout.

## Authoring modules

- Use a compound prefab made from a small group of bricks; `ModuleWidth` belongs on its root.
- Set `Width In Studs` to the module's horizontal footprint.
- Keep modules aligned to the same orientation and baseline so they fit together cleanly.
- Give each module a positive selection weight. Higher weights make a module more likely to appear.
- Keep the module library small while validating the generator and its layouts.

## Deterministic generation

Gameplay generation uses `System.Random` through `SeededRandom` instead of Unity's shared random
state. A stable FNV-1a hash derives subsystem seeds from the level seed, so changes to one random
stream do not shift the sequence used by another subsystem.

The output also depends on the module list and its order, module weights, budget range, and width
conversion setting. Keep those inputs fixed when comparing layouts across runs.

## Planned stability validation

The current generator does not validate whether a layout settles reliably. A planned editor tool
will validate candidate seeds before adding them to an approved pool:

1. Generate a candidate layout and record the initial positions of its bricks.
2. Advance physics for a fixed interval without launching a ball.
3. Measure each brick's displacement and compute a total displacement score.
4. Reject candidates above a configurable threshold; cap attempts to avoid an endless search.
5. Save passing seeds for use as authored level entries.

This approach checks the actual Unity physics setup and keeps validation to one easy-to-tune metric.

## Source files

- `Assets/Scripts/LevelGenerator.cs`
- `Assets/Scripts/ModuleWidth.cs`
- `Assets/Scripts/SeededRandom.cs`
- `Assets/Scripts/LevelSession.cs`
