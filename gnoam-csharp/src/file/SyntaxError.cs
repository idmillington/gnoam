using System;

namespace gnoam.file
{
  public class SyntaxError : Exception {
    public readonly string fileName;
    public readonly int sourceLine;

    public SyntaxError(string fileName, int sourceLine) : base() {
      this.fileName = fileName;
      this.sourceLine = sourceLine;
    }

    public SyntaxError(string message, string fileName, int sourceLine) : base(message) {
      this.fileName = fileName;
      this.sourceLine = sourceLine;
    }

    public SyntaxError(string message, string fileName, int sourceLine, Exception inner) : base(message, inner) {
      this.fileName = fileName;
      this.sourceLine = sourceLine;
    }
  }
}

