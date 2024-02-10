using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachineAPI.Data;
using VendingMachineAPI.Dtos;
using VendingMachineAPI.Models;

namespace VendingMachineAPI.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        readonly ApiContext _context;

        public ProductsController(ApiContext context)
        {
            _context = context;
        }

        //Get api/Products
        [HttpGet("GetProducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            if(products.Count == 0) 
            {
                return NotFound("No Products Found.");
            }
            return Ok(products);
        }

        //Get Product by ID
        [HttpGet("GetProductById")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.ProductId == id);
            if (product == null)
            {
                return BadRequest("Unfortunately we don't sell this product.");
            }
            else if(product.AmountAvailable == 0)
            {
                return NotFound("Out of stock!");
            }
            return Ok(product);
        }

        //Post Product
        [Authorize]
        [HttpPost("AddProduct")]
        public async Task<ActionResult<Product>> AddProduct(NewProductDto product)
        {
            if (!User.IsInRole("Seller"))
            {
                return Forbid("Only sellers can add products!");
            }
            var seller = _context.Users.FirstOrDefault(x => x.Username == User.Identity.Name);

            var productCheck = _context.Products.FirstOrDefault(x => x.SellerId == seller.UserId && x.ProductName == product.ProductName);

            if (productCheck != null) 
            {
                return Conflict("You already have this product.");
            }

            _context.Products.Add(new Product 
            {
                ProductName = product.ProductName,
                Price = (int)product.Price,
                AmountAvailable = (int)product.AmountAvailable,
                SellerId = seller.UserId
            });

            await _context.SaveChangesAsync();

            return Ok($"Product {product.ProductName} is added with amount {product.AmountAvailable}");
        }

        //Put
        [Authorize]
        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int productId, NewProductDto updatedProduct)
        {
            var product = _context.Products.FirstOrDefault(x => x.ProductId == productId);
            
            if (product == null)
            {
                return BadRequest("Product not found");
            }

            var seller = _context.Users.FirstOrDefault(x => x.UserId == product.SellerId);

            if (User.Identity.Name != seller.Username)
            {
                return Forbid("You are not allowed to update other users info!");
            }
 
           
            foreach (var propertyInfo in typeof(NewProductDto).GetProperties())
            {
                var newValue = propertyInfo.GetValue(updatedProduct);
                var currentValue = product.GetType().GetProperty(propertyInfo.Name)?.GetValue(product);

                if (newValue != null && !newValue.Equals(currentValue))
                {
                    product.GetType().GetProperty(propertyInfo.Name)?.SetValue(product, newValue);
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Product Updated Successfully");
        }

        //Delete
        [Authorize]
        [HttpDelete("DeleteProduct")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x=>x.ProductId == id);
            if (product == null)
            {
                return BadRequest("Product not found");
            }

            var userId = _context.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
            
            if (product.SellerId != userId.UserId)
            {
                return Forbid("You are not authorized to delete this product");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok("Product Deleted Successfully");
        }
    }
}
