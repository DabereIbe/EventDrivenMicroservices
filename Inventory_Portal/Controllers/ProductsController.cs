using Inventory_Portal.ApiClient;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Portal.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IInventoryApiWrapper inventoryApiWrapper;

        public ProductsController(IInventoryApiWrapper inventoryApiWrapper)
        {
            this.inventoryApiWrapper = inventoryApiWrapper;
        }

        public async Task<IActionResult> List()
        {
            var products = await inventoryApiWrapper.GetProductsAsync();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var product = new Product();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await inventoryApiWrapper.CreateProductAsync(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }
    }
}
