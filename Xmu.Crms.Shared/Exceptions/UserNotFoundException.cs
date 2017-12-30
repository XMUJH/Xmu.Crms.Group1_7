using System;
using System.Runtime.Serialization;

namespace Xmu.Crms.Shared.Exceptions
{
    [Serializable]
    public class UserNotFoundException : ArgumentOutOfRangeException
    {
        private string alertInfo;
        public UserNotFoundException()
        {
            alertInfo = "未找到对应用户";
        }

        public UserNotFoundException(string paramName) : base(paramName)
        {
            alertInfo = paramName;
        }

        public UserNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UserNotFoundException(string paramName, string message) : base(paramName, message)
        {
        }

        public UserNotFoundException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        protected UserNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string GetAlertInfo()
        {
            return alertInfo;
        }
    }
}