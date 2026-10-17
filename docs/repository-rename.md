# Projekt Alpha9 rename

The project continues the existing repository; no new repository or replacement history is needed.

## Completed in source

- Updated the project display title to Projekt Alpha9.
- Added current onboarding and development notes to main.
- Retained existing game assets, source, settings, and the separate InDev branch.
- Retained the existing project filename and package identifier for compatibility.

## GitHub repository rename

Target: `StudioPrism/Projekt-Alpha9`.

The GitHub connector used for this change does not expose repository settings updates. A repository administrator must rename the existing repository in **Settings → General → Repository name**.

After the rename, update repository links in this README, the Studio Prism organization profile, and Lightstream's related-project documentation. Existing local clones should update their remote:

```sh
git remote set-url origin https://github.com/StudioPrism/Projekt-Alpha9.git
```

Do not run the remote update until the renamed repository is available. Do not change the s&box package identifier as an incidental GitHub rename; review package publication and project references first.
