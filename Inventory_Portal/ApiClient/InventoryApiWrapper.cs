namespace Inventory_Portal.ApiClient
{
    public interface IInventoryApiWrapper
    {
        Task CreateProductAsync(Product product);
        Task<List<Product>> GetProductsAsync();
    }

    public class InventoryApiWrapper : IInventoryApiWrapper
    {
        private readonly InventoryApi _inventoryApi;

        public InventoryApiWrapper()
        {
            _inventoryApi = new InventoryApi("http://edainventory_api:5004", new HttpClient());
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var inventoryItems = await _inventoryApi.GetProductsAsync(new CancellationToken());
            return inventoryItems.ToList();
        }

        public async Task CreateProductAsync(Product product)
        {
            await _inventoryApi.PostProductAsync(product);
        }
    }
}
