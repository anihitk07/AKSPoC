using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AKSWebApi.Core.ErrorHandling
{
    [Serializable]
    public class ApiException : Exception
    {
        public ApiException()
        {
        }

        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ApiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
