using System;
using System.Runtime.Serialization;

namespace Xmu.Crms.Shared.Exceptions
{
    [Serializable]
    public class GroupNotFoundException : ArgumentOutOfRangeException
    {
        private string alertInfo;
        public GroupNotFoundException()
        {
            alertInfo = "未找到小组";
        }

        public GroupNotFoundException(string paramName) : base(paramName)
        {
            alertInfo = paramName;
        }

        public GroupNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public GroupNotFoundException(string paramName, string message) : base(paramName, message)
        {
        }

        public GroupNotFoundException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        protected GroupNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string GetAlertInfo()
        {
            return alertInfo;
        }
    }
}