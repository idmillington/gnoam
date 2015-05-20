using System;
using System.Collections.Generic;

namespace gnoam.engine
{
  public enum OpCode {
    // Binary on Numbers -> Number
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    // Binary on Boolean -> Boolean
    Or,
    And,
    // Unary on Anything -> Boolean
    Not,
    // Binary on Anything -> Boolean
    Equal,
    NotEqual,
    // Binary on Numbers -> Boolean
    LessThan,
    GreaterThan,
    LessThanOrEquals,
    GreaterThanOrEquals,
    // Binary on Numbers -> Number
    Minimum,
    Maximum,
    // Special
    Literal,
    Get
  }

  public struct Instruction {
    public OpCode OpCode;
    public Datum Datum;
    // Only for OpCode.Literal
    public string DataName;
    // Only for OpCode.Get

    public Instruction(OpCode opCode) {
      OpCode = opCode;
      Datum = null;
      DataName = null;
    }

    public Instruction(Datum literal) {
      OpCode = OpCode.Literal;
      Datum = literal;
      DataName = null;
    }

    public Instruction(string dataName) {
      OpCode = OpCode.Get;
      DataName = dataName;
      Datum = null;
    }

    public static Instruction Op(OpCode opCode) {
      return new Instruction(opCode);
    }

    public static Instruction Get(string dataName) {
      return new Instruction(dataName);
    }

    public static Instruction Number(float number) {
      return new Instruction(new NumericDatum(number));
    }

    public static Instruction Text(string text) {
      return new Instruction(new TextDatum(text));
    }

    public static Instruction Boolean(bool val) {
      return new Instruction(new BooleanDatum(val));
    }

    public static Instruction True() {
      return new Instruction(new BooleanDatum(true));
    }

    public static Instruction False() {
      return new Instruction(new BooleanDatum(false));
    }


    public override string ToString() {
      switch (OpCode) {
      case OpCode.Get:
        return string.Format("{0}?", DataName);
      case OpCode.Literal:
        return Datum.ToString();
      case OpCode.Add:
        return "+";
      case OpCode.Subtract:
        return "-";
      case OpCode.Multiply:
        return "*";
      case OpCode.Divide:
        return "/";
      case OpCode.Modulo:
        return "%";
      case OpCode.Equal:
        return "=";
      case OpCode.NotEqual:
        return "!=";
      case OpCode.And:
        return "&";
      case OpCode.Or:
        return "|";
      case OpCode.Not:
        return "!";
      case OpCode.LessThan:
        return "<";
      case OpCode.GreaterThan:
        return ">";
      case OpCode.LessThanOrEquals:
        return "<=";
      case OpCode.GreaterThanOrEquals:
        return ">=";
      case OpCode.Maximum:
        return ">>";
      case OpCode.Minimum:
        return "<<";
      }
      throw new ArgumentException("Unknown opcode in instruction.");
    }
  }

  /**
   * Implements a stack-based VM for the expression language.
   */
  public sealed class Expression : List<Instruction> {
    public Expression() : base() {
    }

    public Expression(IEnumerable<Instruction> instructions) : base(instructions) {
    }

    public override string ToString() {
      return string.Join<Instruction>(" ", this);
    }

    private static Dictionary<string, OpCode> ops = new Dictionary<string, OpCode> {
      {"+", OpCode.Add},
      {"-", OpCode.Subtract},
      {"*", OpCode.Multiply},
      {"/", OpCode.Divide},
      {"%", OpCode.Modulo},

      {"|", OpCode.Or},
      {"&", OpCode.And},
      {"!", OpCode.Not},

      {"=", OpCode.Equal},
      {"!=", OpCode.NotEqual},

      {"<", OpCode.LessThan},
      {"<=", OpCode.LessThanOrEquals},
      {">=", OpCode.GreaterThanOrEquals},
      {">", OpCode.GreaterThan},

      {"<<", OpCode.Minimum},
      {">>", OpCode.Maximum}
    };

    public static Expression Parse(string text) {
      Expression expression = new Expression();
      foreach (var chunk in text.Split(' ')) {
        try {
          float asNumber = float.Parse(chunk);
          expression.Add(Instruction.Number(asNumber));
        } catch (FormatException) {
          if (Expression.ops.ContainsKey(chunk)) {
            expression.Add(Instruction.Op(Expression.ops[chunk]));
          } else if (chunk.EndsWith("?")) {
            expression.Add(Instruction.Get(chunk.Substring(0, chunk.Length - 1)));
          } else {
            throw new ArgumentException(string.Format("Chunk {0} not recognized.", chunk));
          }
        }
      }
      return expression;
    }

    public Datum Evaluate(Namespace data) {
      if (this.Count == 0) return new NumericDatum(1);

      Stack<Datum> stack = new Stack<Datum>();
      foreach (Instruction instruction in this) {
        switch (instruction.OpCode) {
        case OpCode.Literal:
          stack.Push(instruction.Datum);
          break;
        case OpCode.Get:
          stack.Push(data.Get(instruction.DataName));
          break;
        case OpCode.Minimum:
          DoMinimum(stack);
          break;
        case OpCode.Maximum:
          DoMaximum(stack);
          break;
        case OpCode.Add:
          DoAdd(stack);
          break;
        case OpCode.Subtract:
          DoSubtract(stack);
          break;
        case OpCode.Multiply:
          DoMultiply(stack);
          break;
        case OpCode.Divide:
          DoDivide(stack);
          break;
        case OpCode.Modulo:
          DoModulo(stack);
          break;
        case OpCode.Not:
          DoNot(stack);
          break;
        case OpCode.And:
          DoAnd(stack);
          break;
        case OpCode.Or:
          DoOr(stack);
          break;
        case OpCode.Equal:
          DoEqual(stack);
          break;
        case OpCode.NotEqual:
          DoNotEqual(stack);
          break;
        case OpCode.LessThan:
          DoLessThan(stack);
          break;
        case OpCode.LessThanOrEquals:
          DoLessThanOrEquals(stack);
          break;
        case OpCode.GreaterThan:
          DoGreaterThan(stack);
          break;
        case OpCode.GreaterThanOrEquals:
          DoGreaterThanOrEquals(stack);
          break;
        }
      }
      return stack.Pop();
    }

    private void DoGreaterThan(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      BooleanDatum result;
      if (a == null) {
        result = new BooleanDatum(false);
      } else if (b == null) {
        result = new BooleanDatum(true);
      } else {
        result = new BooleanDatum(a.number > b.number);
      }
      stack.Push(result);
    }

    private void DoGreaterThanOrEquals(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      BooleanDatum result;
      if (a == null) {
        result = new BooleanDatum(false);
      } else if (b == null) {
        result = new BooleanDatum(true);
      } else {
        result = new BooleanDatum(a.number >= b.number);
      }
      stack.Push(result);
    }

    private void DoLessThan(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      BooleanDatum result;
      if (a == null) {
        result = new BooleanDatum(true);
      } else if (b == null) {
        result = new BooleanDatum(false);
      } else {
        result = new BooleanDatum(a.number < b.number);
      }
      stack.Push(result);
    }

    private void DoLessThanOrEquals(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      BooleanDatum result;
      if (a == null) {
        result = new BooleanDatum(true);
      } else if (b == null) {
        result = new BooleanDatum(false);
      } else {
        result = new BooleanDatum(a.number <= b.number);
      }
      stack.Push(result);
    }

    private void DoMinimum(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      NumericDatum result;
      if (a == null) {
        if (b == null) {
          result = new NumericDatum(0);
        } else {
          result = new NumericDatum(b.number);
        }
      } else if (b == null) {
        result = new NumericDatum(a.number);
      } else {
        result = new NumericDatum(a.number < b.number ? a.number : b.number);
      }
      stack.Push(result);
    }

    private void DoMaximum(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      NumericDatum result;
      if (a == null) {
        if (b == null) {
          result = new NumericDatum(0);
        } else {
          result = new NumericDatum(b.number);
        }
      } else if (b == null) {
        result = new NumericDatum(a.number);
      } else {
        result = new NumericDatum(a.number > b.number ? a.number : b.number);
      }
      stack.Push(result);
    }

    private void DoAdd(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      NumericDatum result;
      if (a == null) {
        if (b == null) {
          result = new NumericDatum(0);
        } else {
          result = new NumericDatum(b.number);
        }
      } else if (b == null) {
        result = new NumericDatum(a.number);
      } else {
        result = new NumericDatum(a.number + b.number);
      }
      stack.Push(result);
    }

    private void DoSubtract(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      NumericDatum result;
      if (a == null) {
        if (b == null) {
          result = new NumericDatum(0);
        } else {
          result = new NumericDatum(-b.number);
        }
      } else if (b == null) {
        result = new NumericDatum(a.number);
      } else {
        result = new NumericDatum(a.number - b.number);
      }
      stack.Push(result);
    }

    private void DoMultiply(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      NumericDatum result;
      if (a == null) {
        if (b == null) {
          result = new NumericDatum(1);
        } else {
          result = new NumericDatum(b.number);
        }
      } else if (b == null) {
        result = new NumericDatum(a.number);
      } else {
        result = new NumericDatum(a.number * b.number);
      }
      stack.Push(result);
    }

    private void DoDivide(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      NumericDatum result;
      if (a == null) {
        if (b == null) {
          result = new NumericDatum(1);
        } else {
          result = new NumericDatum(b.number);
        }
      } else if (b == null) {
        result = new NumericDatum(a.number);
      } else {
        result = new NumericDatum(a.number / b.number);
      }
      stack.Push(result);
    }

    private void DoAnd(Stack<Datum> stack) {
      BooleanDatum b = stack.Pop() as BooleanDatum;
      BooleanDatum a = stack.Pop() as BooleanDatum;
      BooleanDatum result;
      if (a == null) {
        result = new BooleanDatum(false);
      } else if (b == null) {
        result = new BooleanDatum(false);
      } else {
        result = new BooleanDatum(a.value && b.value);
      }
      stack.Push(result);
    }

    private void DoOr(Stack<Datum> stack) {
      BooleanDatum b = stack.Pop() as BooleanDatum;
      BooleanDatum a = stack.Pop() as BooleanDatum;
      BooleanDatum result;
      if (a == null) {
        if (b == null) result = new BooleanDatum(false);
        else result = new BooleanDatum(b.value);
      } else if (b == null) {
        result = new BooleanDatum(a.value);
      } else {
        result = new BooleanDatum(a.value || b.value);
      }
      stack.Push(result);
    }

    private void DoModulo(Stack<Datum> stack) {
      NumericDatum b = stack.Pop() as NumericDatum;
      NumericDatum a = stack.Pop() as NumericDatum;
      NumericDatum result;
      if (a == null) {
        result = new NumericDatum(0);
      } else if (b == null) {
        result = new NumericDatum(a.number);
      } else {
        result = new NumericDatum(a.number % b.number);
      }
      stack.Push(result);
    }

    private void DoNot(Stack<Datum> stack) {
      Datum a = stack.Pop();
      stack.Push(new BooleanDatum(!a.ToBool()));
    }

    private void DoEqual(Stack<Datum> stack) {
      Datum a = stack.Pop();
      Datum b = stack.Pop();
      BooleanDatum result = new BooleanDatum(a.Equals(b));
      stack.Push(result);
    }

    private void DoNotEqual(Stack<Datum> stack) {
      Datum a = stack.Pop();
      Datum b = stack.Pop();
      BooleanDatum result = new BooleanDatum(!a.Equals(b));
      stack.Push(result);
    }
  }
}

