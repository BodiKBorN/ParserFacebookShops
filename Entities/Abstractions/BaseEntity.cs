using System;

namespace ParserFacebookShops.Entities.Abstractions
{
    public class BaseEntity : IEntity<int>, IDeletableEntity
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}