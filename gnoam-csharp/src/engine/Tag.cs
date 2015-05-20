using System;
using System.Collections.Generic;

namespace gnoam.engine
{
  // --------------------------------------------------------------------------
  // Base classes.
  // --------------------------------------------------------------------------

  public abstract class TagBase : ContentItem, ITagReplacementWatcher {
    private TagReplacementWatchers watchers;

    public override bool IsTag { get { return true; } }

    public TagBase() : this(null) {
    }

    public TagBase(IEnumerable<ITagReplacementWatcher> watchers) {
      if (watchers != null) {
        this.watchers = new TagReplacementWatchers(watchers);
      }
    }

    public void AddWatcher(ITagReplacementWatcher watcher) {
      if (watchers == null) watchers = new TagReplacementWatchers();
      watchers.Add(watcher);
    }

    // IReplacementWatcher
    // ........................................................................

    public void TagWillBeReplaced(TagBase tag, Namespace data) {
      if (watchers != null) watchers.TagWillBeReplaced(tag, data);
    }

    public string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data) {
      if (watchers != null) return watchers.TagHasBeenReplaced(replacement, tag, data);
      else return replacement;
    }
  }

  /**
   * Non recursive tags can generate their own output without rules.
   */
  public abstract class NonRecursiveTag : TagBase {
    public override bool IsRecursive { get { return false; } }

    public abstract string GenerateContent(Namespace data);
  }

  // --------------------------------------------------------------------------
  // Concrete tag types.
  // --------------------------------------------------------------------------

  /**
   * An empty tag is just a repository for clauses.
   */
  public sealed class EmptyTag : NonRecursiveTag {
    public override string GenerateContent(Namespace data) {
      return "";
    }
  }

  /**
   * Data lookup tags are replaced by a piece of data in the namespace.
   */
  public sealed class DataLookupTag : NonRecursiveTag {
    public string DataName { get; private set; }

    public DataLookupTag(string dataName) {
      DataName = dataName;
    }

    public override string GenerateContent(Namespace data) {
      Datum datum = data.Get(DataName);
      if (datum != null) {
        return datum.ToString();
      } else {
        return ToString();
      }
    }

    public override string ToString() {
      return string.Format("[={0}]", DataName);
    }
  }

  /**
   * Tag is the name for recursive tags, those that are matched by rules.
   */
  public sealed class Tag : TagBase, ITagReplacementWatcher {
    public readonly string TagName;
    public readonly string Context;

    public HashSet<string> HashTags { get; private set; }

    public override bool IsTag { get { return true; } }

    public override bool IsRecursive { get { return true; } }

    public Tag(string tagName) : this(tagName, null, null) {
    }

    public Tag(string tagName, IEnumerable<ITagReplacementWatcher> watchers) : this(tagName, null, watchers) {
    }

    public Tag(string tagName, string context, IEnumerable<ITagReplacementWatcher> watchers) : base(watchers) {
      this.Context = context;
      this.TagName = tagName;
      this.HashTags = new HashSet<string>();
    }

    // Equality Testing
    // ........................................................................

    public override bool Equals(object obj) {
      return this == (Tag)obj;
    }

    public override int GetHashCode() {
      if (Context == null) return TagName.GetHashCode();
      else return TagName.GetHashCode() ^ Context.GetHashCode();
    }

    public static bool operator ==(Tag a, Tag b) {
      if (System.Object.ReferenceEquals(a, b)) return true;
      if (((object)a == null) || ((object)b == null)) return false;
      return a.TagName == b.TagName && a.Context == b.Context;
    }

    public static bool operator !=(Tag a, Tag b) {
      return !(a == b);
    }

    // Other API
    // ........................................................................

    public override string ToString() {
      if (Context != null) {
        return string.Format("[{0}.{1}]", Context, TagName);
      } else {
        return string.Format("[{0}]", TagName);
      }
    }
  }
}

