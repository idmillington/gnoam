using System.Collections.Generic;
using gnoam.engine;
using System;

namespace gnoam
{
  class MainClass
  {
    public static void Main(string[] args)
    {
      TagReplacementWatchers watchers;
      DataLookupTag dlt;
      Rule rule;
      RuleSet rules = new RuleSet();

      rule = new Rule("root");
      rule.Output = new Content();
      rule.Output.Add("Hello. ");
      rule.Output.Add(new Tag("cats"));
      rules.Add(rule);
      Console.WriteLine(rule.ToString());

      rule = new Rule("cats");
      rule.Output = new Content();
      dlt = new DataLookupTag("value");
      dlt.AddWatcher(new CardinalFilter());
      rule.Output.Add(dlt);
      rule.Output.Add(" (");
      rule.Output.Add(new DataLookupTag("value"));
      rule.Output.Add(") cat");
      dlt = new DataLookupTag("value");
      dlt.AddWatcher(new PluraliseFilter());
      rule.Output.Add(dlt);
      rule.Output.Add(" in a bag, here is the ");
      dlt = new DataLookupTag("value");
      dlt.AddWatcher(new OrdinalFilter());
      rule.Output.Add(dlt);
      rule.Output.Add(" (");
      dlt = new DataLookupTag("value");
      dlt.AddWatcher(new OrdinalSuffixFilter());
      rule.Output.Add(dlt);
      rule.Output.Add(")");
      rule.AddWatcher(new SentenceFilter());
      rules.Add(rule);
      Console.WriteLine(rule.ToString());

      rule = new Rule("root");
      rule.Frequency = 2;
      rule.Output = new Content();
      rule.Output.Add("Hi ");
      watchers = new TagReplacementWatchers();
      watchers.Add(new AsClause("name"));
      rule.Output.Add(new Tag("nickname", watchers));
      rule.Output.Add("! Do you mind if I call you '");
      rule.Output.Add(new DataLookupTag("name"));
      rule.Output.Add("'?");
      rules.Add(rule);
      Console.WriteLine(rule.ToString());

      rule = new Rule("nickname");
      rule.Output = new Content();
      rule.Output.Add("mate");
      rules.Add(rule);
      Console.WriteLine(rule.ToString());

      rule = new Rule("nickname");
      rule.Output = new Content();
      rule.Output.Add("buddy");
      rules.Add(rule);
      Console.WriteLine(rule.ToString());

      Engine engine = new Engine(rules);
      Namespace data = new Namespace();
      data.Set("value", 3);
      string result = engine.Run("root", data);


      Console.WriteLine("........................................................................");
      Console.WriteLine(result);
      Console.WriteLine("........................................................................");
      Console.WriteLine("Final data:");
      Console.WriteLine(data.ToString("  "));
    }
  }
}
