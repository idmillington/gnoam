using System;
using System.Collections.Generic;

namespace gnoam.file
{
  public enum TokenType {
    OutputContent,
    StartOfTag,
    EndOfTag,
    Name,
    Number,
    Equals,
    Semicolon,
    OpenParens,
    CloseParens
  }

  public class Token {
    public TokenType type;
    public LogicalLineChunk chunk;
    public int offsetIntoChunk;
    public int length;
  }

  /** Takes a logical line and tokenizes it. */
  public static class Tokenizer {
    /*
    public static List<Token> tokenize(LogicalLine line) {
      // Symbols we can't use in a name.
      string specials = "[]\";=()\\";

      bool isInTag = false;
      bool isEscaped = false;
      int position = 0;
      int lineLength = line.Length;

      while (position < lineLength) {
        if (!isInTag) {
          // Find the next [ without a preceding \
        } else {
          // Skip whitespace
          // What is next?
          // A letter -> we have a name
          // A number -> we have a number
          // A special -> we have the corresponding special
        }
        ++position;
      }
    }
    */
  }
}

