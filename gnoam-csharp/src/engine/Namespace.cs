using System.Collections.Generic;
using System.Text;

namespace gnoam.engine
{
  public enum DataType {
    NumericDatum,
    TextDatum,
    BooleanDatum,
    Namespace
  }

  public abstract class Datum {
    public abstract DataType GetDataType();

    public virtual string ToString(string indent) {
      return indent + ToString();
    }

    public abstract bool ToBool();
  }

  public class BooleanDatum : Datum {
    public readonly bool value;

    public BooleanDatum(bool value) {
      this.value = value;
    }

    public override DataType GetDataType() {
      return DataType.BooleanDatum;
    }

    public override string ToString() {
      return value ? "true" : "false";
    }

    public override bool ToBool() {
      return value;
    }

    public override bool Equals(object obj) {
      BooleanDatum other = obj as BooleanDatum;
      if (other == null) return false;
      return value == other.value;
    }

    public override int GetHashCode() {
      return value.GetHashCode();
    }
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

    public override bool ToBool() {
      return number != 0;
    }

    public override bool Equals(object obj) {
      NumericDatum other = obj as NumericDatum;
      if (other == null) return false;
      return number == other.number;
    }

    public override int GetHashCode() {
      return number.GetHashCode();
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

    public override bool ToBool() {
      return text != string.Empty;
    }

    public override bool Equals(object obj) {
      TextDatum other = obj as TextDatum;
      if (other == null) return false;
      return text == other.text;
    }

    public override int GetHashCode() {
      return text.GetHashCode();
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
      Set(dataName, new NumericDatum(number));
    }

    public void Set(string dataName, string text) {
      Set(dataName, new TextDatum(text));
    }

    public void Set(string dataName, Datum datum) {
      data[dataName] = datum;
    }

    public Datum Get(string dataName) {
      if (data.ContainsKey(dataName)) {
        return data[dataName];
      } else {
        return null;
      }
    }

    public override bool ToBool() {
      return data != null && data.Count != 0;
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