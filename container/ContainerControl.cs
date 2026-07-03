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

  public override async void _Ready() {
    EditorInterface.Singleton.GetSelection().SelectionChanged += NodeSelectionChangedCallback;
    EditorInterface.Singleton.GetFileSystemDock().SelectionChanged += FileSystemSelectionChangedCallback;
  }

  #region entry management

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

  }

  public void RemoveAll(string filter) {

  }

  private void RecordEntry(EntryModel model) {
    int existingIndex = FindEntryIndex(model);
    if (existingIndex != -1) {
      RemoveChild(GetChildren()[existingIndex]);
    }

    Node entryNode = _entryTemplate.Instantiate();
    EntryControl entryControl = entryNode.GetNode<EntryControl>(".");
    // await ToSignal(entryControl, Node.SignalName.Ready);
    entryControl.Entry = model;

    AddChild(entryNode);
    MoveChild(entryNode, 0);
    entryNode.Owner = this;

    while (GetChildren().Count > _sizeLimit) {
      // remove the last
      RemoveChild(GetChildren()[GetChildren().Count - 1]);
    }
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

  private EntryModel CreateNodeEntryModel(Node node) {
    return new NodeEntryModel(node);
  }

  private EntryModel CreateFileEntryModel(string path) {
    return new FileEntryModel(path);
  }

  private async void NodeSelectionChangedCallback() {
    Array<Node> selectedNodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
    if (selectedNodes.Count > 0) {
      RecordEntry(CreateNodeEntryModel(selectedNodes[0]));
    }
  }

  private async void FileSystemSelectionChangedCallback() {
    string[] selectedPaths = EditorInterface.Singleton.GetSelectedPaths();
    if (selectedPaths.Length > 0) {
      string path = selectedPaths[0];
      if (!path.Trim().EndsWith("/")) {
        RecordEntry(CreateFileEntryModel(path));
      }
    }
  }

}
#endif
