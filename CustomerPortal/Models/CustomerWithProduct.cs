using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerPortal.Models;

public class CustomerWithProduct
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Guid SelectedProduct { get; set; }

    public string ProductName { get; set; }

    public List<SelectListItem> ProductId { get; set; }

    public int ItemInCart { get; set; }
}