using System;

namespace AppSettings
{
    /// <summary>
    /// Provides a specific error related to settings parsing
    /// </summary>

    public class ParseException : Exception
    {
        public ParseException(string Message) : base(Message) { }
    }
}
