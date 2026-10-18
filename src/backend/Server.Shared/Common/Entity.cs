namespace Server.Shared.Common;

/// <summary>
/// Base type with properties all entities share.
/// </summary>
public class Entity
{
    /// <summary>
    /// Identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// When the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the entity was updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}