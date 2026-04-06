using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly DataStore _dataStore;

    public ProductsController(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        return Ok(_dataStore.Products);
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        var product = _dataStore.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound();

        product.Category = _dataStore.Categories.FirstOrDefault(c => c.Id == product.CategoryId);
        return Ok(product);
    }
}
