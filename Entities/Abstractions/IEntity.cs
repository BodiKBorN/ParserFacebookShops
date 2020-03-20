namespace ParserFacebookShops.Entities.Abstractions
{
    public interface IEntity<T>
    {
        T Id { get; set; }
    }
}