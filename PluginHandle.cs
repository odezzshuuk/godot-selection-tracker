#if TOOLS
using Godot;
using System;

namespace Odezzshuuk.Editor.SelectionTracker;

public class PluginSettings {

  private PluginSettings() { }
}

[Tool]
public class PluginHandle {

  private static readonly Lazy<PluginHandle> s_Instance = new(() => new PluginHandle());

  public static PluginHandle Instance => s_Instance.Value;

  public Node panelNode;
  public Node containerNode;

  public Action<Node> onSelectedSceneChanged;
  public Action<string> onSearchTextChanged;

}

#endif
