using BackendAPI.Entities.Enums;
using System;

namespace BackendAPI.Exceptions
{
    public class CustomException : Exception
    {
        public ErrorType ErrorType { get; set; }

        public CustomException(ErrorType errorType) : base()
        {
            this.ErrorType = errorType;
        }

        public CustomException(string message, ErrorType errorType) : base(message)
        {
            this.ErrorType = errorType;
        }

        public CustomException()
        {
            this.ErrorType = ErrorType.OTHER;
        }

        public CustomException(string message) : base(message)
        {
            this.ErrorType = ErrorType.OTHER;
        }
    }
}
