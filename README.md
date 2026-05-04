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
- **Album view** — full-screen thumbnail grid of the target folder with large preview alongside; navigate and remove without leaving the view
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

Album view (F3 or "View Album" button on the Target tab):

```
┌──────────────────────┬──────────────────────────────────────────┐
│  [thumb] [thumb] ... │  Large preview                           │
│  [thumb] [thumb] ... │                                          │
│  [thumb] [thumb] ... │  filename — date              [Remove]   │
└──────────────────────┴──────────────────────────────────────────┘
```

## Keyboard shortcuts

### Main window

| Key | Action |
|-----|--------|
| `F2` | Toggle File List / Preview tab |
| `F3` | Open Album view |
| `←` / `→` | Previous / next image (Preview tab) |
| `C` | Copy to target (Source tab, Preview tab) |
| `R` | Remove to _removed (Target tab, Preview tab) |
| `U` | Undo removal (Removed tab, Preview tab) |

### Album view

| Key | Action |
|-----|--------|
| `←` `↑` | Previous image |
| `→` `↓` | Next image |
| `Delete` | Remove selected image |

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
