namespace ParserFacebookShops.Entities.Abstractions
{
    public interface IDeletableEntity
    {
        bool IsDeleted { get; set; }
    }
}