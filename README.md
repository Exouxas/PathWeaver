# PathWeaver

**PathWeaver** is a non-destructive Unity tool for VRChat avatars that turns your GameObject hierarchy into your VRChat expression menu folder structure. It rewrites [VRCFury](https://vrcfury.com/) menu paths at avatar build time — no manual path editing required.

## How It Works

Place a `PathWeaver` component on any GameObject in your avatar hierarchy. The names of the GameObjects nested beneath it are used to automatically generate menu folder paths for any VRCFury features found in the same subtree.

At build time (before VRCFury runs), PathWeaver walks every child `VRCFury` component and rewrites its menu path based on where the component sits in the hierarchy:

- **Toggles** (`VF.Model.Feature.Toggle`): the folder path derived from the hierarchy is prepended to the existing toggle name (e.g. `MyFeature` under `Outfit/Shirt` becomes `Outfit/Shirt/MyFeature`).
- **Full Controllers** (`VF.Model.Feature.FullController`): the menu prefix is replaced entirely with the folder path.

## Configuration

| Field | Description |
|---|---|
| `Root Prefix` | A string prepended to every generated path, e.g. `"My Avatar"`. Leave empty for none. |
| `Skip Prefix` | GameObjects whose name starts with this string are excluded from the folder path, e.g. `"_"`. |
| `Rules` | List of rewrite rules. Each rule maps a VRCFury feature type to a field name and a path mode. |

### Path Modes

- **FolderPlusLeaf** — uses the folder path plus the last segment of the existing value. Suitable for Toggles.
- **FolderOnly** — replaces the existing value entirely with the folder path. Suitable for Full Controller menu prefixes.

## Editor Utilities

Right-click the `PathWeaver` component header in the Inspector for two helper actions:

- **Preview Generated Paths** — performs a dry run and logs what paths would be written, without modifying anything.
- **Dump Feature Fields** — logs every unique VRCFury feature type found under the root along with its serialized fields, useful for configuring custom rules.


## 🎉 Publishing a Release

You can make a release by running the [Build Release](.github/workflows/release.yml) action. The version specified in your `package.json` file will be used to define the version of the release.

## 📃 Rebuilding the Listing

Whenever you make a change to a release - manually publishing it, or manually creating, editing or deleting a release, the [Build Repo Listing](.github/workflows/build-listing.yml) action will make a new index of all the releases available, and publish them as a website hosted fore free on [GitHub Pages](https://pages.github.com/). This listing can be used by the VPM to keep your package up to date, and the generated index page can serve as a simple landing page with info for your package. The URL for your package will be in the format `https://username.github.io/repo-name`.

## 🏠 Customizing the Landing Page (Optional)

The action which rebuilds the listing also publishes a landing page. The source for this page is in `Website/index.html`. The automation system uses [Scriban](https://github.com/scriban/scriban) to fill in the objects like `{{ this }}` with information from the latest release's manifest, so it will stay up-to-date with the name, id and description that you provide there. You are welcome to modify this page however you want - just use the existing `{{ template.objects }}` to fill in that info wherever you like. The entire contents of your "Website" folder are published to your GitHub Page each time.

## 💻 Technical Stuff

You are welcome to make your own changes to the automation process to make it fit your needs, and you can create Pull Requests if you have some changes you think we should adopt. Here's some more info on the included automation:

### Build Release Action
[release.yml](/.github/workflows/release.yml)

This is a composite action combining a variety of existing GitHub Actions and some shell commands to create both a .zip of your Package and a .unitypackage. It creates a release which is named for the `version` in the `package.json` file found in your target Package, and publishes the zip, the unitypackage and the package.json file to this release.

### Build Repo Listing
[build-listing.yml](.github/workflows/build-listing.yml)

This is a composite action which builds a vpm-compatible [Repo Listing](https://vcc.docs.vrchat.com/vpm/repos) based on the releases you've created. In order to find all your releases and combine them into a listing, it checks out [another repository](https://github.com/vrchat-community/package-list-action) which has a [Nuke](https://nuke.build/) project which includes the VPM core lib to have access to its types and methods. This project will be expanded to include more functionality in the future - for now, the action just calls its `BuildRepoListing` target.
