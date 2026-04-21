using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopBanQuanAo.Models;
using System.Diagnostics;

namespace ShopBanQuanAo.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // ========================
        // HOME PAGE
        // ========================
        //public IActionResult Index()
        //{
        //    var products = _context.Products
        //        //.Where(p => p.IsActive)
        //        .OrderByDescending(p => p.CreatedAt)
        //        .Take(8)
        //        .ToList();

        //    var cart = HttpContext.Session.GetString("Cart");
        //    int count = 0;

        //    if (cart != null)
        //    {
        //        var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItem>>(cart);
        //        count = list.Sum(x => x.Quantity);
        //    }

        //    ViewBag.CartCount = count;

        //    return View(products);
        //}
        public IActionResult Index(string keyword, int? categoryId, int page = 1)
        {
            int pageSize = 12;

            var query = _context.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            // search
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword));
            }

            // filter category
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }


            int totalItems = query.Count();

            var products = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // cart count
            var cart = HttpContext.Session.GetString("Cart");
            int cartCount = 0;
            if (cart != null)
            {
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItem>>(cart);
                cartCount = list?.Sum(x => x.Quantity) ?? 0;
            }


            ViewBag.CartCount = cartCount;
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        // ========================
        // PRODUCT DETAIL
        // ========================
        public IActionResult Detail(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            var related = _context.Products
                .Where(p => p.CategoryId == product.CategoryId
                         && p.Id != id
                         && p.IsActive)
                .Take(4)
                .ToList();

            ViewBag.Related = related;

            return View(product);
        }

        public IActionResult Search(string keyword)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                products = products.Where(p => p.Name.Contains(keyword));
            }

            return View(products.ToList());
        }

    }
}
