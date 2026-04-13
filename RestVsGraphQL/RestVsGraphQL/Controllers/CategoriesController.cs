using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : BaseEntityController<Category>
{
    public CategoriesController(DataStore dataStore) : base(dataStore)
    {
    }

    protected override IEnumerable<Category> GetCollection() => DataStore.Categories;

    protected override Category? FindById(int id) => DataStore.Categories.FirstOrDefault(c => c.Id == id);

    /// <summary>
    /// Batch endpoint for GraphQL DataLoader efficiency.
    /// GET /api/categories/batch?ids=1,2,3,4,5
    /// </summary>
    [HttpGet("batch")]
    public ActionResult<IEnumerable<Category>> GetCategoriesByIds([FromQuery] int[] ids)
    {
        var categories = DataStore.Categories.Where(c => ids.Contains(c.Id));
        return Ok(categories);
    }
}
