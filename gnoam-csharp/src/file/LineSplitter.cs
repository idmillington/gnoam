using System;
using System.Text;
using System.Collections.Generic;

namespace gnoam.file
{
  public struct LogicalLine {
    public string line;
    public int lineNumberInOriginalFile;
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
    private static void _addLogicalLineFromBuffer(List<LogicalLine> result, List<string> buffer, int fileLine) {
      if (buffer.Count == 0) return;

      // Join the string
      StringBuilder builder = new StringBuilder();
      foreach (string line in buffer) {
        builder.Append(buffer);
        builder.Append(" ");
      }
      string lineContent = builder.ToString();

      // Replace whitespace (TODO - replace internal whitespace using regex)
      lineContent = lineContent.Trim();

      // Create and store the result.
      LogicalLine logicalLine = new LogicalLine();
      logicalLine.line = lineContent;
      logicalLine.lineNumberInOriginalFile = fileLine;
      result.Add(logicalLine);
    }

    public static List<LogicalLine> getLines(string fileContent, string fileName=null) {
      var result = new List<LogicalLine>();

      var fileLines = fileContent.Split(new char[] {'\n','\r'});

      bool lastLineWasBlank = false;
      int fileLineNumber = 1;
      int currentLogicalLineStartedOnFileLine = 1;
      List<string> currentLogicalLineBuffer = new List<String>();
      foreach (string fileLine in fileLines) {
        if (fileLine.Trim().Length == 0) {
          lastLineWasBlank = true;
        } else if (char.IsWhiteSpace(fileLine[0])) {
          if (lastLineWasBlank) {
            string msg = "Can't have a follow-on line after a blank line.";
            throw new gnoam.file.SyntaxError(msg, fileName, fileLineNumber);
          }
          // Add the follow on line to the buffer, unless it is a comment.
          string trimmedFileLine = fileLine.TrimStart();
          if (trimmedFileLine[0] != '#') currentLogicalLineBuffer.Add(trimmedFileLine);
          lastLineWasBlank = false;
        } else {
          // We have a new line, so clear the buffer.
          _addLogicalLineFromBuffer(result, currentLogicalLineBuffer, currentLogicalLineStartedOnFileLine);

          // Start anew.
          currentLogicalLineStartedOnFileLine = fileLineNumber;
          currentLogicalLineBuffer.Clear();
          lastLineWasBlank = false;
        }
        ++fileLineNumber;
      }
      // Clear any remaining lines from the buffer.
      _addLogicalLineFromBuffer(result, currentLogicalLineBuffer, currentLogicalLineStartedOnFileLine);

      return result;
    }
  }
}

