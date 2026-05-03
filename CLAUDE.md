# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```powershell
dotnet build                          # build solution
dotnet test                           # run all tests
dotnet test --filter "ClassName"      # run single test class
dotnet run --project PhotoManager     # launch the app
```

## Project Structure

```
PhotoManager/
├── PhotoManager.sln
├── PhotoManager/              (.NET 10 WinForms app)
│   ├── MainForm.cs / MainForm.Designer.cs
│   ├── Program.cs
│   ├── Models/                ImageFile.cs, SortOptions.cs  ✅
│   ├── Helpers/               ImageFormatHelper.cs  ✅
│   ├── Settings/              AppSettings.cs, SettingsService.cs  ✅
│   ├── Services/              FolderScanService.cs ✅, FileOperationService.cs ✅, ImageLoadService  (Phase 8)
│   └── Controls/              FolderTreePanel.cs ✅, FileListPanel.cs ✅, PreviewPanel  (Phase 8)
└── PhotoManager.Tests/        (xUnit, net10.0-windows)
```

Note: `System.Text.Json` is not an explicit package reference — it is included in the net10.0-windows SDK.

## Architecture

**Two-pane WinForms layout.** `SplitContainer` divides left (folder tree) from right (file list + preview).

**Left pane** — three `TabControl` tabs: Source, Target, Removed. Each tab hosts a `FolderTreePanel` UserControl with a lazy-loading `TreeView`. Only image-containing folders appear. `_removed` is always excluded from the Target tree.

**Right pane** — two tabs: File List (`FileListPanel`) and Preview (`PreviewPanel`). Both reflect whichever folder node is selected in the active left-pane tab.

**Event flow:** `FolderTreePanel.SelectedFolderChanged` → `MainForm` coordinator → `FileListPanel.LoadFolderAsync`. `FileListPanel.FileDoubleClicked` → switch to Preview tab → `PreviewPanel.LoadFolderAsync`. `PreviewPanel.FileActioned` → `MainForm` updates both `FileListPanel` and `FolderTreePanel`.

**`MainForm` is the single coordinator.** It owns settings, wires all cross-panel events, and keeps the tree and file list in sync after every operation.

**Three file operations:**
- Copy (Source→Target): replicates relative path; silently skips if destination exists; source file stays on disk but is removed from in-memory view.
- Remove (Target→`_removed`): moves file preserving relative path under `<targetRoot>\_removed\`.
- Undo (`_removed`→Target): reverses remove; silently skips if destination exists.

**Settings** persist to `%APPDATA%\PhotoManager\appsettings.json` via `System.Text.Json`. Loaded at startup, saved on `FormClosed`.

**Background threading:** all file I/O and folder scans use `await Task.Run(...)`. No `async void` except WinForms event handlers. Images are scaled to display dimensions before assignment to `PictureBox`; previous bitmap disposed before loading next.

**Supported formats:** `.jpg .jpeg .png .gif .bmp .tiff .tif .heic .webp` (via `ImageFormatHelper.IsImageFile`, case-insensitive). HEIC decode failures show a specific message rather than a generic error.

## Implementation Phases

Work through `docs/implementation-plan.md` in order. Each phase has a checkpoint (`dotnet build` / `dotnet test` / manual verification). Do not begin the next phase until the checkpoint passes.

| Phase | Goal |
|---|---|
| 1 | Solution scaffold, models, helpers ✅ |
| 2 | Settings service ✅ |
| 3 | FolderScanService ✅ |
| 4 | FileOperationService ✅ |
| 5 | MainForm layout and splitter ✅ |
| 6 | FolderTreePanel ✅ |
| 7 | FileListPanel ✅ |
| 8 | PreviewPanel + ImageLoadService |
| 9 | Error handling and edge cases |
| 10 | Polish, keyboard shortcuts, window persistence |

## Key Constraints

- `_removed` folder must never appear as a selectable node in the Target tree.
- Keep all layout logic in `Designer.cs`; all behaviour in the non-designer partial class. WinForms designer corrupts files if this boundary is violated.
- Action button must be disabled during in-progress file operations (prevent double-click).
- `Copy to Target` button disabled when no target root is set.
- After any operation that empties a folder, remove that folder node from the tree.
