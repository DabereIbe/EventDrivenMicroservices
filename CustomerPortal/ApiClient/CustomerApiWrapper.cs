namespace CustomerPortal.ApiClient;

public class CustomerApiWrapper : ICustomerApiWrapper
{
    private readonly CustomerApi _customerApi; 

    public CustomerApiWrapper()
    {
        _customerApi = new CustomerApi("http://edacustomer_api:5003", new HttpClient());
    }

    public async Task<ICollection<Product>> GetProduct()
    {
        return await _customerApi.ProductsAsync();
    }

    public async Task<ICollection<Customer>> GetCustomers()
    {
        return await _customerApi.CustomersAsync();
    }

    public IEnumerable<Product> GetProductName()
    {
        return _customerApi.ProductsAsync().Result;
    }


    public async Task CreateCustomer(Customer customer, CancellationToken cancellationToken = default)
    {
        await _customerApi.CustomerAsync(customer, cancellationToken);
    }
}