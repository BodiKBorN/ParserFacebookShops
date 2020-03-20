using ParserFacebookShops.Models.Abstractions.Generics;
using System;
using System.Diagnostics;


namespace ParserFacebookShops.Models.Implementation.Generics
{
    [DebuggerDisplay("Success={Success} Data={Data}")]
    internal class Result<TData> : IResult<TData>
    {
        public TData Data { get; private set; }

        public string Message { get; private set; }

        public bool Success { get; private set; }

        public Exception Exception { get; private set; }

        private Result()
        {
        }

        public static Result<TData> CreateSuccess(TData data)
            => new Result<TData>
            {
                Data = data,
                Success = true,
            };

        public static Result<TData> CreateFailed(string message, Exception exception = null)
            => new Result<TData>
            {
                Message = message,
                Exception = exception,
            };

        public static Result<TData> CreateFailed(params string[] messages)
            => new Result<TData>
            {
                Message = string.Join('\n', messages)
            };
    }
}