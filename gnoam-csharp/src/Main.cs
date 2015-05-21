using System.Collections.Generic;
using gnoam.engine;
using gnoam.file;
using System;

namespace gnoam
{
  class MainClass
  {
    public static void Main(string[] args) {
      TestEngine();
      //TestParse();
    }

    public static void TestParse() {
      string content = 
        "[root] -> Hello.\n" +
        "[root] -> Hi [nickname as name]!\n" +
        "  Do you mind if I call you '[=name]'?\n" +
        "# Add more nicknames if required.\n"+
        "[nickname] -> mate\n" +
        "[nickname] -> buddy\n";

      var lines = LineSplitter.getLines(content, "internal");
      Console.WriteLine(lines.Count);
      foreach (var line in lines) {
        Console.WriteLine(line.ToString());
      }
    }

    public static void TestEngine() {
      TagReplacementWatchers watchers;
      DataLookupTag dlt;
      Rule rule;
      RuleSet rules = new RuleSet();

      rule = new Rule("root");
      rule.Priority = 2; rule.MinPriority = 1;
      rule.Output = new Content();
      rule.Output.Add("Hello. ");
      rule.Output.Add(new Tag("cats"));
      rule.Expression = Expression.Parse("value? 2 >=");
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

      rule = new Rule("nickname");
      rule.Output = new Content();
      rule.Output.Add("dude");
      rules.Add(rule);
      Console.WriteLine(rule.ToString());

      rule = new Rule("nickname");
      rule.Output = new Content();
      rule.Output.Add("man");
      rules.Add(rule);
      Console.WriteLine(rule.ToString());

      rule = new Rule("nickname");
      rule.Output = new Content();
      rule.Output.Add("friendo");
      rules.Add(rule);
      Console.WriteLine(rule.ToString());

      Engine engine = new Engine(rules);
      Namespace data = new Namespace();
      data.Set("value", 2);
      string result = engine.Run("root", data);


      Console.WriteLine("........................................................................");
      Console.WriteLine(result);
      Console.WriteLine("........................................................................");
      Console.WriteLine("Final data:");
      Console.WriteLine(data.ToString("  "));
    }
  }
}
