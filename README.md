# Photo Album Manager

Windows desktop app for curating photos across a source and target folder. Browse, preview, copy, remove, and undo — all without leaving the app.

## Download

Grab the latest `PhotoAlbumManager.exe` from [Releases](../../releases/latest). No installer needed — just run it.

**Requires:** Windows 11

## What it does

- **Browse** source and target folders in side-by-side tree views
- **Preview** images with next/previous navigation
- **Copy** from source to target, preserving folder structure
- **Remove** from target to a `_removed` subfolder
- **Undo** removals, restoring files to their original location
- Remembers your folders, sort order, and window layout between sessions

**Supported formats:** JPG, PNG, GIF, BMP, TIFF, WEBP, HEIC

## Layout

```
┌─────────────────────────┬──────────────────────────────────────┐
│  [Source][Target][Rmvd] │  [File List] [Preview]               │
│                         │                                      │
│  Folder tree            │  Files or image preview for          │
│                         │  selected folder                     │
└─────────────────────────┴──────────────────────────────────────┘
```

## Keyboard shortcuts

| Key | Action |
|-----|--------|
| `←` / `→` | Previous / next image |
| `C` | Copy to target |
| `Delete` | Remove to _removed |
| `U` | Undo removal |

## Building from source

```powershell
dotnet build
dotnet test
dotnet run --project PhotoManager
```

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

## Releasing

```powershell
.\release.ps1           # auto-increments patch version
.\release.ps1 -Tag v2.0.0  # explicit tag
```

Pushing a `v*` tag triggers the GitHub Actions workflow, which builds a self-contained `PhotoAlbumManager.exe` and publishes it as a GitHub release.
