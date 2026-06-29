#if TOOLS
using Godot;
using static Odezzshuuk.Editor.SelectionTracker.Constants;

namespace Odezzshuuk.Editor.SelectionTracker;

[Tool]
public partial class Bootstrap : EditorPlugin {

  private EditorDock _pluginDock;
  private FileSystemDock _fileSystemDock;

  private readonly PluginHandle _pluginHandle = PluginHandle.Instance;

  public override void _EnterTree() {
    PackedScene panelSc = ResourceLoader.Load<PackedScene>(MAIN_PANEL_SCENE_PATH);
    Node panelNode = panelSc.Instantiate();

    _pluginHandle.panelNode = panelNode;

    _pluginDock = new EditorDock {
      Title = "Selections Tracker",

      DefaultSlot = EditorDock.DockSlot.Bottom,
      AvailableLayouts = EditorDock.DockLayout.Horizontal | EditorDock.DockLayout.Floating
    };


    _pluginDock.AddChild(panelNode);
    AddDock(_pluginDock);

    SceneChanged += (scene) => _pluginHandle.onSelectedSceneChanged?.Invoke(scene);
  }

  public override void _DisablePlugin() {
    RemoveDock(_pluginDock);
  }
}
#endif
