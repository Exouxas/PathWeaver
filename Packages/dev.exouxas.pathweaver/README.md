# PathWeaver

Your GameObject hierarchy becomes your VRChat expression menu.

Put a **PathWeaver** component on a root object (for example `Menus`). At upload time, every VRCFury Toggle underneath gets its Menu Path rewritten from the object tree:

```
Menus            (PathWeaver here)
  Clothing
    Hoodie       Toggle "Hoodie"        -> Clothing/Hoodie
    Colors
      Hue        Toggle "Hue"           -> Clothing/Colors/Hue
  Movement
    GogoLoco     Full Controller prefix -> Movement
```

Only the last segment of an existing path is kept as the item name, so existing toggles keep working. Everything happens on the build copy of the avatar; your scene is never changed.

## Install

Add the VPM listing `https://exouxas.github.io/PathWeaver/index.json` in the Creator Companion, then add PathWeaver to your project.

## Use

1. Add **PathWeaver** to the root of your menu objects.
2. Nest objects to form folders. Objects holding toggles count as folders too.
3. Right-click the component header: **Preview Generated Paths** logs every rewrite without changing anything, **Dump Feature Fields** lists VRCFury feature types and fields for writing new rules.

## Rules

Each rule maps a VRCFury feature type to the field holding its path:

| Feature type | Field | Mode |
|---|---|---|
| VF.Model.Feature.Toggle | name | Folder + leaf |
| VF.Model.Feature.FullController | menus.prefix | Folder only |

Use dots to step into lists. Add rules from the Inspector.

## Requirements

VRChat Avatars SDK 3.5+, VRCFury.
