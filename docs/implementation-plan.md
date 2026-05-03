# Photo Album Manager — Claude Code Implementation Plan

## How to Use This Plan

Work through phases in order. At the end of each phase, run the stated **checkpoint** verification before proceeding. Do not begin the next phase until the checkpoint passes. Each phase builds on the last — skipping ahead will create integration problems.

Where instructions say "do not implement X yet", treat that as a hard constraint for that phase.

---

## Project Structure (Target)

```
PhotoManager/
├── PhotoManager.sln
├── PhotoManager/
│   ├── PhotoManager.csproj          (.NET 10, WinForms)
│   ├── Program.cs
│   ├── MainForm.cs
│   ├── MainForm.Designer.cs
│   ├── Settings/
│   │   ├── AppSettings.cs
│   │   └── SettingsService.cs
│   ├── Models/
│   │   ├── ImageFile.cs
│   │   └── SortOptions.cs
│   ├── Services/
│   │   ├── FolderScanService.cs
│   │   ├── FileOperationService.cs
│   │   └── ImageLoadService.cs
│   ├── Controls/
│   │   ├── FolderTreePanel.cs
│   │   ├── FolderTreePanel.Designer.cs
│   │   ├── FileListPanel.cs
│   │   ├── FileListPanel.Designer.cs
│   │   ├── PreviewPanel.cs
│   │   └── PreviewPanel.Designer.cs
│   └── Helpers/
│       ├── FileHelper.cs
│       └── ImageFormatHelper.cs
└── PhotoManager.Tests/
    ├── PhotoManager.Tests.csproj    (xUnit)
    ├── Services/
    │   ├── FolderScanServiceTests.cs
    │   ├── FileOperationServiceTests.cs
    │   └── ImageFormatHelperTests.cs
    └── Helpers/
        └── TempFolderFixture.cs
```

---

## Phase 1 — Solution Scaffold and Core Models ✅ COMPLETE

### Goal
Create a compiling solution with the correct project structure, NuGet references, and domain models. No UI beyond a blank form.

### Tasks

1. Create the solution and projects:
   ```
   dotnet new sln -n PhotoManager
   dotnet new winforms -n PhotoManager -f net10.0-windows
   dotnet new xunit -n PhotoManager.Tests
   dotnet sln add PhotoManager/PhotoManager.csproj
   dotnet sln add PhotoManager.Tests/PhotoManager.Tests.csproj
   dotnet add PhotoManager.Tests/PhotoManager.Tests.csproj reference PhotoManager/PhotoManager.csproj
   ```

2. Add NuGet packages to `PhotoManager`:
   - `Microsoft.Extensions.Configuration`
   - `Microsoft.Extensions.Configuration.Json`
   - `System.Text.Json`

3. Add NuGet packages to `PhotoManager.Tests`:
   - `xunit`
   - `xunit.runner.visualstudio`
   - `Microsoft.NET.Test.Sdk`

4. Create `Models/SortOptions.cs`:
   ```csharp
   public enum SortField { Name, DateCreated }
   public enum SortDirection { Ascending, Descending }

   public record SortOptions(SortField Field, SortDirection Direction)
   {
       public static SortOptions Default => new(SortField.Name, SortDirection.Ascending);
       public SortOptions Toggle() => Direction == SortDirection.Ascending
           ? this with { Direction = SortDirection.Descending }
           : this with { Direction = SortDirection.Ascending };
       public SortOptions WithField(SortField field) =>
           Field == field ? Toggle() : new(field, SortDirection.Ascending);
   }
   ```

5. Create `Models/ImageFile.cs`:
   ```csharp
   public record ImageFile(
       string FullPath,
       string RelativePath,   // relative to its tree root
       string FileName,
       DateTime DateCreated,
       long FileSizeBytes
   )
   {
       public string FormattedSize => FileSizeBytes >= 1_048_576
           ? $"{FileSizeBytes / 1_048_576.0:F1} MB"
           : $"{FileSizeBytes / 1024.0:F1} KB";

       public string FormattedDate => DateCreated.ToString("dd MMM yyyy HH:mm");
   }
   ```

6. Create `Helpers/ImageFormatHelper.cs`:
   ```csharp
   public static class ImageFormatHelper
   {
       private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
       {
           ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".heic", ".webp"
       };

       public static bool IsImageFile(string path) =>
           SupportedExtensions.Contains(Path.GetExtension(path));
   }
   ```

7. Create a blank `MainForm.cs` (just a resizable window, no controls yet):
   - Title: `Photo Album Manager`
   - Minimum size: 900 × 600
   - Start position: `CenterScreen`

### Checkpoint ✅
```
dotnet build
dotnet test
```
Both must pass with zero errors and zero test failures. The app must launch and show a blank window.

**Result:** Build succeeded (0 errors). 1/1 tests passed. Note: `System.Text.Json` explicit package reference removed — already included in net10.0-windows SDK.

---

## Phase 2 — Settings Service

### Goal
Implement persistence of user settings to `%APPDATA%\PhotoManager\appsettings.json`. No UI yet.

### Tasks

1. Create `Settings/AppSettings.cs`:
   ```csharp
   public class AppSettings
   {
       public string SourceFolderPath { get; set; } = string.Empty;
       public string TargetFolderPath { get; set; } = string.Empty;
       public SortField SortField { get; set; } = SortField.Name;
       public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
       public int SplitterPosition { get; set; } = 280;
   }
   ```

2. Create `Settings/SettingsService.cs` with methods:
   - `AppSettings Load()` — reads from JSON file; returns defaults if not found
   - `void Save(AppSettings settings)` — writes to JSON file, creating the directory if needed
   - The settings file path is `%APPDATA%\PhotoManager\appsettings.json`
   - Use `System.Text.Json` with `JsonSerializerOptions { WriteIndented = true }`

3. Wire up in `Program.cs`:
   - Instantiate `SettingsService`
   - Load settings on startup and pass to `MainForm`
   - Save settings when `MainForm` closes (`FormClosed` event)

4. Write unit tests in `PhotoManager.Tests/Services/` covering:
   - `Load()` returns defaults when file does not exist
   - `Save()` then `Load()` round-trips all fields correctly
   - `Load()` handles a corrupt/invalid JSON file gracefully (returns defaults)

### Checkpoint
```
dotnet test
```
All settings tests must pass. Manually run the app, close it, and verify `%APPDATA%\PhotoManager\appsettings.json` is created.

---

## Phase 3 — Folder Scan Service

### Goal
Implement the service that scans a folder tree and returns image files. No UI yet.

### Tasks

1. Create `Services/FolderScanService.cs` with the following methods:

   ```csharp
   // Returns all direct image files in a folder (not recursive)
   Task<IReadOnlyList<ImageFile>> GetFilesInFolderAsync(string folderPath, string rootPath)

   // Returns all immediate child directories that contain at least one image
   // file anywhere in their subtree
   Task<IReadOnlyList<string>> GetImageSubfoldersAsync(string folderPath)

   // Returns count of direct image files in a folder
   Task<int> GetImageCountAsync(string folderPath)
   ```

2. `GetFilesInFolderAsync` must:
   - Filter by supported extensions via `ImageFormatHelper.IsImageFile`
   - Populate `RelativePath` as the path relative to `rootPath`
   - Use `FileInfo` for `DateCreated` and `FileSizeBytes`
   - Run on a background thread (`Task.Run`)

3. `GetImageSubfoldersAsync` must:
   - Return only directories that contain at least one image file (directly or in any descendant)
   - Exclude any folder named `_removed` (case-insensitive) and all its descendants
   - Run on a background thread

4. Create `Helpers/TempFolderFixture.cs` in the test project — a test helper that creates and tears down a temporary folder tree:
   ```csharp
   public class TempFolderFixture : IDisposable
   {
       public string Root { get; }
       public TempFolderFixture() => Root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
       public string CreateFolder(string relativePath) { ... }
       public string CreateImageFile(string relativePath) { ... } // creates a 1x1 valid PNG
       public void Dispose() => Directory.Delete(Root, recursive: true);
   }
   ```

5. Write unit tests covering:
   - Empty folder returns empty list
   - Non-image files are excluded
   - `_removed` folder and its contents are excluded from subfolder results
   - `RelativePath` is correct relative to the root
   - `GetImageCountAsync` returns the correct count

### Checkpoint
```
dotnet test
```
All scan service tests must pass.

---

## Phase 4 — File Operation Service

### Goal
Implement the three file operations (copy, remove, undo) as a tested service. No UI yet.

### Tasks

1. Create `Services/FileOperationService.cs` with:

   ```csharp
   // Copy source file to target, replicating folder structure.
   // Returns false (no exception) if destination already exists.
   Task<bool> CopyToTargetAsync(ImageFile file, string sourceRoot, string targetRoot)

   // Move file from target to _removed, preserving relative path.
   Task MoveToRemovedAsync(ImageFile file, string targetRoot)

   // Move file from _removed back to target.
   // Returns false (no exception) if destination already exists.
   Task<bool> UndoRemoveAsync(ImageFile file, string targetRoot)
   ```

2. All methods must:
   - Create intermediate directories as needed
   - Run file I/O on a background thread
   - Propagate `IOException` and `UnauthorizedAccessException` to the caller (do not swallow)
   - The `_removed` root is always `Path.Combine(targetRoot, "_removed")`

3. Write unit tests using `TempFolderFixture` covering:
   - `CopyToTargetAsync` creates the file at the correct path with correct relative structure
   - `CopyToTargetAsync` returns `false` and leaves both files intact when destination exists
   - `MoveToRemovedAsync` moves the file to `_removed` with correct relative path; source no longer exists
   - `UndoRemoveAsync` moves the file back to the target; `_removed` copy no longer exists
   - `UndoRemoveAsync` returns `false` when target already exists

### Checkpoint
```
dotnet test
```
All operation service tests must pass.

---

## Phase 5 — Main Form Layout and Splitter

### Goal
Build the top-level `MainForm` layout: two panes separated by a splitter, with placeholder tab controls in each pane. No real content in the tabs yet.

### Tasks

1. In `MainForm.Designer.cs`, add:
   - A `SplitContainer` docked to fill the form
   - `Panel1` (left): a `TabControl` with three tabs labelled **Source**, **Target**, **Removed**
   - `Panel2` (right): a `TabControl` with two tabs labelled **File List**, **Preview**
   - A `StatusStrip` at the bottom with one `ToolStripStatusLabel` (used later for folder summaries)

2. In `MainForm.cs`:
   - On load, restore `SplitterPosition` from settings
   - On `SplitterMoved`, update the in-memory settings object (save on close)

3. Each left-pane tab page contains for now:
   - A `Button` labelled "Select Folder…" at the top (Source and Target tabs only; Removed tab has a `Label` saying "Derived from Target folder")
   - A `Label` showing the current path (empty by default)
   - A placeholder `Label` in the centre: "No folder selected"

4. Each right-pane tab page contains:
   - A placeholder `Label` in the centre

5. The **Select Folder…** buttons must open a `FolderBrowserDialog` and update the path label and the settings object. No tree loading yet.

6. On startup, if a saved path exists, populate the path labels. Do not load trees yet.

### Checkpoint
- `dotnet build` passes with no warnings
- Launch the app manually and verify:
  - Splitter is draggable
  - Source and Target tabs each have a working folder picker that updates the label
  - Removed tab shows the derived-from label
  - Window remembers splitter position between runs

---

## Phase 6 — Folder Tree Panel (Left Pane)

### Goal
Implement `FolderTreePanel` — a reusable user control containing the full recursive `TreeView` with lazy loading and folder image counts.

### Tasks

1. Create `Controls/FolderTreePanel.cs` as a `UserControl` containing:
   - A `TreeView` docked to fill the control, with checkboxes disabled
   - Lazy loading: on first expand of a node, call `FolderScanService.GetImageSubfoldersAsync` to populate children
   - Root node is populated when `LoadRootAsync(string path, string rootPath)` is called
   - Each node's `Tag` stores the full folder path
   - A `SelectedFolderChanged` event (`EventArgs<string>`) fires when a node is selected

2. Root node behaviour:
   - Show the folder name as the node text
   - Immediately load first-level children on `LoadRootAsync`
   - Show a dummy child node ("Loading…") for nodes that have not yet been expanded

3. On node selection:
   - Call `FolderScanService.GetImageCountAsync` for the selected folder
   - Update the `StatusStrip` label on `MainForm` with `"{n} image(s)"`

4. Expose a public method `RemoveFileNode` — not needed yet, but stub it for Phase 8.

5. Expose a public method `RefreshNodeAsync(string folderPath)` — re-scans a folder's children and updates the tree. Stub for now.

6. Replace the placeholder content in each left-pane tab with an instance of `FolderTreePanel`.

7. Wire `SelectedFolderChanged` from each `FolderTreePanel` to a handler on `MainForm` that will (for now) just update the status strip. Right-pane population comes in Phase 7.

8. On startup, if a saved Source or Target path is valid, call `LoadRootAsync` automatically.

### Checkpoint
- `dotnet build` passes
- Launch the app, select a source folder containing nested image subfolders, and verify:
  - Tree populates with only image-containing folders
  - `_removed` does not appear in the Target tree
  - Expanding nodes lazy-loads children correctly
  - Selecting a node updates the status strip count
  - Tree loads automatically on relaunch if paths were saved

---

## Phase 7 — File List Panel (Right Pane)

### Goal
Implement `FileListPanel` — a user control showing the image files in the selected folder as a sortable list.

### Tasks

1. Create `Controls/FileListPanel.cs` as a `UserControl` containing:
   - A `ToolStrip` at the top with two toggle buttons: **Sort by Name** and **Sort by Date**
   - A `ListView` in Details view with columns: **Name**, **Date**, **Size**
   - Column widths: Name stretches to fill; Date and Size are fixed (~140px and ~80px)

2. Expose:
   - `async Task LoadFolderAsync(string folderPath, string rootPath, SortOptions sort)` — scans and populates the list
   - `SortOptions CurrentSort` property
   - `ImageFile? SelectedFile` property
   - `event EventHandler<ImageFile> FileSelected` — fires on single-click selection
   - `event EventHandler<ImageFile> FileDoubleClicked` — fires on double-click

3. Sort toggle behaviour:
   - Clicking the active sort button reverses direction (shown with ▲ / ▼ suffix on button text)
   - Clicking the inactive sort button activates it ascending
   - Re-sort is applied in-memory without re-scanning the folder

4. Replace the placeholder in the **File List** right-pane tab with `FileListPanel`.

5. On `MainForm`, wire `SelectedFolderChanged` from each `FolderTreePanel` to call `FileListPanel.LoadFolderAsync` with the correct root path and the current sort from settings.

6. Track which left-pane tab is active (Source / Target / Removed) so the correct context is passed to the right pane in later phases.

7. Persist sort changes to the in-memory settings object (saved on close).

### Checkpoint
- `dotnet build` passes
- Launch the app, select a folder in the Source tree, and verify:
  - Images appear in the file list with correct Name, Date, and Size values
  - Sorting by Name and Date works correctly in both directions
  - Switching folders updates the list
  - Switching left-pane tabs updates the list to reflect the selected folder on that tab

---

## Phase 8 — Preview Panel (Right Pane)

### Goal
Implement `PreviewPanel` — a user control showing the full image preview with navigation and action buttons.

### Tasks

1. Create `Controls/PreviewPanel.cs` as a `UserControl` containing:
   - A `PictureBox` (or custom drawn panel) that displays the image scaled to fit, maintaining aspect ratio, anchored to fill the available space
   - A label above showing `{filename}  —  {formatted date}`
   - A position indicator label: `{n} / {total}`
   - Navigation buttons: **◀ Prev** and **Next ▶**
   - A **Sort** toggle button: cycles `Name ↑` → `Name ↓` → `Date ↑` → `Date ↓`
   - An action button area (bottom): one button whose label and handler change based on context

2. Expose:
   - `async Task LoadFolderAsync(IReadOnlyList<ImageFile> files, int selectedIndex, SortOptions sort, PreviewContext context)` — loads the image list and shows the file at `selectedIndex`
   - `enum PreviewContext { Source, Target, Removed }`
   - `event EventHandler SortChanged` — fires when sort is toggled; `MainForm` updates both panels

3. Image loading (`Services/ImageLoadService.cs`):
   - Load images asynchronously on a background thread
   - Scale the bitmap to the display size before assigning to the `PictureBox` to avoid loading full-resolution bitmaps into memory
   - Show a "Loading…" placeholder while loading
   - On decode failure, show "Cannot preview this image"
   - For HEIC files that cannot be decoded, show "HEIC preview requires Windows HEIC codec"

4. Action button behaviour by context:

   | Context | Button Label | Action |
   |---|---|---|
   | Source | Copy to Target | Call `FileOperationService.CopyToTargetAsync`; on success, remove file from list and advance |
   | Target | Remove | Call `FileOperationService.MoveToRemovedAsync`; on success, remove file from list and advance |
   | Removed | Undo | Call `FileOperationService.UndoRemoveAsync`; on success, remove file from list and advance |

5. After any action:
   - Remove the file from the in-memory list
   - Advance to the next file (wrap to first if at end; show blank state if list is empty)
   - Fire a `FileActioned` event carrying the `ImageFile` that was acted on, so `MainForm` can update the `FileListPanel` and `FolderTreePanel`

6. On `MainForm`:
   - Wire `FileListPanel.FileDoubleClicked` → switch to Preview tab and call `PreviewPanel.LoadFolderAsync`
   - Wire `FileListPanel.FileSelected` → if Preview tab is already active, call `PreviewPanel.LoadFolderAsync`
   - Wire `PreviewPanel.SortChanged` → update `FileListPanel` sort and in-memory settings
   - Wire `PreviewPanel.FileActioned`:
     - Refresh `FileListPanel`
     - Call `FolderTreePanel.RemoveFileNode` or `RefreshNodeAsync` for the affected folder

7. The action button must be disabled if no file is loaded.

### Checkpoint
- `dotnet build` passes
- Launch the app and verify:
  - Double-clicking a file in the file list switches to Preview tab and shows the image
  - Prev / Next navigation works and wraps correctly
  - Position indicator is correct
  - Sort toggle on preview tab updates both preview order and file list order
  - Copy (Source context): file disappears from source list; file appears in target folder on disk
  - Remove (Target context): file moves to `_removed` on disk; disappears from target list
  - Undo (Removed context): file moves back to target on disk; disappears from removed list
  - After each action, preview advances to next file

---

## Phase 9 — Error Handling and Edge Cases

### Goal
Harden the application against real-world failure conditions.

### Tasks

1. **Path no longer valid on startup:**
   - If a saved source or target path does not exist, show a non-blocking warning `Label` on the relevant tab ("Folder not found — please select again") instead of loading the tree. Do not throw.

2. **File operation failures:**
   - Wrap all action button handlers in try/catch for `IOException` and `UnauthorizedAccessException`
   - Show a `MessageBox.Show(errorMessage, "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning)`
   - Do not remove the file from the UI if the operation failed

3. **Copy skip (duplicate):**
   - When `CopyToTargetAsync` returns `false`, show a brief non-modal status message: "File already exists in target — skipped." (update the status strip label for 3 seconds, then clear)

4. **Undo skip (duplicate):**
   - Same treatment as copy skip

5. **Empty folder after action:**
   - After an action empties a folder, remove that folder node from the tree if it now contains no images
   - Call `FolderScanService.GetImageSubfoldersAsync` to verify before removing

6. **Target folder not selected when Copy is attempted:**
   - Disable the **Copy to Target** button if no target root has been set
   - Show tooltip: "Select a target folder first"

7. **Large image performance:**
   - In `ImageLoadService`, scale the image to `PictureBox` display dimensions before setting it. Dispose the previous bitmap before loading the next.

8. **Concurrent operations:**
   - Disable the action button during an in-progress file operation to prevent double-clicks
   - Re-enable on completion (success or failure)

### Checkpoint
- `dotnet build` passes
- Manually test the following scenarios:
  - [ ] Launch with a source path that has been deleted → warning label shown, no crash
  - [ ] Attempt to copy a file that already exists in target → status strip shows skip message
  - [ ] Attempt to copy when no target is set → Copy button is disabled
  - [ ] Rapidly click Copy → second click is ignored while first is in progress
  - [ ] Navigate a folder with 1 image, act on it → blank preview state, no crash

---

## Phase 10 — Polish and Final Wiring

### Goal
Final UX polish, keyboard shortcuts, and release readiness.

### Tasks

1. **Keyboard shortcuts:**
   - `Left` / `Right` arrow keys navigate Prev / Next in Preview tab (when preview has focus)
   - `C` triggers Copy, `R` triggers Remove, `U` triggers Undo (when action button is enabled)
   - `F2` switches right pane between File List and Preview tabs

2. **Column resize persistence:**
   - Save the `Name` column width to settings and restore on startup

3. **Window size and position persistence:**
   - Save `WindowState`, `Width`, `Height`, `Left`, `Top` to settings
   - Restore on startup; if `WindowState` is `Minimized`, open as `Normal`

4. **Application icon:**
   - Set a simple icon on `MainForm` (use a stock Windows shell icon if no custom asset is available)

5. **About box (optional but recommended):**
   - `Help → About` menu item showing app name and .NET version

6. **Final settings fields** — add to `AppSettings` and `SettingsService`:
   - `NameColumnWidth` (int, default 300)
   - `WindowState`, `WindowWidth`, `WindowHeight`, `WindowLeft`, `WindowTop`

7. **Code review pass:**
   - Ensure all `IDisposable` objects (bitmaps, file streams) are disposed correctly
   - Ensure no `async void` methods except WinForms event handlers
   - Ensure all background work uses `await Task.Run(...)` not `Task.Factory.StartNew`
   - Remove any TODO or placeholder comments

8. **Final test run:**
   ```
   dotnet test
   ```
   All tests must pass.

### Final Manual Checklist
- [ ] App launches cleanly with no console errors
- [ ] Source and target paths persist across restarts
- [ ] Sort preference persists across restarts
- [ ] Splitter position persists across restarts
- [ ] Window size and position persist across restarts
- [ ] All three operations (copy, remove, undo) work end-to-end
- [ ] File list and tree stay in sync after every operation
- [ ] No UI freezes during folder scan or image load
- [ ] Error dialogs appear correctly for failed operations
- [ ] Keyboard shortcuts work in preview mode

---

## Known Risks and Mitigations

| Risk | Mitigation |
|---|---|
| HEIC files cannot be decoded without Windows codec | Detect decode failure and show a specific message rather than a generic error |
| Large RAW-adjacent files (TIFF) cause memory spikes | Always scale images to display resolution before assigning to PictureBox; dispose previous bitmap |
| `_removed` folder appearing in target tree | `GetImageSubfoldersAsync` explicitly excludes any folder named `_removed` case-insensitively |
| Tree and file list going out of sync after an operation | All operations fire `FileActioned` event; `MainForm` is the single coordinator that updates both panels |
| WinForms designer corrupting `Designer.cs` | Keep all layout logic in `Designer.cs`; keep all behaviour logic in the non-designer partial class |
| Concurrent file operations (double-click) | Disable action button during in-progress operation; re-enable on completion |