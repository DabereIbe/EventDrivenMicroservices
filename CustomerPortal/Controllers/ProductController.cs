using CustomerPortal.ApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CustomerPortal.Controllers;

public class ProductController : Controller
{
    private readonly ICustomerApiWrapper _customerApiWrapper;

    public ProductController(ICustomerApiWrapper customerApiWrapper)
    {
        _customerApiWrapper = customerApiWrapper;
    }

    public async Task<IActionResult> List()
    {
        var products = await _customerApiWrapper.GetProduct();
        return View(products);
    }
}