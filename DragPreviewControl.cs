#if TOOLS
using Godot;
namespace Odezzshuuk.Editor.SelectionTracker;

[Tool]
public partial class DragPreviewControl : Control {

  [Export]
  private TextureRect _icon;
  [Export]
  private Label _label;

  public EntryModel PreviewData {
    set {
      _label.Text = value.DisplayName;
      _icon.Texture = value.Icon;
    }
  }
}
#endif
