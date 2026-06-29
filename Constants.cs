#if TOOLS

namespace Odezzshuuk.Editor.SelectionTracker;

public static class Constants {

  public const string PLUGIN_DIR_PATH = "res://addons/godot-selection-tracker/";
  public const string SCENE_INSTANCE_DIR_PATH = "user://editor/godot-selection-tracker/";

  public const string MAIN_PANEL_SCENE_PATH = PLUGIN_DIR_PATH + "scenes/main-window.tscn";
  public const string MAIN_PANEL_INSTANCE_PATH = SCENE_INSTANCE_DIR_PATH + "main-window.tscn";

  public const string SELECTION_ENTRIES_SCENE_PATH = PLUGIN_DIR_PATH + "scenes/selection-entries.tscn";
  public const string SELECTION_ENTRIES_INSTANCE_PATH = SCENE_INSTANCE_DIR_PATH + "selection-entries.tscn";

}
#endif
