# Counter Smash

**Counter Smash is a mobile physics game prototype about timing a charged shot and making each hit feel impactful.** Aim at toy-brick structures, release at the right moment, and clear the room with limited ammunition.

Built with Unity 6. The project focuses on a compact loop: charge, aim, fire, and read the result through physics, sound, and visual feedback.

## Gameplay Video

[![Watch Counter Smash gameplay](Assets/Screenshots/ss2.png)](https://www.youtube.com/shorts/d7HCrYggvNI)

Click the preview to watch the gameplay short on YouTube.

## Screenshots

<p align="center">
  <img src="Assets/Screenshots/ss1.png" width="24%" alt="Toy-brick targets and remaining shots during gameplay">
  <img src="Assets/Screenshots/ss2.png" width="24%" alt="A perfect shot triggering an explosive hit">
  <img src="Assets/Screenshots/ss3.png" width="24%" alt="Aiming at the remaining toy bricks">
  <img src="Assets/Screenshots/ss4.png" width="24%" alt="Level clear screen showing the earned stars">
</p>

## The Core Feel

Hold anywhere to charge and release to fire. The meter follows a one-second cycle in the gameplay scene: it grows, then snaps back to zero and repeats while held. Its ease-out curve makes the visible power rise quickly at first and linger near the high end. The aim direction comes from the pointer position at release, projected onto a fixed plane in front of the camera; the shot travels directly toward that point rather than along a manually aimed arc.

The gameplay scene uses a 98% threshold on the curved power value. With its current curve and one-second cycle, the perfect range begins roughly 0.14 seconds before the meter resets. This is a timing window, rather than a single exact frame. A normal shot uses the charged speed; a perfect shot also gets a 25% speed boost. The shot's direction and power are independent: moving the pointer sets direction, while hold time sets power.

A perfect ball explodes on its first brick collision, applying an outward impulse to nearby non-kinematic rigidbodies before the ball despawns. The feedback layers are tied to that event: a separate launch cue, explosion sound and particles, screen flash, camera shake, and a brief freeze followed by slow motion. Ordinary brick impacts scale their sound and shake with impact strength; brief hit-stop is reserved for stronger impacts, and repeated ordinary impacts do not extend an active freeze.

## Design Focus

- **Readable timing:** A looping meter gives the player a visible, repeatable charge cycle, while the curved output leaves a short high-power window.
- **Physics-led destruction:** Bricks are individual rigidbodies. They topple and scatter through Unity physics; the perfect-shot blast adds force to nearby bricks instead of fracturing meshes.
- **Layered impact feedback:** Sound, particles, camera motion, flash, and time effects reinforce different stages of a hit, with a stronger sequence for a perfect shot.
- **Repeatable layouts:** Hand-authored modules are selected by weight from a seeded random sequence, so the same seed and configuration reproduce the same row.
- **Short progression loop:** Clear targets with limited ammunition, earn up to three stars based on shots used, and save best results across 20 levels.

## Built With

- Unity `6000.3.17f1` (exact editor version in `ProjectSettings/ProjectVersion.txt`)
- C# and the Unity Input System (`com.unity.inputsystem` 1.19.0)
- Universal Render Pipeline (`com.unity.render-pipelines.universal` 17.3.0)
- TextMesh Pro / Unity UI
- Cartoon FX Remaster Free (separately imported; required by a project script at compile time)

## Project Structure

- [`Assets/Scenes/MainMenu.unity`](Assets/Scenes/MainMenu.unity) — menu, settings, and 20-level selection
- [`Assets/Scenes/Arena.unity`](Assets/Scenes/Arena.unity) — gameplay scene and generated brick layout
- [`Assets/Scripts/`](Assets/Scripts/) — gameplay, generation, progression, UI, and feedback code
- [`PROCEDURAL_GENERATION.md`](PROCEDURAL_GENERATION.md) — generation approach and known limitations

### Selected Systems

- [`BallLauncher.cs`](Assets/Scripts/BallLauncher.cs) / [`BallProjectile.cs`](Assets/Scripts/BallProjectile.cs) — pointer input, charge curve, release-point aiming, projectile motion, and ball pooling
- [`PerfectBallExplosion.cs`](Assets/Scripts/PerfectBallExplosion.cs) — first-brick-contact blast and pooled explosion effect
- [`BrickImpactSound.cs`](Assets/Scripts/BrickImpactSound.cs), [`HitStop.cs`](Assets/Scripts/HitStop.cs), [`CameraShake.cs`](Assets/Scripts/CameraShake.cs), and [`ScreenFlash.cs`](Assets/Scripts/ScreenFlash.cs) — impact strength and perfect-hit feedback
- [`LevelGenerator.cs`](Assets/Scripts/LevelGenerator.cs), [`ModuleWidth.cs`](Assets/Scripts/ModuleWidth.cs), and [`SeededRandom.cs`](Assets/Scripts/SeededRandom.cs) — weighted module placement with reproducible seeds
- [`ClearableBrick.cs`](Assets/Scripts/ClearableBrick.cs) and [`LevelManager.cs`](Assets/Scripts/LevelManager.cs) — target cleanup, level result, and star calculation
- [`LevelSession.cs`](Assets/Scripts/LevelSession.cs) — level selection, retries, unlocks, and saved best scores

## Open in Unity

1. Install Unity `6000.3.17f1` and open this repository's project folder in Unity Hub. Unity Package Manager resolves the packages listed in `Packages/manifest.json`.
2. Import Cartoon FX Remaster Free under your own Unity Asset Store license to resolve the required compile-time dependency. The vendor folder is excluded from this repository, while [`PooledEffect.cs`](Assets/Scripts/PooledEffect.cs) references its `CFXR_Effect` type directly.
3. Open `Assets/Scenes/MainMenu.unity` and press Play to start at the menu. The main menu and gameplay scenes are enabled in Build Settings. Open `Assets/Scenes/Arena.unity` to inspect gameplay directly.

This repository contains the Unity project source; no playable build is linked here.

## Third-Party Assets

The project uses CC0 assets from [Kenney](https://kenney.nl/) and [Kay Lousberg](https://kaylousberg.com/); their license files are included alongside the asset packs.

Cartoon FX Remaster Free is distributed through the Unity Asset Store. Its source files are excluded from this repository, and the project references its `CFXR_Effect` type at compile time. Import it separately under your own Unity Asset Store license to open and compile a clean checkout.

## License

No license is currently granted for the original project source or artwork. Third-party assets remain subject to their respective license terms.
