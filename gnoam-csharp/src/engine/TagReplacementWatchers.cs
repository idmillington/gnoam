using System;
using System.Collections.Generic;

namespace gnoam.engine
{
  public interface ITagReplacementWatcher {
    void TagWillBeReplaced(TagBase tag, Namespace data);

    string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data);
  }

  /**
   * Convenience class to hold a series of watchers and delegate calls to them in the correct order.
   */
  public sealed class TagReplacementWatchers : List<ITagReplacementWatcher> {
    public TagReplacementWatchers() : base() {
    }

    public TagReplacementWatchers(IEnumerable<ITagReplacementWatcher> watchers) : base(watchers) {
    }

    public void TagWillBeReplaced(TagBase tag, Namespace data) {
      foreach (ITagReplacementWatcher watcher in this) {
        watcher.TagWillBeReplaced(tag, data);
      }
    }

    public string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data) {
      for (int i = Count - 1; i >= 0; --i) {
        ITagReplacementWatcher watcher = this[i];
        replacement = watcher.TagHasBeenReplaced(replacement, tag, data);
      }
      return replacement;
    }
  }

  public abstract class FilterBase : ITagReplacementWatcher {
    public virtual void TagWillBeReplaced(TagBase tag, Namespace data) {
    }

    public virtual string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data) {
      return replacement;
    }
  }

  public abstract class NumericFilterBase : FilterBase {
    public override string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data) {
      try {
        float number = float.Parse(replacement);
        return GenerateTextFromNumber(number);
      } catch (FormatException) {
        return replacement;
      }
    }

    public abstract string GenerateTextFromNumber(float number);
  }

  // --------------------------------------------------------------------------
  // Concrete watchers (these typically map to clauses in tags or rules).
  // --------------------------------------------------------------------------

  /**
   * Stores the replaced text in the namespace under the given data name.
   */
  public class AsClause : ITagReplacementWatcher {
    protected string dataName;

    public AsClause(string dataName) {
      this.dataName = dataName;
    }

    public void TagWillBeReplaced(TagBase tag, Namespace data) {
    }

    public string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data) {
      data.Set(dataName, replacement);
      return replacement;
    }
  }

  /**
   * Ensures the text begins with a capital and ends with a period, unless it already ends with
   * a sentence closing punctuation.
   */
  public class SentenceFilter : FilterBase {
    public override string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data) {
      if (replacement.Length <= 0) return replacement;

      bool replaceLast = false;
      bool addPeriod = true;
      switch (replacement[replacement.Length - 1]) {
      case ';':
      case ':':
      case ',':
      case '-':
        replaceLast = true;
        break;
      case '?':
      case '.':
      case '!':
        addPeriod = false;
        break;
      }

      return string.Format("{0}{1}{2}",
        replacement[0].ToString().ToUpper(),
        replacement.Substring(1, replacement.Length - (replaceLast ? 2 : 1)),
        addPeriod ? "." : ""
      );
    }
  }

  /**
   * Outputs a plural suffix unless the input is 1.
   * 
   * Default plural is 's', but singular and plural values can be given.
   */
  public class PluraliseFilter : NumericFilterBase {
    protected string singular;
    protected string plural;

    public PluraliseFilter(string singular, string plural) {
      this.singular = singular;
      this.plural = plural;
    }

    public PluraliseFilter(string plural) : this("", plural) {
    }

    public PluraliseFilter() : this("", "s") {
    }

    public override string GenerateTextFromNumber(float number) {
      if (number == 1) return singular;
      else return plural;
    }
  }

  /** Outputs the number with an ordinal suffix, e.g. 101st.
   * 
   * Only works with +ve integers, floats are output without modification.
   */ 
  public class OrdinalSuffixFilter : NumericFilterBase {
    public override string GenerateTextFromNumber(float number) {
      string output = number.ToString();
      int asInt = (int)Math.Floor(number);
      if (output.Length > 0 && (float)asInt == number && asInt > 0) {
        if (output.Length > 1 && output[output.Length - 2] == '1') {
          output += "th";
        } else {
          switch (output[output.Length - 1]) {
          case '1':
            output += "st";
            break;
          case '2':
            output += "nd";
            break;
          case '3':
            output += "rd";
            break;
          default:
            output += "th";
            break;
          }
        }
      }
      return output;
    }
  }

  /** APA guidelines say,  spell out numbers up to and including 9. */
  public class OrdinalFilter : OrdinalSuffixFilter {
    public static string[] output = new string[] {
      "0th", "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth"
    };

    public override string GenerateTextFromNumber(float number) {
      int index = (int)Math.Floor(number);
      if ((float)index != number || index < 0 || index >= output.Length) {
        return base.GenerateTextFromNumber(number);
      } else {
        return output[index];
      }
    }
  }

  /** APA guidelines say,  spell out numbers up to and including 9. */
  public class CardinalFilter : NumericFilterBase {
    public static string[] output = new string[] {
      "0", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"
    };

    public override string GenerateTextFromNumber(float number) {
      int index = (int)Math.Floor(number);
      if ((float)index != number || index < 0 || index >= output.Length) return number.ToString();
      else return output[index];
    }
  }
}

