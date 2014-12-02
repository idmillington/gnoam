using System;
using System.Text;
using System.Collections.Generic;

namespace gnoam.engine
{
  public abstract class ContentItem {
    public abstract bool IsTag { get; }
    public abstract bool IsRecursive { get; }
  }

  public sealed class TextContent : ContentItem {
    public readonly string text;

    public override bool IsTag { get { return false; } }
    public override bool IsRecursive { get { return false; } }

    public TextContent(string text) {
      this.text = text;
    }

    public override string ToString() {
      return text;
    }
  }

  public sealed class Content : List<ContentItem> {
    public override string ToString() {
      StringBuilder builder = new StringBuilder();
      foreach (ContentItem item in this) {
        builder.Append(item.ToString());
      }
      return builder.ToString();
    }
  }
}