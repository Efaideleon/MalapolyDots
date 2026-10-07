# Malapoly dice

Two rounded ivory dice with recessed pips, a floor drop, and a camera-facing result reveal.

## In the game

Use the existing Roll button. The server chooses the two values and broadcasts them;
each client loads `Resources/Dice/DiceRollPresentation.prefab` automatically. The dice
fall and bounce on the board floor for 1.65 seconds, pause for 0.3 seconds, then lift
toward the camera over 0.8 seconds. The chosen faces turn toward the screen and a label
shows the individual values and their total (for example, `3 + 5 = 8`) for 1.4 seconds.
The active player's completion message releases token movement after the screen reveal.
An eight-second timeout prevents an unavailable client from stalling the turn.

The existing custom-roll debug setting still chooses a total. It is clamped to 2–12
and split into two legal d6 values. Animation does not change the gameplay result.

## Try a chosen pair

Open `Assets/Scenes/DicePreview.unity` and enter Play mode. Select
`DiceRollPresentation`, set **Preview First** and **Preview Second** in its inspector,
then use the component context menu **Roll chosen values** to roll again.
The preview starts with 3 and 5. From code, call `DiceRollPresenter.Play(first, second)`.
`Floor Height` defaults to the board's world Y = 0; `Floor Die Size` controls the dice
size on the board. The floor position is captured when the throw begins, so moving
the camera does not drag the dice. The final screen position follows the camera.

## Source and rebuilding

- Blender source: `/Volumes/T7/Blender/MalacityBlender/Dice/malapoly_dice.blend`
- Blender generator: `create_dice.py` in that same directory.
- Game export: `Malapoly_Die.fbx`. The earlier `Malapoly_Tray.fbx` remains available as
  an optional source asset; the current game presentation does not use the tray.
- Recreate Unity materials/prefab/preview: **Tools > Malapoly > Dice > Build assets**.

The imported `Face_1` through `Face_6` markers define the final orientations, so the
animation does not depend on Blender-to-Unity axis assumptions. Opposite faces sum to
seven. Camera-relative studio shading keeps the dice readable during camera changes.

The roll is animated deterministically, with tumbling, diminishing bounces and exact
final orientations. It does not rely on a random rigidbody simulation finding a
requested result. No scene wiring or Rigidbody is required for the game integration.

## Verification

`Tests/Editor/Dice/DiceRollTests.cs` covers all 36 pairs, valid custom totals, duplicate
and unauthorized requests, waiting for settlement, the timeout, turn cancellation,
and the client presentation/completion path. Tests also check that every selected
face points at the camera after the pull, with no jump at the start of the lift.
`DiceAssetSetup.Validate()` samples all 36 throws and checks mesh vertices against the floor.
