using System;
using System.Runtime.Serialization;

namespace IAutoM8.Global.Exceptions
{
    public class ForbiddenException : Exception
    {
        public bool ShouldRedirect { get; set; }
        public ForbiddenException(bool shouldRedirect = false)
        {
            ShouldRedirect = shouldRedirect;
        }

        public ForbiddenException(string message, bool shouldRedirect = false) : base(message)
        {
            ShouldRedirect = shouldRedirect;
        }

        public ForbiddenException(string message, Exception innerException, bool shouldRedirect = false) : base(message, innerException)
        {
            ShouldRedirect = shouldRedirect;
        }

        protected ForbiddenException(SerializationInfo info, StreamingContext context, bool shouldRedirect = false) : base(info, context)
        {
            ShouldRedirect = shouldRedirect;
        }
    }
}
