﻿using System;
using System.Runtime.Serialization;

namespace Xmu.Crms.Shared.Exceptions
{
    [Serializable]
    public class FixGroupNotFoundException : ArgumentOutOfRangeException
    {
        private string alertInfo;
        public FixGroupNotFoundException()
        {
            alertInfo = "未找到小组";
        }

        public FixGroupNotFoundException(string paramName) : base(paramName)
        {
            alertInfo = paramName;
        }

        public FixGroupNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public FixGroupNotFoundException(string paramName, string message) : base(paramName, message)
        {
        }

        public FixGroupNotFoundException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }

        protected FixGroupNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string GetAlertInfo()
        {
            return alertInfo;
        }
    }
}