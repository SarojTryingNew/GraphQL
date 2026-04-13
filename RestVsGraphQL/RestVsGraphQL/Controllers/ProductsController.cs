using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseEntityController<Product>
{
    public ProductsController(DataStore dataStore) : base(dataStore)
    {
    }

    protected override IEnumerable<Product> GetCollection() => DataStore.Products;

    protected override Product? FindById(int id)
    {
        var product = DataStore.Products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            product.Category = DataStore.Categories.FirstOrDefault(c => c.Id == product.CategoryId);
        }
        return product;
    }

    /// <summary>
    /// Batch endpoint for GraphQL DataLoader efficiency.
    /// GET /api/products/batch?ids=1,2,3,4,5
    /// </summary>
    [HttpGet("batch")]
    public ActionResult<IEnumerable<Product>> GetProductsByIds([FromQuery] int[] ids)
    {
        var products = DataStore.Products.Where(p => ids.Contains(p.Id));
        return Ok(products);
    }

    /// <summary>
    /// Batch endpoint for GraphQL DataLoader efficiency.
    /// GET /api/products/by-categories?categoryIds=1,2,3
    /// </summary>
    [HttpGet("by-categories")]
    public ActionResult<IEnumerable<Product>> GetProductsByCategoryIds([FromQuery] int[] categoryIds)
    {
        var products = DataStore.Products.Where(p => categoryIds.Contains(p.CategoryId));
        return Ok(products);
    }
}
