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

      Rule rule1 = new Rule("root");
      rule1.Output = new Content();
      rule1.Output.Add(new TextContent("Hello."));

      Rule rule2 = new Rule("root");
      rule2.Frequency = 2;
      rule2.Output = new Content();
      rule2.Output.Add(new TextContent("Hi "));
      watchers = new TagReplacementWatchers();
      watchers.Add(new AsClause("name"));
      rule2.Output.Add(new Tag("nickname", watchers));
      rule2.Output.Add(new TextContent("! Do you mind if I call you '"));
      rule2.Output.Add(new DataLookupTag("name"));
      rule2.Output.Add(new TextContent("'?"));

      Rule rule3 = new Rule("nickname");
      rule3.Output = new Content();
      rule3.Output.Add(new TextContent("mate"));

      Rule rule4 = new Rule("nickname");
      rule4.Output = new Content();
      rule4.Output.Add(new TextContent("buddy"));

      RuleSet rules = new RuleSet();
      rules.Add(rule1);
      rules.Add(rule2);
      rules.Add(rule3);
      rules.Add(rule4);

      Engine engine = new Engine(rules);
      Namespace data = new Namespace();
      string result = engine.Run("root", data);

      Console.WriteLine(rule1.ToString());
      Console.WriteLine(rule2.ToString());
      Console.WriteLine(rule3.ToString());
      Console.WriteLine(rule4.ToString());
      Console.WriteLine("........................................................................");
      Console.WriteLine(result);
      Console.WriteLine("........................................................................");
      Console.WriteLine("Final data:");
      Console.WriteLine(data.ToString("  "));
    }
  }
}
