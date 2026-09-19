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
    public class CategoriesController : ControllerBase
    {
        private readonly CatalogContext _context;

        public CategoriesController(CatalogContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(
            int id,
            Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return BadRequest("Category name is required.");
            }

            category.Name = category.Name.Trim();

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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
        public async Task<ActionResult<Category>> PostCategory(
            Category category)
        {
            if (category == null)
            {
                return BadRequest("Category is required.");
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return BadRequest("Category name is required.");
            }

            category.Name = category.Name.Trim();

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.Id },
                category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetCategoryProducts(
            int id)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == id);

            if (!categoryExists)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id)
                .ToListAsync();

            return products;
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories
                .Any(e => e.Id == id);
        }
    }
}