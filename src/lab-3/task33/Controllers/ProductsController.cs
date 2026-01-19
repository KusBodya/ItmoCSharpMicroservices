using Microsoft.AspNetCore.Mvc;
using Task33.Models;

namespace Task33.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(IOrdersClient ordersClient, ILogger<ProductsController> logger)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product: {Name} with price {Price}", request.Name, request.Price);

        ProductDto product = await ordersClient.CreateProductAsync(
            request.Name,
            (decimal)request.Price,
            cancellationToken);

        return Ok(product);
    }
}
