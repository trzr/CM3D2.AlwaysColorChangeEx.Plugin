/*
 */
using System;
using System.Runtime.Serialization;

namespace CM3D2.AlwaysColorChange.Plugin
{
    /// <summary>
    /// Description of ACCException.
    /// </summary>
    public class ACCException : Exception, ISerializable
    {
        public ACCException()
        {
        }

         public ACCException(string message) : base(message)
        {
        }

        public ACCException(string message, Exception innerException) : base(message, innerException)
        {
        }

        // This constructor is needed for serialization.
        protected ACCException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}