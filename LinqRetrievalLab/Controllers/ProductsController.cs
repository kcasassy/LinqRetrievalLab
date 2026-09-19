using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinqRetrievalLab.Data;
using LinqRetrievalLab.Models;

namespace LinqRetrievalLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly CatalogContext _context;

        public ProductsController(CatalogContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(
            int id,
            Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                return BadRequest("Product name is required.");
            }

            if (product.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (product.Stock < 0)
            {
                return BadRequest("Stock cannot be negative.");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == product.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (product == null)
            {
                return BadRequest("Product is required.");
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                return BadRequest("Product name is required.");
            }

            if (product.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (product.Stock < 0)
            {
                return BadRequest("Stock cannot be negative.");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == product.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            product.Name = product.Name.Trim();

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(
            int categoryId)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == categoryId);

            if (!categoryExists)
            {
                return NotFound("Category does not exist.");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();

            return products;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Search string is required.");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => EF.Functions.Like(
                    p.Name,
                    $"%{name.Trim()}%"))
                .ToListAsync();

            return products;
        }

        [HttpGet("price-range")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByPriceRange(
            decimal lower,
            decimal upper)
        {
            if (lower < 0 || upper < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (lower > upper)
            {
                return BadRequest(
                    "Lower price cannot be greater than upper price.");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Price >= lower && p.Price <= upper)
                .ToListAsync();

            return products;
        }

        [HttpGet("stock-total")]
        public async Task<ActionResult<int>> GetTotalStock()
        {
            var totalStock = await _context.Products
                .SumAsync(p => p.Stock);

            return totalStock;
        }

        [HttpGet("average-price")]
        public async Task<ActionResult<decimal>> GetAveragePrice()
        {
            var productCount = await _context.Products.CountAsync();

            if (productCount == 0)
            {
                return 0;
            }

            var averagePrice = await _context.Products
                .AverageAsync(p => p.Price);

            return averagePrice;
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetProductCount()
        {
            var count = await _context.Products.CountAsync();

            return count;
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}