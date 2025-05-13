using EDA_Customer.Data;
using EDA_Customer.RabbitMq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.RabbitMQ;
using Shared.Settings;

namespace EDA_Customer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerDbContext context;
        private readonly IRabbitMqUtil rabbitMqUtil;
        private readonly RabbitMqSettings rabbitMqSettings;

        public CustomerController(CustomerDbContext context, IRabbitMqUtil rabbitMqUtil, RabbitMqSettings rabbitMqSettings)
        {
            this.context = context;
            this.rabbitMqUtil = rabbitMqUtil;
            this.rabbitMqSettings = rabbitMqSettings;
        }

        [HttpGet]
        [Route("/customers")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await context.Customers.ToListAsync();
        }
        
        [HttpGet]
        [Route("/products")]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            return context.Products.ToList();
        }

        [HttpPost]
        public async Task PostCustomer(Customer customer)
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var product = JsonConvert.SerializeObject(new
            {
                customer.ProductId,
                TotalBought = customer.ItemInCart,
            });
            await rabbitMqUtil.PublishMessageQueue(rabbitMqSettings.CustomerRoutingKey, product);
        }
    }
}
