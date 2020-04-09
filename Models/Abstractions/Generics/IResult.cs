namespace ParserFacebookShops.Models.Abstractions.Generics
{
    public interface IResult<out TData> : IResult
    {
        TData Data { get; }
    }
}