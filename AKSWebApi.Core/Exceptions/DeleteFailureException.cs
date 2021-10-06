using System;
using System.Runtime.Serialization;

namespace AKSWebApi.Core.Exceptions
{
    [Serializable]
    // Important: This attribute is NOT inherited from Exception, and MUST be specified 
    // otherwise serialization will fail with a SerializationException stating that
    // "Type X in Assembly Y is not marked as serializable."
    public class DeleteFailureException : Exception
    {
        public DeleteFailureException()
        {
        }

        public DeleteFailureException(string message)
            : base(message)
        {
        }

        public DeleteFailureException(string name, object key, string message)
            : base($"Deletion of entity \"{name}\" ({key}) failed. {message}")
        {
        }

        // Without this constructor, deserialization will fail
        protected DeleteFailureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
