using System.Collections.Generic;
using System.Text;

namespace gnoam.engine
{
  public enum DataType {
    NumericDatum,
    TextDatum,
    Namespace
  }

  public abstract class Datum {
    public abstract DataType GetDataType();

    public abstract string ToString(string indent);
  }

  public class NumericDatum : Datum {
    public readonly float number;

    public NumericDatum(float number) {
      this.number = number;
    }

    public override DataType GetDataType() {
      return DataType.NumericDatum;
    }

    public override string ToString() {
      return string.Format("{0}", number);
    }

    public override string ToString(string indent) {
      return indent + ToString();
    }
  }

  public class TextDatum : Datum {
    public readonly string text;

    public TextDatum(string text) {
      this.text = text;
    }

    public override DataType GetDataType() {
      return DataType.TextDatum;
    }

    public override string ToString() {
      return text;
    }

    public override string ToString(string indent) {
      return indent + ToString();
    }
  }

  public class Namespace : Datum {
    public readonly Dictionary<string, Datum> data;

    public override DataType GetDataType() {
      return DataType.Namespace;
    }

    public Namespace() {
      data = new Dictionary<string, Datum>();
    }

    public void Set(string dataName, float number) {
      data[dataName] = new NumericDatum(number);
    }

    public void Set(string dataName, string text) {
      if (text == null) {
        if (data.ContainsKey(dataName)) data.Remove(dataName);
      } else {
        data[dataName] = new TextDatum(text);
      }
    }

    public Datum Get(string dataName) {
      if (data.ContainsKey(dataName)) {
        return data[dataName];
      } else {
        return null;
      }
    }

    public override string ToString(string indent) {
      StringBuilder builder = new StringBuilder();
      foreach (string key in data.Keys) {
        builder.Append(indent);
        builder.Append(key);
        builder.Append(": ");
        builder.Append(data[key].ToString());
        builder.Append("\n");
      }
      return builder.ToString();
    }

    public override string ToString() {
      return ToString("");
    }
  }
}