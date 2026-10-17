# Projekt Alpha9

A science-fiction FPS prototype built in [s&box](https://sbox.game/) game project by [Studio Prism](https://github.com/StudioPrism).

This repository is an early prototype containing camera/controller work, development scenes, and project settings. It is not a playable release.

## Project identity

Projekt Alpha9 continues the existing Projekt Alpha repository and its history. The s&box display title is now Projekt Alpha9. The existing `projektalpha.sbproj` filename and `local.projektalpha` package identity are retained to avoid changing project references during this rename.

The GitHub repository slug is currently `StudioPrism/Projekt-Alpha`; the intended new name is `Projekt-Alpha9`. Links below use the existing working URL until the repository rename is completed.

## Branches and current status

The notes below describe the default `main` branch. The [`InDev` branch](https://github.com/StudioPrism/Projekt-Alpha/tree/InDev) contains a different, more extensive controller implementation; its dependencies and compile status must be checked on that branch. Do not treat `main`'s controller stub as the state of all development work.

- Project definition: [projektalpha.sbproj](projektalpha.sbproj).
- Startup scene configured in the project: `scenes/minimal.scene`.
- Camera prototype: [Code/Player/cameraController.cs](Code/Player/cameraController.cs).
- Player controller: [Code/Player/playerController.cs](Code/Player/playerController.cs), currently unfinished.

**Known blocker on `main`:** the player-controller source contains an incomplete field declaration. A fresh checkout should not be described as build-ready until that is completed and the editor compile succeeds.

## Open the project

1. Install the s&box editor through Steam.
2. Clone this repository:
   ```sh
   git clone https://github.com/StudioPrism/Projekt-Alpha.git
   ```
3. Open the s&box editor and add/open the existing project using `projektalpha.sbproj`.
4. Resolve any current compile errors before entering Play mode.
5. Open `Assets/scenes/minimal.scene` for the configured startup scene. Development scenes also live under `Assets/scenes/`.

Use the editor's code-editor integration to open the generated solution in Rider or another compatible IDE. Generated solutions and build artifacts are excluded from version control.

See the [s&box installation guide](https://sbox.game/dev/doc/getting-started/installation) for the editor's current setup instructions.

## Source map

| Path | Purpose |
| --- | --- |
| [Code/](Code/) | Game code and player prototypes |
| [Editor/](Editor/) | Editor-side code |
| [Assets/](Assets/) | Scenes and project assets |
| [ProjectSettings/](ProjectSettings/) | Input and collision settings |
| [projektalpha.sbproj](projektalpha.sbproj) | Project metadata and startup-scene setting |
| [docs/development.md](docs/development.md) | Development and verification workflow |

Input actions are defined in [ProjectSettings/Input.config](ProjectSettings/Input.config). A configured input action does not establish that its gameplay behavior is implemented.

## Workflow

Track current work in [Issues](https://github.com/StudioPrism/Projekt-Alpha/issues). Keep each task focused on an observable outcome, and link its implementation pull request.

przm.DEV can launch s&box and open a linked local clone in Rider. It is a separate application; it is not included in this checkout. [Lightstream-Engine](https://github.com/StudioPrism/Lightstream-Engine) is the separate framework and SDK effort being built around s&box. Its main branch currently contains the foundation layout and documentation; this game does not yet declare a Lightstream dependency.

## Contributions and notices

Follow the [Studio Prism contribution guide](https://github.com/StudioPrism/.github/blob/main/CONTRIBUTING.md). Preserve existing source and asset notices. This documentation does not replace or expand the licensing terms attached to individual files or third-party assets.
