using System;
using System.Runtime.Serialization;

namespace Xmu.Crms.Shared.Exceptions
{
    [Serializable]
    public class ClassNotFoundException : ArgumentOutOfRangeException
    {
        private string alertInfo;
        public ClassNotFoundException()
        {
            alertInfo = "未找到班级";
        }

        public ClassNotFoundException(string paramName) : base(paramName)
        {
            alertInfo = paramName;
        }

        public ClassNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ClassNotFoundException(string paramName, string message) : base(paramName, message)
        {
        }

        public ClassNotFoundException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        protected ClassNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string GetAlertInfo()
        {
            return alertInfo;
        }
    }
}