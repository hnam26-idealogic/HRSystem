namespace HRSystem.API.Models.Domain
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted => DeletedAt.HasValue;
    }
}
