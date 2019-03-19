using System;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public abstract class ObservedErrorException : Exception
    {
        protected ObservedErrorException(object sourceObject, Exception exception) : base(null, exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            SourceObject = sourceObject;
        }

        public object SourceObject { get; }
    }
}
