using System;
using System.Collections.Generic;

namespace gnoam.engine
{
  /**
   * Base class for how rules need to operate to work in the engine.
   */
  public abstract class RuleBase : IComparable<RuleBase>, ITagReplacementWatcher {
    public RuleBase(string tagName, int priority, double frequency) : this(new Tag(tagName), priority, frequency) {
    }

    public RuleBase(Tag tag, int priority, double frequency) {
      Tag = tag;
      Priority = MinPriority = priority;
      Frequency = frequency;
    }

    // Core data
    // ........................................................................

    public Tag Tag { get; set; }

    public HashSet<string> HashTags { get; protected set; }

    public int Priority { get; set; }

    /**
     * MinPriority allows a rule to be valid in a range of priorities.
     * 
     * A Priority of 1 and MinPriority of 0, for example, would allow the
     * rule to be chosen if other rules of Priority 1 were valid, but
     * if it were the only one chosen (or if it were chosen along with others
     * that also had a MinPriority of 0), then it would fall through and
     * also allow priority 0 rules to be chosen.
     */
    private int _minPriority;
    public int MinPriority { 
      get { return _minPriority; }
      set { 
        if (value > Priority) value = Priority;
        _minPriority = value;
      }
    }

    public double Frequency { get; set; }

    // Firing support
    // ........................................................................

    public abstract bool CanFire(Tag tag, Namespace data);

    public abstract Content Fire(Tag tag, Namespace data);

    // IReplacementWatcher
    // ........................................................................

    public abstract void TagWillBeReplaced(TagBase tag, Namespace data);

    public abstract string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data);

    // IComparable
    // ........................................................................

    public int CompareTo(RuleBase other) {
      if (other == null) return 1;
      int priorityCompare = other.Priority.CompareTo(Priority);
      if (priorityCompare != 0) return priorityCompare;
      else return GetHashCode().CompareTo(other.GetHashCode());
    }
  }

  /**
   * A basic rule with a fixed output and a set of delegate replacement watchers.
   */
  public class Rule : RuleBase {
    public Content Output;
    public Expression Expression;

    private TagReplacementWatchers watchers;

    // ........................................................................

    public Rule(string tagName) : this(new Tag(tagName), null, 1, 1, null) {
    }

    public Rule(Tag tag) : this(tag, null, 1, 1, null) {

    }

    public Rule(Tag tag, Content output, int priority, double frequency, IEnumerable<ITagReplacementWatcher> watchers)
      : base(tag, priority, frequency) {
      Output = output;
      if (watchers != null) {
        this.watchers = new TagReplacementWatchers(watchers);
      }
    }

    public void AddWatcher(ITagReplacementWatcher watcher) {
      if (watchers == null) watchers = new TagReplacementWatchers();
      watchers.Add(watcher);
    }

    // RuleBase
    // ........................................................................

    /** 
     * Checks if any other constrains on the rule allow it to fire.
     * 
     * This is called after the tag is checked for matching, and after hashtags
     * are considered, so they can be ignored.
     */
    public override bool CanFire(Tag tag, Namespace data) {
      if (Expression != null) {
        return Expression.Evaluate(data).ToBool();
      } else {
        return true;
      }
    }

    public override Content Fire(Tag tag, Namespace data) {
      return Output;
    }

    // IReplacementWatcher
    // ........................................................................

    public override void TagWillBeReplaced(TagBase tag, Namespace data) {
      if (watchers != null) watchers.TagWillBeReplaced(tag, data);
    }

    public override string TagHasBeenReplaced(string replacement, TagBase tag, Namespace data) {
      if (watchers != null) return watchers.TagHasBeenReplaced(replacement, tag, data);
      else return replacement;
    }

    // Other API
    // ........................................................................

    public override string ToString() {
      return string.Format("{0} -> {1}", Tag.ToString(), Output.ToString());
    }
  }

  /**
   * A repository for rules that can be queried by tag.
   */
  public class RuleSet {
    protected Dictionary<Tag, SortedSet<RuleBase>> rules;

    public RuleSet() {
      rules = new Dictionary<Tag, SortedSet<RuleBase>>();
    }

    public void Add(RuleBase rule) {
      Tag tag = rule.Tag;
      if (!rules.ContainsKey(tag)) {
        rules[tag] = new SortedSet<RuleBase>();
      }
      rules[tag].Add(rule);
    }

    public SortedSet<RuleBase> GetRules(Tag tag) {
      if (rules.ContainsKey(tag)) {
        return rules[tag];
      } else {
        return null;
      }
    }
  }
}

