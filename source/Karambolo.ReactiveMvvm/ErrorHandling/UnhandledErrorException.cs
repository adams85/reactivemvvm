using System;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public class UnhandledErrorException : Exception
    {
        public UnhandledErrorException(string message, Exception exception) : base(message, exception) { }
    }
}
