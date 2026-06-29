#if TOOLS
using Godot;
using System;

namespace Odezzshuuk.Editor.SelectionTracker;

[Tool]
public partial class WindowControl : Control {

  [Export]
  private LineEdit _searchBar;

  [Export]
  private PopupMenu _contextMenu;

  [Export]
  private Node _containerControl;

  private PopupMenuHelper _popupMenuHelper;

  public (int id, string text, bool isSeparator, Action callback)[] ContextMenuItems { get; private set; }

  public override void _Ready() {
    void RemoveAllEntries() {
      foreach (Node child in _containerControl.GetChildren()) {
        child.QueueFree();
      }
    }
    _contextMenu.Clear();
    _popupMenuHelper = new PopupMenuHelper();
    _popupMenuHelper.AddItem("Remove Deleted", () => GD.Print("Remove Deleted Not Implemented"))
                    .AddItem("Remove All", RemoveAllEntries)
                    .AddSeparator()
                    .AddItem("Tree Root Scene", () => GD.Print(GetTree().EditedSceneRoot.Name))
                    .AddItem("Current Scene", () => GD.Print(GetTree().CurrentScene.Name))
                    .AddItem("Edited Scene", () => GD.Print(EditorInterface.Singleton.GetEditedSceneRoot().Name))
                    .ApplyTo(_contextMenu);
  }


  public override void _GuiInput(InputEvent @event) {
    if (@event is InputEventMouseButton mouseEvent) {
      if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed) {
        _contextMenu.Position = DisplayServer.MouseGetPosition();
        _contextMenu.Popup();
        AcceptEvent();
      }
    }
  }

  private void FilterEntryies() {

  }

  private bool PassFilter() {
    return false;
  }

  private void ContextMenuPressedCallback(long id) {
    _popupMenuHelper.InvokeCallbackById(id);
  }

  private void textChangedCallback(string text) {
    PluginHandle.Instance.onSearchTextChanged?.Invoke(text);
  }

  private void StoreChangedCallback() {

  }

}
#endif
