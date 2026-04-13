using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

/// <summary>
/// Base controller providing common CRUD operations for entities.
/// Eliminates duplication across entity-specific controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseEntityController<TEntity> : ControllerBase where TEntity : class
{
    protected readonly DataStore DataStore;

    protected BaseEntityController(DataStore dataStore)
    {
        DataStore = dataStore;
    }

    /// <summary>
    /// Gets the collection of entities from the data store.
    /// Override in derived classes to specify the correct collection.
    /// </summary>
    protected abstract IEnumerable<TEntity> GetCollection();

    /// <summary>
    /// Finds an entity by its ID.
    /// Override in derived classes to provide entity-specific lookup logic.
    /// </summary>
    protected abstract TEntity? FindById(int id);

    /// <summary>
    /// Gets all entities of this type.
    /// </summary>
    [HttpGet]
    public virtual ActionResult<IEnumerable<TEntity>> GetAll()
    {
        return Ok(GetCollection());
    }

    /// <summary>
    /// Gets a single entity by ID with standard NotFound handling.
    /// </summary>
    [HttpGet("{id}")]
    public virtual ActionResult<TEntity> GetById(int id)
    {
        var entity = FindById(id);
        if (entity == null)
            return NotFound();

        return Ok(entity);
    }
}
