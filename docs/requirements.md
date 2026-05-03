# Photo Manager — Requirements

## 1. Overview

Photo Manager is a .NET 10 Windows Forms desktop application for reviewing, curating, and organising image files across a source folder and a target folder. The user can copy images from source to target, remove images from target to a reserved `_removed` subfolder, and undo removals. All operations preserve folder structure relative to the root of each tree.

---

## 2. Technology

| Concern | Choice |
|---|---|
| Framework | .NET 10 |
| UI | Windows Forms (WinForms) |
| Target OS | Windows 11 |
| Persistence | `appsettings.json` via `System.Text.Json` (user settings) |
| Image decoding | `System.Drawing` / `ImageMagick.NET` for HEIC support |

---

## 3. Supported Image Formats

The application recognises and processes the following file extensions (case-insensitive):

`.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.tiff`, `.tif`, `.heic`, `.webp`

Non-image files are ignored throughout — they will not appear in any list, tree count, or preview.

---

## 4. Application Layout

The main window is divided into two resizable panes arranged side by side:

```
┌─────────────────────────┬──────────────────────────────────────┐
│   LEFT PANE             │   RIGHT PANE                         │
│   Folder Tree Tabs      │   File / Preview Tabs                │
│                         │                                      │
│  [Source][Target][Rmvd] │  [File List] [Preview]               │
│                         │                                      │
│  TreeView of folders    │  File list or image preview          │
│                         │  for the folder selected on left     │
└─────────────────────────┴──────────────────────────────────────┘
```

A splitter between the two panes allows the user to adjust the width of each.

---

## 5. Left Pane — Folder Tree Tabs

### 5.1 Tabs

The left pane contains three tabs:

| Tab | Label | Root |
|---|---|---|
| 1 | Source | User-selected source folder |
| 2 | Target | User-selected target folder |
| 3 | Removed | `<target root>\_removed\` |

### 5.2 Folder Selection (Source and Target tabs only)

- Each tab (Source and Target) has a **"Select Folder…"** button at the top.
- Clicking it opens a `FolderBrowserDialog`.
- The selected path is displayed beneath the button and persisted to settings (see §10).
- The Removed tab derives its root automatically from the Target path; it has no select button.

### 5.3 Tree View

- Each tab shows a `TreeView` control populated with the full recursive folder hierarchy under its root.
- Only folders that contain at least one image file (directly or in any descendant) are shown.
- Empty folders (image-wise) are omitted from the tree.
- The tree is lazy-loaded: child nodes are expanded on demand.
- Selecting a folder node in the tree populates the right pane with that folder's contents.
- The currently active tab (Source / Target / Removed) determines which context the right pane operates in.

### 5.4 Folder Summary

When a folder node is selected, a status strip or label beneath the tree displays:

```
{n} image(s)
```

This count reflects only the direct contents of the selected folder, not descendants.

---

## 6. Right Pane — File List and Preview Tabs

The right pane contains two tabs: **File List** and **Preview**. Both tabs reflect the folder currently selected in the left pane.

### 6.1 File List Tab

#### Display

Files are shown in a `ListView` in Details view with the following columns:

| Column | Content |
|---|---|
| Name | Filename (with extension) |
| Date | File created date (formatted as `dd MMM yyyy HH:mm`) |
| Size | File size, formatted as KB or MB as appropriate |

#### Sorting

- A toolbar or header above the list provides **Sort by Name** and **Sort by Date Created** options (toggle buttons or a dropdown).
- The active sort is highlighted.
- Default sort is by name, ascending.
- Clicking an already-active sort reverses the direction (ascending ↔ descending).

#### Selection

- Clicking a row selects that file.
- Double-clicking a row, or single-clicking with the Preview tab already active, switches the right pane to the **Preview** tab and loads that image.

---

### 6.2 Preview Tab

#### Image Display

- The selected image is displayed scaled to fit the available area, maintaining aspect ratio.
- A filename label appears above or below the image showing the file name and created date.

#### Navigation

- **◀ Previous** and **Next ▶** buttons navigate through all image files in the currently selected folder, in the current sort order.
- The sort order used for navigation matches the sort order active in the File List tab, and can also be toggled directly on the Preview tab via a **Sort** button (cycles Name ↔ Date).
- Navigation wraps at the ends (last file → first, first file → last).
- A position indicator is shown, e.g. `3 / 14`.

#### Action Buttons

The action buttons shown depend on the active left-pane tab:

| Active Tab | Button | Behaviour |
|---|---|---|
| Source | **Copy to Target** | Copies the file to the target folder, replicating folder structure (see §7). The file is removed from the source tree and file list immediately after a successful copy. |
| Target | **Remove** | Moves the file to `<target root>\_removed\<relative path>` (see §8). The file disappears from the Target tree immediately. |
| Removed | **Undo** | Moves the file back from `_removed` to its original location in the target folder (see §9). The file disappears from the Removed tree immediately. |

All action buttons are disabled if no file is currently selected.

After an action, the preview automatically advances to the next file in the folder (if one exists), or returns to a blank state if the folder is now empty.

---

## 7. Copy Operation (Source → Target)

### Rules

- The relative path of the file beneath the source root is replicated beneath the target root.
  - Example: `<source>\2024\Holiday\photo.jpg` → `<target>\2024\Holiday\photo.jpg`
- Intermediate directories in the target are created automatically if they do not exist.
- **If the destination file already exists, the copy is silently skipped.** No error is shown; the file is treated as already present.
- The original source file is **not deleted**; it is only removed from the application's in-memory view of the source tree (the source file remains on disk).

### Post-copy UI

- The file is removed from the Source file list and tree counts immediately.
- If the folder the file was in becomes image-empty after the operation, that folder node is removed from the Source tree.
- The preview advances to the next file.

---

## 8. Remove Operation (Target → _removed)

### Rules

- The file is **moved** (not copied) to `<target root>\_removed\<relative path from target root>`.
  - Example: `<target>\2024\Holiday\photo.jpg` → `<target>\_removed\2024\Holiday\photo.jpg`
- Intermediate directories under `_removed` are created automatically.
- The `_removed` folder itself is excluded from the Target tree view.

### Post-remove UI

- The file disappears from the Target file list and tree immediately.
- The Removed tab's tree is updated to include the newly moved file.
- The preview advances to the next file.

---

## 9. Undo Operation (_removed → Target)

### Rules

- The file is **moved** back from `<target root>\_removed\<relative path>` to `<target root>\<relative path>`.
- Intermediate directories in the target are created if they do not exist.
- If a file with the same name already exists in the target destination, the undo is **skipped silently** (consistent with the copy behaviour).

### Post-undo UI

- The file disappears from the Removed file list and tree immediately.
- The Target tab's tree is updated to reflect the restored file.
- The preview advances to the next file.

---

## 10. Settings Persistence

The application saves and restores the following settings between sessions using a local `appsettings.json` file stored in the user's application data folder (`%APPDATA%\PhotoManager\`):

| Setting | Description |
|---|---|
| `SourceFolderPath` | Last selected source root folder |
| `TargetFolderPath` | Last selected target root folder |
| `SortField` | Last active sort field (`Name` or `Date`) |
| `SortDirection` | Last active sort direction (`Ascending` or `Descending`) |
| `SplitterPosition` | Left/right pane splitter position in pixels |

On startup, if persisted paths exist and are still accessible, the trees are loaded automatically.

---

## 11. Error Handling

| Scenario | Behaviour |
|---|---|
| Source or target path no longer exists on startup | Show a non-blocking warning label; allow the user to re-select |
| Copy or move fails (permissions, disk full, etc.) | Show a `MessageBox` with the error message; do not remove the file from the UI |
| Image cannot be decoded for preview | Display a placeholder "Cannot preview this image" message in the preview area |
| HEIC format not supported on current system | Display a placeholder with a note that HEIC preview requires Windows codec support |

---

## 12. Non-Functional Requirements

| Requirement | Detail |
|---|---|
| Responsiveness | Folder tree loading and image preview must not block the UI thread. Use `async/await` with background tasks. |
| Image memory | Large images must be loaded at display resolution, not full resolution, to avoid excessive memory use. |
| Consistency | The `_removed` folder must never appear as a selectable node in the Target tree. |
| Accessibility | Standard WinForms tab order and keyboard navigation must be functional throughout. |

---

## 13. Out of Scope

The following are explicitly out of scope for this version:

- Renaming files
- Editing image metadata (EXIF)
- Batch copy or batch remove operations
- Undo history beyond the single Undo button on the Removed tab
- Drag-and-drop between panes
- Any cloud or network storage integration