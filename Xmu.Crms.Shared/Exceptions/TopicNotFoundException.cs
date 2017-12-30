using System;
using System.Runtime.Serialization;

namespace Xmu.Crms.Shared.Exceptions
{
    [Serializable]
    public class TopicNotFoundException : ArgumentOutOfRangeException
    {
        private string alertInfo;
        public TopicNotFoundException()
        {
            alertInfo = "未找到话题";
        }

        public TopicNotFoundException(string paramName) : base(paramName)
        {
            alertInfo = paramName;
        }

        public TopicNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TopicNotFoundException(string paramName, string message) : base(paramName, message)
        {
        }

        public TopicNotFoundException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        protected TopicNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string GetAlertInfo()
        {
            return alertInfo;
        }
    }
}