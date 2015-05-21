using System;
using System.Text;
using System.Collections.Generic;

namespace gnoam.file
{
  public class LogicalLineChunk {
    public string content;
    public int lineNumberInOriginalFile;
    public int startingColumnInOriginalFile;

    public LogicalLineChunk(string content, int lineNumberInOriginalFile) 
      : this(content, lineNumberInOriginalFile, 0) {
    }

    public LogicalLineChunk(string content, int lineNumberInOriginalFile, int startingColumnInOriginalFile) {
      this.content = content;
      this.lineNumberInOriginalFile = lineNumberInOriginalFile;
      this.startingColumnInOriginalFile = startingColumnInOriginalFile;
    }
  }

  public class LogicalLine {
    public List<LogicalLineChunk> chunks;
    public int startingLineNumberInOriginalFile;
    public int Length {
      get;
      private set;
    }

    public LogicalLine() {
      Length = 0;
      chunks = new List<LogicalLineChunk>();
    }

    public bool IsEmpty {
      get { return chunks.Count == 0; }
    }

    /** Adds a line from the original file. */
    public void AddNewLine(string lineFromOriginalFile, int lineNumberInOriginalFile) {
      string trimmed = lineFromOriginalFile.Trim();
      if (trimmed.Length == 0) return;

      // If this is our first line, store where we started.
      if (IsEmpty) startingLineNumberInOriginalFile = lineNumberInOriginalFile;

      // Find out how much whitespace we have to strip.
      int i = 0;
      while (char.IsWhiteSpace(lineFromOriginalFile[i])) ++i;

      Length += trimmed.Length;
      chunks.Add(new LogicalLineChunk(trimmed, i));
    }

    public override string ToString() {
      StringBuilder builder = new StringBuilder();
      builder.Append(startingLineNumberInOriginalFile);
      builder.Append(": ");
      foreach (var chunk in chunks) {
        builder.Append(chunk.content);
        builder.Append(" ");
      }
      return builder.ToString();
    }
  }

  /** Takes the text input and splits it into logical lines.
   * 
   * In Gnoam files, lines can continue if they are indented.
   * Lines with an initial indent are always continuations,
   * unless they are preceeded with a blank line, in which case
   * they are a syntax error.
   */
  public static class LineSplitter
  {
    public static List<LogicalLine> getLines(string fileContent, string fileName="<unknown file>") {
      var result = new List<LogicalLine>();

      var fileLines = fileContent.Split(new char[] {'\n','\r'});

      int fileLineNumber = 1;
      bool canHaveFollowOnLine = false;

      LogicalLine logicalLine = new LogicalLine();

      foreach (string fileLine in fileLines) {
        string trimmedFileLine = fileLine.Trim();
        if (trimmedFileLine.Length == 0) {
          canHaveFollowOnLine = false;
        } else if (trimmedFileLine[0] == '#') {
          // Ignore comments.
        } else if (char.IsWhiteSpace(fileLine[0])) {
          // We have a follow-on line, make sure we follow on from something.
          if (!canHaveFollowOnLine) {
            string msg = "Can't have a follow-on line at the start, or after a blank line.";
            throw new gnoam.file.SyntaxError(msg, fileName, fileLineNumber);
          }

          // Add the follow on line to the buffer.
          logicalLine.AddNewLine(fileLine, fileLineNumber);
        } else {
          // We have a new line, so clear the buffer.
          if (!logicalLine.IsEmpty) {
            result.Add(logicalLine);
            logicalLine = new LogicalLine();
          }

          // Add this first part of the line to the buffer.
          logicalLine.AddNewLine(fileLine, fileLineNumber);
          canHaveFollowOnLine = true;
        }
        ++fileLineNumber;
      }
      // Add any remaining content.
      if (!logicalLine.IsEmpty) result.Add(logicalLine);

      return result;
    }
  }
}

