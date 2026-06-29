#if TOOLS
using Godot;
using Godot.Collections;
using System;

namespace Odezzshuuk.Editor.SelectionTracker;

[Tool]
public partial class ContainerControl : Control {

  [Export]
  private PackedScene _entryTemplate;

  private readonly int _sizeLimit = 100;
  private int _currentSelectionIndex = -1;

  // public override void _EnterTree() {
  //   if (GetTree().EditedSceneRoot != null) {
  //     return;
  //   }
  // }

  public override void _Ready() {
    EditorInterface.Singleton.GetSelection().SelectionChanged += NodeSelectionChangedCallback;
    EditorInterface.Singleton.GetFileSystemDock().SelectionChanged += FileSystemSelectionChangedCallback;
  }

  #region entry management
  public void RecordEntry(EntryModel entry) {
    if (_currentSelectionIndex > 0 &&
        GetChildCount() > _currentSelectionIndex &&
        entry.Equals(GetChildren()[_currentSelectionIndex].GetNode<EntryControl>(".").Entry)) {
      return;
    }

    int existingIndex = FindEntryIndex(entry);
    if (existingIndex != -1) {
      RemoveChild(GetChildren()[existingIndex]);
    }

    Node entryNode = _entryTemplate.Instantiate();
    EntryControl entryControl = entryNode.GetNode<EntryControl>(".");
    entryControl.Entry = entry;

    AddChild(entryNode);
    MoveChild(entryNode, 0);
    entryNode.Owner = this;

    while (GetChildren().Count > _sizeLimit) {
      // remove the last
      RemoveChild(GetChildren()[GetChildren().Count - 1]);
    }

    ResetCurrentSelection();
  }

  public void RemoveEntry(EntryModel entry) {
    int existingIndex = FindEntryIndex(entry);
    if (existingIndex < 0) {
      return;
    }

    RemoveChild(GetChildren()[existingIndex]);
  }

  public void RemoveAll() {

    foreach (Node child in GetChildren()) {
      child.QueueFree();
    }

    ResetCurrentSelection();
  }

  public void RemoveAll(string filter) {
    ResetCurrentSelection();
  }

  public Node PreviousSelection() {
    if (GetChildren().Count == 0) {
      return null;
    }

    _currentSelectionIndex--;
    if (_currentSelectionIndex < 0) {
      _currentSelectionIndex = 0;
    }

    return GetChildren()[_currentSelectionIndex];
  }

  public Node NextSelection() {
    if (GetChildren().Count == 0) {
      return null;
    }

    _currentSelectionIndex++;
    if (_currentSelectionIndex >= GetChildren().Count) {
      _currentSelectionIndex = GetChildren().Count - 1;
    }

    return GetChildren()[_currentSelectionIndex];
  }

  public void ResetCurrentSelection() {
    _currentSelectionIndex = -1;
  }

  private int FindEntryIndex<T>(IEquatable<T> entry) {
    for (int index = 0; index < GetChildren().Count; index++) {
      if (GetChildren()[index]?.GetNode<EntryControl>(".").Entry?.Equals(entry) == true) {
        return index;
      }
    }

    return -1;
  }
  #endregion

  private EntryModel CreateNodeEntry(Node node) {
    return new NodeEntryModel(node);
  }

  private EntryModel CreateFileEntry(string path) {
    return new FileEntryModel(path);
  }

  private void NodeSelectionChangedCallback() {
    Array<Node> selectedNodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
    if (selectedNodes.Count > 0) {
      RecordEntry(CreateNodeEntry(selectedNodes[0]));
    }
  }

  private void FileSystemSelectionChangedCallback() {
    string[] selectedPaths = EditorInterface.Singleton.GetSelectedPaths();
    if (selectedPaths.Length > 0) {
      string path = selectedPaths[0];
      if (!path.Trim().EndsWith("/")) {
        RecordEntry(CreateFileEntry(path));
      }
    }
  }

}
#endif
