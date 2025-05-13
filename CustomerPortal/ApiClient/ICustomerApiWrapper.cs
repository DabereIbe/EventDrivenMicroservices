namespace CustomerPortal.ApiClient;

public interface ICustomerApiWrapper
{
    Task<ICollection<Product>> GetProduct();

    IEnumerable<Product> GetProductName();

    Task<ICollection<Customer>> GetCustomers();
    Task CreateCustomer(Customer customer, CancellationToken cancellationToken = default);
}