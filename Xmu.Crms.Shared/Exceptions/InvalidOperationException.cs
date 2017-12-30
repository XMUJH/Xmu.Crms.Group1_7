using System;
using System.Runtime.Serialization;

namespace Xmu.Crms.Shared.Exceptions
{
    [Serializable]
    public class InvalidOperationException : ArgumentOutOfRangeException
    {
        private string alertInfo;
        public InvalidOperationException()
        {
            alertInfo = "该操作不合法";
        }

        public InvalidOperationException(string paramName) : base(paramName)
        {
            alertInfo = paramName;
        }

        public InvalidOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidOperationException(string paramName, string message) : base(paramName, message)
        {
        }

        public InvalidOperationException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        protected InvalidOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string GetAlertInfo()
        {
            return alertInfo;
        }
    }
}
