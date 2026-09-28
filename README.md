# Counter Smash

**Counter Smash is a portfolio game prototype exploring how precise charge timing and punchy audiovisual feedback can make a simple physics shot feel satisfying.**

A mobile physics game built with Unity 6. Aim at toy-brick structures, time your shot, and watch the room react.

<!-- Add a repository banner or gameplay GIF here when ready. Suggested path: Documentation/Images/hero.gif -->

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

Hold anywhere on the screen to charge, then release to fire. The charge meter loops: it fills, reaches its peak, drops back to zero, and starts again. Its output follows a tuned ease-out curve rather than a linear fill, giving the player more readable control near the high-power end while keeping the perfect shot demanding.

There are two outcomes:

- **Normal shot:** Release outside the perfect window for a standard physics shot.
- **Explosive shot:** Release at or above the perfect-shot threshold (98% by default) to trigger an explosive hit. The window is intentionally risky: wait too long and the meter wraps from full back to zero.

The feedback is part of the mechanic: the charge sound changes in volume and pitch as power builds, the meter signals when the perfect window is reached, and a successful explosive shot adds a distinct launch cue, impact effect, sound, and physical blast.

## What I Focused On

- **Game feel and juice:** Hit stop, camera shake, screen flash, particles, impact audio, charge feedback, and perfect-shot feedback make each action easy to read and satisfying.
- **A clear risk/reward interaction:** Decide whether to fire a safe normal shot or time the narrow, looping charge window for an explosive payoff.
- **Physics-driven destruction:** Each brick uses a Rigidbody and Collider; structures topple and scatter through Unity physics rather than a scripted fracture system.
- **Seeded level generation:** Hand-authored brick modules are combined with weighted random selection. A fixed seed and matching configuration reproduce the same layout.
- **Concise progression:** Clear every target with limited ammo, earn up to three stars based on shots used, and keep your best result across 20 levels.

## Built With

- Unity `6000.3.17f1`
- C#
- Universal Render Pipeline (URP)
- Unity Input System

## Project Structure

- `Assets/Scenes/MainMenu.unity` — main menu and level selection
- `Assets/Scenes/Arena.unity` — gameplay scene
- `Assets/Scripts/` — gameplay, generation, progression, UI, and feedback systems
- [`PROCEDURAL_GENERATION.md`](PROCEDURAL_GENERATION.md) — generation design and current limitations

### Selected Systems

- `BallLauncher` / `BallProjectile` — pointer input, charge curve and timing, aiming, projectile motion, and ball pooling
- `PerfectBallExplosion` — perfect-shot blast and its impact feedback
- `LevelGenerator` / `ModuleWidth` / `SeededRandom` — weighted module selection and reproducible layouts
- `ClearableBrick` / `LevelManager` — target cleanup, win/fail state, and star calculation
- `LevelSession` — level selection, progression, retries, and saved best scores

## Open in Unity

1. Install Unity `6000.3.17f1` with the modules you need.
2. Clone this repository and open the project folder in Unity Hub.
3. Open `Assets/Scenes/MainMenu.unity` to start from the menu, or `Assets/Scenes/Arena.unity` to inspect gameplay.

The repository contains the Unity project source. A playable build will be linked here if one is published.

## Third-Party Assets

The project uses CC0 assets from [Kenney](https://kenney.nl/) and [Kay Lousberg](https://kaylousberg.com/); their license files are included alongside the asset packs.

Cartoon FX Remaster Free is distributed through the Unity Asset Store. Its source files are excluded from this repository. Import it separately under your own Unity Asset Store license if you want to restore those effects.

## License

No license is currently granted for the original project source or artwork. Third-party assets remain subject to their respective license terms.
