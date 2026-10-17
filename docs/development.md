# Developing Projekt Alpha9

## Branch scope

These setup notes describe `main`. `InDev` contains a separate controller implementation. Record which branch and commit you are testing; do not transfer a build-status claim from one branch to the other.

## Tools and setup

Use the s&box editor for project loading, compilation, scenes, and play testing. Use Rider or another compatible IDE for C# work. Blender and Krita can support asset creation, but no custom integration or required versions are defined by this repository.

The checked-in `projektalpha.sbproj` configures `scenes/minimal.scene` as the startup scene. Input definitions live in `ProjectSettings/Input.config`.

## Verification checklist

Before reporting a change as verified:

1. Record the commit and s&box/editor version.
2. Open the project and inspect the compile output.
3. Resolve errors introduced by the change. Record any existing baseline errors separately.
4. If compilation succeeds, open the relevant scene and test the changed behavior in Play mode.
5. For controller/camera changes, include concise reproduction steps and a capture when it helps review.

The `main` branch's player controller contains an unfinished declaration. This documentation cleanup does not change runtime code or fix that blocker. No automated build/test workflow is established by these notes.

## Repository hygiene

Keep `Code/`, `Editor/`, source assets, and shared project settings under version control. Respect the existing ignore rules for generated editor output, solutions, and exported games. Do not add broad ignore patterns that hide required source assets.

Keep active task tracking in Issues rather than a separate checklist in the organization-profile repository. When documenting a gameplay feature, distinguish intended behavior from what the current project actually implements.
