# Development

## First setup

1. Clone the repository and check out `Dev`.
2. Create a new game project using the s&box editor, using Projekt Alpha9 as its display title. Use the repository root for the project and retain this documentation.
3. Commit the actual project descriptor and required generated source/settings. Do not restore the old descriptor or its startup-scene references without reviewing them.
4. Create a minimal development scene and configure it as the startup scene.
5. Confirm that the project compiles and opens in Play mode before implementing gameplay.

No project descriptor, startup scene, controller, automated build, or runtime test suite currently exists on main or Dev. IDE/build instructions will be documented after the new editor project is created.

## Workflow

Branch from Dev for focused changes. Merge reviewed changes into Dev, validate in s&box, and promote stable work to main. Record the editor version, tested commit, scene, and reproduction steps when reporting gameplay verification.

Keep generated IDE files and caches out of git. Track source assets, required project metadata, and shared project settings. Do not add dependencies or describe features as working until they have been validated.

## Pre-reset references

- `PreReset-1` preserves main at `9dbbb7f968b9fa6842a405d4904391a8d0772114`.
- `InDev` retains its separate prototype at `c144b64c7d561efc85ab535ededc4a81487e74d7`.

The reset is a normal commit on main; earlier history remains available. Restore individual files only after reviewing their purpose, dependencies, and source notices.
