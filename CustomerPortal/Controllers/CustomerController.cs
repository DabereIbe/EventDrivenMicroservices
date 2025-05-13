using CustomerPortal.ApiClient;
using CustomerPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerPortal.Controllers;

public class CustomerController : Controller
{
    private readonly ICustomerApiWrapper _customerApiWrapper;

    public CustomerController(ICustomerApiWrapper customerApiWrapper)
    {
        _customerApiWrapper = customerApiWrapper;
    }

    public IActionResult Create()
    {
        var customer = new CustomerWithProduct
        {
            ProductId = new List<SelectListItem>()
        };

        foreach (var item in _customerApiWrapper.GetProductName())
            customer.ProductId.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.ProductId.ToString()
            });

        return View(customer);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CustomerWithProduct customerWithProduct)
    {
        var selectedProduct = customerWithProduct.SelectedProduct;

        var customer = new Customer
        {
            Name = customerWithProduct.Name,
            ProductId = selectedProduct,
            ItemInCart = customerWithProduct.ItemInCart
        };

        await _customerApiWrapper.CreateCustomer(customer);

        return RedirectToAction("List");
    }


    public async Task<IActionResult> List()
    {
        var customerWithProducts = new List<CustomerWithProduct>();
        var customers = await _customerApiWrapper.GetCustomers();

        var products = _customerApiWrapper.GetProductName();

        foreach (var customer in customers)
            customerWithProducts.Add(
                new CustomerWithProduct
                {
                    Name = customer.Name,
                    ProductName = products.FirstOrDefault(x => x.ProductId == customer.ProductId).Name,
                    ItemInCart = customer.ItemInCart
                }
            );

        return View(customerWithProducts);
    }
}