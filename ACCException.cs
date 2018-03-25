using System;
using System.Runtime.Serialization;

namespace CM3D2.AlwaysColorChangeEx.Plugin {
    /// <summary>
    /// Description of ACCException.
    /// </summary>
    public class ACCException : Exception {
        public ACCException() { }

        public ACCException(string message) : base(message) { }

        public ACCException(string message, Exception innerException) : base(message, innerException) { }

        // This constructor is needed for serialization.
        protected ACCException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}