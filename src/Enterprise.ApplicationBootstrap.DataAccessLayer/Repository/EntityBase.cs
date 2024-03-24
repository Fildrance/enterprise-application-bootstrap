namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary> Base class for EF entities. Simplifies a lot of repository related logic. </summary>
/// <typeparam name="TId"> Type of Id to be used by entity. </typeparam>
public class EntityBase<TId>
{
    /// <summary> Id for entity (primary key). </summary>
    public TId Id { get; set; }
}