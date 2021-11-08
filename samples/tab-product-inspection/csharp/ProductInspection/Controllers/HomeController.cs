using ProductInspection.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;

namespace ProductInspection.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Creating a static list of item that will be used for product inspection.
        /// </summary>
        public static List<ProductDetails> ProductList = new List<ProductDetails>()
        {
            new ProductDetails(){ProductId = "01DU890", ProductName = "Desktop", Image = "", Status = ""},
            new ProductDetails(){ProductId = "01PM998", ProductName = "Mobile", Image = "", Status = ""},
            new ProductDetails(){ProductId = "01SD001", ProductName = "Laptop", Image = "", Status = ""}
        };

        /// <summary>
        /// This enpoint is called to load initial page for product inspection app.
        /// </summary>
        [Route("index")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// This enpoint is called to load scan barcode page inside task module.
        /// </summary>
        [Route("scanProduct")]
        [HttpGet]
        public IActionResult ScanProduct()
        {
            return View();
        }

        /// <summary>
        /// This enpoint is called to load page for scanning details of a specific product.
        /// </summary>
        [Route("viewDetails")]
        [HttpGet]
        public IActionResult ViewDetails()
        {
            return View();
        }

        /// <summary>
        /// This enpoint is called to get details of a particular product based on product id.
        /// </summary>
        /// <param name="productId">Id of the product for which the user wants to fetch details.</param>
        [HttpGet]
        [Route("productDetails")]
        public IActionResult ProductDetails(string productId)
        {
            var productItem = ProductList.FirstOrDefault(p => p.ProductId == productId);

            if (productItem == null)
            {
                ViewBag.Message = new ProductDetails() { ProductId = "", ProductName = "", Image = "", Status = "" };
            }
            else
            {
                ViewBag.Message = productItem;
            }

            return View();
        }

        /// <summary>
        /// This enpoint is called to save the updated changes for particular product based on id.
        /// </summary>
        [HttpPost]
        [Route("Save")]
        public string Save()
        {
            var productId = Request.Form["productId"];
            var image = Request.Form["image"];
            var status = Request.Form["status"];

            // Check if product id recieved is null
            if (string.IsNullOrEmpty(productId))
            {
                return "Empty Product id";
            }
            else
            {
                var productItem = ProductList.FirstOrDefault(p => p.ProductId == productId);
                productItem.Image = image;
                productItem.Status = status;
                return "Product details updated successfully";
            }
        }
    }
}