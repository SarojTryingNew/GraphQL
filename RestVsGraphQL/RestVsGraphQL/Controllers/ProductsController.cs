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
}
