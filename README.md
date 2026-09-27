# Toy Room Smash

Toy Room Smash is a small mobile physics game built with Unity 6 and URP. Players launch balls at toy-brick structures, clear every target with limited ammunition, and earn up to three stars based on shot efficiency.

This is a portfolio project focused on a compact, polished mobile gameplay loop rather than monetization or long-term progression systems.

## Highlights

- Touch-driven aiming with tap shots and a looping hold-to-charge power meter
- Rigidbody-based destruction with no scripted fracture system
- Seeded procedural layouts assembled from hand-authored brick modules
- Reproducible level seeds with independent random streams
- Ammo, remaining-target, level-select, and star-progression UI
- Camera shake, hit stop, screen flash, particles, spatial impact audio, and perfect-shot feedback

## Project setup

- Unity `6000.3.17f1`
- Universal Render Pipeline (URP)
- Input System package
- Main scenes: `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/Arena.unity`

Open the project with the matching Unity editor version, then load `MainMenu` or use the scenes in Build Settings.

## Architecture

Gameplay code lives in `Assets/Scripts`. The main systems are:

- `BallLauncher` and `BallProjectile` for input, aiming, charge, and projectile motion
- `LevelGenerator`, `ModuleWidth`, and `SeededRandom` for deterministic layouts
- `ClearableBrick`, `KillZone`, and `LevelManager` for win/fail state and cleanup
- `LevelSession` for level selection, unlock state, retries, and best-star persistence
- Small, event-driven feedback components for audio, camera, particles, UI, and hit reactions

See [PROCEDURAL_GENERATION.md](PROCEDURAL_GENERATION.md) for the generation design and planned stability validation.

## Third-party assets

The project uses CC0 assets from [Kenney](https://kenney.nl/) and [Kay Lousberg](https://kaylousberg.com/). Their original license files are included beside the asset packs.

Cartoon FX Remaster Free is distributed through the Unity Asset Store. Its source files are excluded from this repository; import the package separately under your own Unity Asset Store license to restore those effects in the project.

## License

No license is currently granted for the original project source or artwork. Third-party assets remain subject to their respective license terms.
