using System;
using System.Diagnostics;

namespace gnoam.engine
{
  public class Context {
    public Content Content { get; protected set; }

    public Namespace Data { get; protected set; }

    public int Position { get; protected set; }

    public bool HasUnprocessedTags { get; protected set; }

    public bool IsDone {
      get { return Position >= Content.Count; }
    }

    public Context(Content content) : this(content, new Namespace()) {
    }

    public Context(Content content, Namespace data) {
      Content = content;
      Data = data;
      Position = 0;
    }

    public void ScanForwardToNextTag() {
      while (Position < Content.Count && !Content[Position].IsTag) {
        if (!Content[Position].CanOutput) {
          DeferredAction action = Content[Position] as DeferredAction;
          Debug.Assert(action != null, "Internal content items should be deferred actions.");

          Content.RemoveAt(Position);
          action.Do(this);
        } else {
          ++Position;
        }
      }
    }

    public void SkipCurrentTag() {
      HasUnprocessedTags = true;
      ++Position;
    }

    public Tag GetCurrentTag() {
      if (IsDone) return null;
      else return (Tag)Content[Position];
    }

    public void ReplaceCurrentPosition(Tag tag, Rule rule) {
      EndReplacement end = new EndReplacement(Position, tag, rule);
      Content newContent = rule.Fire(this);
      Content.RemoveAt(Position);
      Content.InsertRange(Position, newContent);
      Content.Insert(Position + newContent.Count, end);
    }

    /** Replaces the range from the given location to the current position with a text node. */
    public void Reduce(int from) {
      string reduction = Content.ToString(from, Position);
      Content.RemoveRange(from, Position - from);
      TextContent text = new TextContent(reduction);
      Content.Insert(from, text);
      Position = from;
    }
  }
}

