#if TOOLS
using Godot;

namespace Odezzshuuk.Editor.SelectionTracker;

[Tool]
public partial class SettingsControl : Control {

}

[Tool]
public partial class Settings : Resource {

  private bool _showUnloaded = true;
  private bool _showDeleted = true;
  private bool _showFreed = true;

}
#endif
