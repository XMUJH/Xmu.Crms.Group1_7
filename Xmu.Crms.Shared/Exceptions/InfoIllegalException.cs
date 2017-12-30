using System;
using System.Runtime.Serialization;

namespace Xmu.Crms.Shared.Exceptions
{
    [Serializable]
    public class InfoIllegalException : ArgumentOutOfRangeException
    {
        private string alertInfo;
        public InfoIllegalException()
        {
            alertInfo = "输入信息不合法";
        }

        public InfoIllegalException(string paramName) : base(paramName)
        {
            alertInfo = paramName;
        }

        public InfoIllegalException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InfoIllegalException(string paramName, string message) : base(paramName, message)
        {
        }

        public InfoIllegalException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        protected InfoIllegalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string GetAlertInfo()
        {
            return alertInfo;
        }
    }
}
