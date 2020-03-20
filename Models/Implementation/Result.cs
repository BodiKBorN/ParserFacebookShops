using ParserFacebookShops.Models.Abstractions;
using System;

namespace ParserFacebookShops.Models.Implementation
{
    internal class Result : IResult
    {
        public string Message { get; private set; }

        public bool Success { get; private set; }

        public Exception Exception { get; private set; }

        private Result()
        {
        }

        public static Result CreateSuccess()
            => new Result
            {
                Success = true,
            };

        public static Result CreateFailed(string message, Exception exception = null)
            => new Result
            {
                Message = message,
                Exception = exception,
            };

        public static Result CreateFailed(params string[] messages)
            => new Result
            {
                Message = string.Join('\n', messages),
            };
    }
}