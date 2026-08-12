namespace EduTrack.Domain.Common;

/// <summary>Base class: every entity has an integer identity key.</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
