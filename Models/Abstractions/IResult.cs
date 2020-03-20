using System;

namespace ParserFacebookShops.Models.Abstractions
{
    public interface IResult
    {
        string Message { get; }

        bool Success { get; }

        Exception Exception { get; }
    }
}