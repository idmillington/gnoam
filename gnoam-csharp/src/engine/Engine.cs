using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace gnoam.engine
{
  public class Engine {
    public readonly RuleSet rules;
    public readonly Random random;

    public Engine(RuleSet rules) : this(rules, new Random()) {
    }

    public Engine(RuleSet rules, Random random) {
      this.random = random;
      this.rules = rules;
    }

    // Public API
    // ........................................................................

    public string Run(string rootTagName, Namespace data = null) {
      Tag tag = new Tag(rootTagName);
      return Run(tag, data);
    }

    public string Run(TagBase tag, Namespace data = null) {
      if (!tag.IsRecursive) {
        return RunNonRecursiveTag(tag as NonRecursiveTag, data);
      } else {
        Tag tagToReplace = tag as Tag;
        MatchingRules matchingRules = FindRulesThatCanFire(tagToReplace, data);
        if (matchingRules.Count == 0) {
          return tagToReplace.ToString();
        } else {
          RuleBase ruleToFire = matchingRules.ChooseRuleToFire(random);
          return FireRuleOnTag(ruleToFire, tagToReplace, data);
        }
      }
    }

    // Internal API
    // ........................................................................

    protected string RunNonRecursiveTag(NonRecursiveTag tag, Namespace data) {
      tag.TagWillBeReplaced(tag, data);
      string result = tag.GenerateContent(data);
      result = tag.TagHasBeenReplaced(result, tag, data);
      return result;

    }

    protected string FireRuleOnTag(RuleBase ruleToFire, Tag tagToReplace, Namespace data) {
      ruleToFire.TagWillBeReplaced(tagToReplace, data);
      tagToReplace.TagWillBeReplaced(tagToReplace, data);

      // Recursively generate any content.
      Content content = ruleToFire.Fire(tagToReplace, data);
      StringBuilder builder = new StringBuilder();
      foreach (ContentItem item in content) {
        if (item.IsTag) {
          TagBase tagItem = item as TagBase;
          builder.Append(Run(tagItem, data));
        } else {
          builder.Append(item.ToString());
        }
      }
      string result = builder.ToString();

      // Filter back through tag and root
      result = tagToReplace.TagHasBeenReplaced(result, tagToReplace, data);
      result = ruleToFire.TagHasBeenReplaced(result, tagToReplace, data);
      return result;
    }

    protected class MatchingRules : List<RuleBase> {
      public double totalFrequency = 0;

      public RuleBase ChooseRuleToFire(Random random) {
        double rnd = random.NextDouble() * totalFrequency;
        foreach (RuleBase rule in this) {
          if (rnd <= rule.Frequency) return rule;
          rnd -= rule.Frequency;
        }
        throw new ArgumentNullException("Error in selection algorithm, rule should have been chosen.");
      }
    }

    protected MatchingRules FindRulesThatCanFire(Tag tagToMatch, Namespace data) {
      var result = new MatchingRules();
      var rulesForTag = rules.GetRules(tagToMatch);
      if (rulesForTag != null) {
        int lastMatchingPriority = int.MinValue;
        foreach (RuleBase rule in rulesForTag) {
          if (rule.Priority < lastMatchingPriority) break;

          // If the tag has no hashtags, it doesn't care, if it does, at least one must be present.
          if (tagToMatch.HashTags.Count > 0 && !tagToMatch.HashTags.Overlaps(rule.HashTags)) continue;

          // The rule can have arbitrary additional requirements (like if-clauses).
          if (!rule.CanFire(tagToMatch, data)) continue;

          result.Add(rule);
          result.totalFrequency += rule.Frequency;

          // Only increase the matched priority.
          if (rule.MinPriority > lastMatchingPriority) lastMatchingPriority = rule.MinPriority;
        }
      }
      return result;
    }
  }
}

