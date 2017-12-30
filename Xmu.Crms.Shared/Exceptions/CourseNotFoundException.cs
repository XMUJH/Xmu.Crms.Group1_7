using System;
using System.Runtime.Serialization;

namespace Xmu.Crms.Shared.Exceptions
{
    [Serializable]
    public class CourseNotFoundException : ArgumentOutOfRangeException
    {
        private string alertInfo;
        public CourseNotFoundException()
        {
            alertInfo = "未找到课程";
        }

        public CourseNotFoundException(string paramName) : base(paramName)
        {
            alertInfo = paramName;
        }

        public CourseNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CourseNotFoundException(string paramName, string message) : base(paramName, message)
        {
        }

        public CourseNotFoundException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        protected CourseNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string GetAlertInfo()
        {
            return alertInfo;
        }

    }
}