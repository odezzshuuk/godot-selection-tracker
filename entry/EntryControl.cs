#if TOOLS
using Godot;
using Godot.Collections;

namespace Odezzshuuk.Editor.SelectionTracker;

[Tool]
public partial class EntryControl : Control {

  [ExportGroup("References")]
  [Export]
  private RichTextLabel _entryNameLabel;
  [Export]
  private TextureRect _entryIcon;
  [Export] private Button _locateButton;
  [Export] private Button _openButton;
  [Export] private PopupMenu _contextMenu;
  [Export] private PackedScene _dragPreviewTemplate;

  [ExportGroup("Style")]
  [Export] private Color _loadedColor = Colors.White;
  [Export] private Color _unloadedColor = new(0.85f, 0.73f, 0.33f);
  [Export] private Color _deletedColor = new(0.92f, 0.42f, 0.42f);
  [Export] private Color _defaultColor = Colors.White;

  [Export]
  private EntryModel _entry;

  private readonly PopupMenuHelper _popupMenuHelper = new();

  public int Index { get; set; }

  public string EntryText => _entryNameLabel?.Text ?? string.Empty;

  public EditorInterface Editor { get; set; }

  public EntryModel Entry {
    get => _entry;
    set => BindEntry(value);
  }

  public override void _EnterTree() {

    Texture2D searchIcon = EditorInterface.Singleton.GetEditorTheme().GetIcon("Search", "EditorIcons");
    _locateButton.Icon = searchIcon;

    Texture2D openIcon = EditorInterface.Singleton.GetEditorTheme().GetIcon("Folder", "EditorIcons");
    _openButton.Icon = openIcon;

    _contextMenu.Clear();
    _popupMenuHelper.AddItem("Remove All", RemoveAllEntries)
                    .AddItem("Remove", () => GD.Print("Not Implemented"))
                    .AddSeparator()
                    .AddItem("Entry Info", () => GD.Print(_entry))
                    .ApplyTo(_contextMenu);
  }

  public override void _Ready() {
    PluginHandle.Instance.onSearchTextChanged += SearchTextChangedCallback;

  }


  public override void _GuiInput(InputEvent @event) {
    if (_entry == null || @event is not InputEventMouseButton mouseButton) {
      return;
    }

    if (mouseButton.ButtonIndex == MouseButton.Left) {
      if (mouseButton.DoubleClick) {
        _entry.Open();
        AcceptEvent();
      }
      if (!mouseButton.Pressed) {  // released
        _entry.Locate();
        AcceptEvent();
      }
      return;
    }

    if (mouseButton.ButtonIndex == MouseButton.Right) {
      _contextMenu.Position = DisplayServer.MouseGetPosition();
      _contextMenu.Popup();
      AcceptEvent();
    }
  }

  public override Variant _GetDragData(Vector2 position) {
    var dragData = new Dictionary { };
    Node root = _dragPreviewTemplate.Instantiate();
    DragPreviewControl dragPreview = root.GetNode<DragPreviewControl>(".");
    dragPreview.PreviewData = _entry;
    SetDragPreview(dragPreview);

    switch (_entry.DragPayloadType) {
      case "nodes":
        dragData["type"] = "nodes";
        dragData["nodes"] = new[] { _entry.DragPayloadData };
        break;
      case "files":
        dragData["type"] = "files";
        dragData["files"] = new[] { _entry.DragPayloadData };
        break;
      default:
        GD.PrintErr($"Unknown drag payload type: {_entry.DragPayloadType}");
        break;
    }

    return dragData;
  }

  private void BindEntry(EntryModel value) {
    _entry = value;
    _entryIcon.Texture = _entry.Icon;
    _entryIcon.Visible = _entry.Icon != null;
    bool hideOpen = _entry.CurrentEntryState.HasFlag(EntryState.Accessible);
    _openButton.Visible = hideOpen;
    _entryNameLabel.Modulate = _loadedColor;
    _entryNameLabel.Text = _entry.DisplayName;

    _entry.onUpdated += UpdatedCallback;
  }

  private void RemoveAllEntries() {
    foreach (Node child in GetParent().GetNode<ContainerControl>(".").GetChildren()) {
      child.QueueFree();
    }
  }

  private void LocatePressedCallback() {
    _entry.Locate();
  }

  private void OpenPressedCallback() {
    _entry.Open();
  }

  private void ContextMenuIdPressedCallback(int id) {
    _popupMenuHelper.InvokeCallbackById(id);
  }

  private void UpdatedCallback() {
    EntryState state = _entry.CurrentEntryState;

    if (state.HasFlag(EntryState.Deleted) || state.HasFlag(EntryState.Freed)) {
      _entryNameLabel.Modulate = _deletedColor;
      _entryNameLabel.Text = $"[s]{_entry.DisplayName}[/s]";
    }

    if (state.HasFlag(EntryState.Unaccessible)) {
      _entryNameLabel.Modulate = _unloadedColor;
      _entryNameLabel.Text = _entry.DisplayName;
    }

    if (state.HasFlag(EntryState.Accessible)) {
      _entryNameLabel.Modulate = _loadedColor;
      _entryNameLabel.Text = _entry.DisplayName;
    }
  }

  private void SearchTextChangedCallback(string searchText) {
    if (string.IsNullOrEmpty(searchText)) {
      Visible = true;
      return;
    }

    string entryText = _entryNameLabel.Text.ToLower();
    string searchLower = searchText.ToLower();

    Visible = entryText.Contains(searchLower);
  }
}
#endif
