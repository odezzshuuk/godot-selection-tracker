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


  private string GetSafeNodeName() {
    if (IsAlive(_cachedNode)) {
      _cachedName = _cachedNode.Name;
    }
    return _cachedName ?? string.Empty;
  }

  // public override Variant Ref => _cachedNode;
  public override string DisplayName => string.Concat(_cachedSceneFileName, "/", GetSafeNodeName());

  public override EntryState CurrentEntryState {
    get => _cachedRefState;
    set {
      _cachedRefState = value;
      onUpdated?.Invoke();
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

  public override void _ExitTree() {
    if (IsAlive(_cachedSceneTree)) {
      _cachedSceneTree.NodeRemoved -= NodeRemovedCallback;
      _cachedSceneTree.NodeRenamed -= NodeRenamedCallback;
    }

    if (IsAlive(EditorInterface.Singleton)) {
      EditorFileSystem filesystem = EditorInterface.Singleton.GetResourceFilesystem();
      if (IsAlive(filesystem)) {
        filesystem.FilesystemChanged -= FileSystemChangedCallback;
      }
    }

    if (PluginHandle.Instance != null) {
      PluginHandle.Instance.onSelectedSceneChanged -= SelectedSceneChangedCallback;
    }
  }


  public override bool Equals(EntryModel other) {
    if (!base.Equals(other)) {
      return false;
    }

    if (other is not NodeEntryModel otherNodeEntry) {
      return false;
    }

    return otherNodeEntry._cachedNode == _cachedNode ||
      otherNodeEntry._instanceId == _instanceId;
  }

  public override int GetHashCode() {
    return HashCode.Combine(_cachedNodePath);
  }

  public override void Locate() {
    if (!IsAlive(_cachedNode)) {
      return;
    }
    EditorInterface.Singleton.EditNode(_cachedNode);
  }

  public override void Open() {
    EditorInterface editor = EditorInterface.Singleton;
    if (CurrentEntryState.HasFlag(EntryState.Loaded)) {

    }

    if (CurrentEntryState.HasFlag(EntryState.Unloaded)) {
      editor.OpenSceneFromPath(_cachedMountedScenePath);
      if (IsAlive(_cachedNode)) {
        editor.GetSelection().Clear();
        editor.GetSelection().AddNode(_cachedNode);
        editor.EditNode(_cachedNode);
      }
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
    if (!IsAlive(node) || node.GetInstanceId() != _instanceId) {
      return;
    }

    _cachedName = node.Name;
    _cachedNode = null;

    if (EditorInterface.Singleton.GetEditedSceneRoot() == _cachedOwner) {
      CurrentEntryState = EntryState.Deleted;
    } else {
      CurrentEntryState = EntryState.Unloaded;
    }
  }

  private void NodeRenamedCallback(Node node) {
    if (!IsAlive(node) || node.GetInstanceId() != _instanceId) {
      return;
    }

    _cachedName = node.Name;
    onUpdated?.Invoke();
  }

  private void SelectedSceneChangedCallback(Node node) {
    if (IsAlive(_cachedOwner) && node == _cachedOwner) {
      CurrentEntryState = EntryState.Loaded;
    }
  }

  // when the filesystem changes, update the scene file name
  private void FileSystemChangedCallback() {
    // update scene file prefix
    string path = ResourceUid.Singleton.GetIdPath(_sceneFileUid);
    _cachedSceneFileName = path.GetFile();
    onUpdated?.Invoke();
  }

  private bool IsAlive(GodotObject obj) {
    return obj != null && IsInstanceValid(obj);
  }

  #region debugging
  public override string ToString() {
    string nodeName = GetSafeNodeName();
    string parentName = IsAlive(_cachedNode) && IsAlive(_cachedNode.GetParent())
      ? _cachedNode.GetParent().Name
      : "<none>";
    string cachedSceneName = IsAlive(_cachedOwner) ? _cachedOwner.Name : "<none>";
    string editedSceneTreeRoot = IsAlive(_cachedSceneTree) && IsAlive(_cachedSceneTree.EditedSceneRoot)
      ? _cachedSceneTree.EditedSceneRoot.Name
      : "<none>";

    return $"""
    ----------- EntryModel Debug Info -----------
      NodeName: {nodeName},
      NodePath: {_cachedNodePath},
      ParentNode: {parentName},
      ScenePath: {_cachedMountedScenePath},
      CachedScene: {cachedSceneName},
      EditedSceneTreeRoot: {editedSceneTreeRoot},
      EditedSceneRoot: {EditorInterface.Singleton.GetEditedSceneRoot()?.Name},
      PathFromId: {ResourceUid.Singleton.GetIdPath(_sceneFileUid)},
    """;
    // CurrentScene: {_cachedSceneTree?.CurrentScene.Name}

  }
  #endregion

}
#endif
