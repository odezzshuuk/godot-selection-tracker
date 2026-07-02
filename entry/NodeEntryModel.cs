#if TOOLS
using Godot;
using System;

namespace Odezzshuuk.Editor.SelectionTracker;

[Tool]
public partial class NodeEntryModel : EntryModel {

  [Export] private string _cachedNodePath;
  [Export] private string _cachedSceneFileName;
  [Export] private string _cachedMountedScenePath;

  [Export]
  protected ulong _instanceId;  // session-only
  [Export]
  protected long _sceneFileUid;

  [Export] protected string _cachedNodeType;
  [Export] private Node _cachedNode;
  [Export] private Node _cachedOwner;
  [Export] private string _cachedName;

  private SceneTree _cachedSceneTree;

  // public override Variant Ref => _cachedNode;
  public override string DisplayName => _cachedNode != null
    ? string.Concat(_cachedSceneFileName, "/", _cachedNode.Name)
    : string.Concat(_cachedSceneFileName, "/", _cachedName);

  public override EntryState CurrentEntryState {
    get => _cachedRefState;
    set {
      _cachedRefState = value;
      onUpdated.Invoke();
    }
  }

  public NodeEntryModel() { }
  public NodeEntryModel(Node node) {
    CacheNodeInfo(node);
    node.GetTree().NodeRemoved += NodeRemovedCallback;
    node.GetTree().NodeRenamed += NodeRenamedCallback;
    EditorInterface.Singleton.GetResourceFilesystem().FilesystemChanged += FileSystemChangedCallback;
    PluginHandle.Instance.onSelectedSceneChanged += SelectedSceneChangedCallback;
  }


  public override bool Equals(EntryModel other) {
    if (!base.Equals(other)) {
      return false;
    }

    if (other is not NodeEntryModel otherNodeEntry) {
      return false;
    }

    return otherNodeEntry._cachedNode == _cachedNode || otherNodeEntry._instanceId == _instanceId;
  }

  public override int GetHashCode() {
    return HashCode.Combine(_cachedNodePath);
  }

  public override void Locate() {
    EditorInterface.Singleton.EditNode(_cachedNode);
  }

  public override void Open() {
    EditorInterface editor = EditorInterface.Singleton;
    if (CurrentEntryState.HasFlag(EntryState.Loaded)) {

    }

    if (CurrentEntryState.HasFlag(EntryState.Unloaded)) {
      editor.OpenSceneFromPath(_cachedMountedScenePath);
      editor.GetSelection().Clear();
      editor.GetSelection().AddNode(_cachedNode);
      editor.EditNode(_cachedNode);
      return;
    }
  }

  protected void CacheNodeInfo(Node node) {
    _cachedNode = node;
    _cachedNodePath = node.IsInsideTree() ? node.GetPath() : string.Empty;
    _cachedMountedScenePath = GetScenePath(node);
    _cachedName = node.Name;
    _cachedSceneFileName = _cachedMountedScenePath.GetFile();
    _cachedSceneTree = node.GetTree();
    _cachedOwner = node.Owner;
    _cachedNodeType = node.GetType().Name;

    _cachedIcon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon(node.GetClass(), "EditorIcons");
    _instanceId = node.GetInstanceId();
    _cachedRefState = EntryState.Loaded;
    _sceneFileUid = ResourceLoader.GetResourceUid(_cachedMountedScenePath);

    _dragPayloadType = "nodes";
    _dragPayloadData = _cachedNodePath;
  }

  private string GetScenePath(Node node) {
    Node current = node;
    while (current.GetParent() != null) {
      current = current.GetParent();
      if (!string.IsNullOrEmpty(current.SceneFilePath)) {
        return current.SceneFilePath;
      }
    }
    return node.SceneFilePath;
  }

  private void NodeRemovedCallback(Node node) {
    if (node == _cachedNode) {
      if (EditorInterface.Singleton.GetEditedSceneRoot() == _cachedOwner) {
        CurrentEntryState = EntryState.Deleted;
        node.GetTree().NodeRemoved -= NodeRemovedCallback;
        node.GetTree().NodeRenamed -= NodeRenamedCallback;
        EditorInterface.Singleton.GetResourceFilesystem().FilesystemChanged -= FileSystemChangedCallback;
        PluginHandle.Instance.onSelectedSceneChanged -= SelectedSceneChangedCallback;
      } else {
        CurrentEntryState = EntryState.Unloaded;
      }
    }
  }

  private void NodeRenamedCallback(Node node) {
    if (node == _cachedNode) {
      onUpdated.Invoke();
    }
  }

  private void SelectedSceneChangedCallback(Node node) {
    if (node == _cachedOwner) {
      CurrentEntryState = EntryState.Loaded;
    }
  }

  private void FileSystemChangedCallback() {
    // update scene file prefix
    string path = ResourceUid.Singleton.GetIdPath(_sceneFileUid);
    _cachedSceneFileName = path.GetFile();
    onUpdated.Invoke();
  }

  #region debugging
  public override string ToString() {
    return $"""
    ----------- EntryModel Debug Info -----------
      NodeName: {_cachedNode.Name},
      NodePath: {_cachedNodePath},
      ParentNode: {_cachedNode?.GetParent()?.Name},
      ScenePath: {_cachedMountedScenePath},
      CachedScene: {_cachedOwner?.Name},
      EditedSceneTreeRoot: {_cachedSceneTree?.EditedSceneRoot.Name},
      EditedSceneRoot: {EditorInterface.Singleton.GetEditedSceneRoot()?.Name},
      PathFromId: {ResourceUid.Singleton.GetIdPath(_sceneFileUid)},
    """;
    // CurrentScene: {_cachedSceneTree?.CurrentScene.Name}

  }
  #endregion

}
#endif
