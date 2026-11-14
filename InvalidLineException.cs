// Asked for: extra uppgift 1 (utbyggnad U3-1), an exception type of our own that inherits from Exception.
// Thrown when a line in the file cannot become an item. It remembers which line it was,
// so the message can tell the user where in the file the problem is.
class InvalidLineException : Exception
{
    public int LineNumber { get; }

    public InvalidLineException(int lineNumber, string message) : base(message)
    {
        LineNumber = lineNumber;
    }

    // Asked for: utbyggnad U3-2. The same, but it also keeps the exception that caused it (for
    // example Item's ArgumentException) as the inner exception, so nothing about the cause is lost.
    public InvalidLineException(int lineNumber, string message, Exception inner) : base(message, inner)
    {
        LineNumber = lineNumber;
    }
}
