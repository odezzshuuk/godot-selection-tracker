#if TOOLS
using Godot;
using Godot.Collections;
using System;

namespace Odezzshuuk.Editor.SelectionTracker;

public interface IEntryStore {
  Array<EntryModel> Entries { get; }
  int CurrentSelectionIndex { get; set; }
  void RecordEntry(EntryModel entry);
  void RemoveEntry(EntryModel entry);
}


[Tool]
public abstract partial class EntryStore : Node, IEntryStore {

  public abstract Array<EntryModel> Entries { get; }
  public virtual int CurrentSelectionIndex { get; set; }

  public abstract void RecordEntry(EntryModel entry);
  public abstract void RemoveEntry(EntryModel entry);

  private int FindEntryIndex<[MustBeVariant] T>(Array<T> entries, IEquatable<T> entry) {
    for (int index = 0; index < entries.Count; index++) {
      if (entries[index]?.Equals(entry) == true) {
        return index;
      }
    }

    return -1;
  }
}

#endif
