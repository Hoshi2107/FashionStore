using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

public class CartController : Controller
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    // ========================
    // ADD TO CART
    // ========================
    public IActionResult AddToCart(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);

        if (product == null) return NotFound();

        var cart = HttpContext.Session.GetString("Cart");
        List<CartItem> cartItems;

        if (cart != null)
        {
            cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cart);
        }
        else
        {
            cartItems = new List<CartItem>();
        }

        var item = cartItems.FirstOrDefault(x => x.ProductId == id);

        if (item != null)
        {
            item.Quantity++;
        }
        else
        {
            cartItems.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1,
                ImageUrl = product.ImageUrl
            });
        }

        HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cartItems));

        return RedirectToAction("Index", "Home");
    }

    // ========================
    // VIEW CART
    // ========================
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetString("Cart");

        if (cart == null)
        {
            return View(new List<CartItem>());
        }

        var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cart);

        return View(cartItems);
    }

    // ========================
    // REMOVE
    // ========================
    public IActionResult Remove(int id)
    {
        var cart = HttpContext.Session.GetString("Cart");

        if (cart != null)
        {
            var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cart);

            var item = cartItems.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                cartItems.Remove(item);
            }

            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cartItems));
        }

        return RedirectToAction("Index");
    }

    // ========================
    // INCREASE QUANTITY
    // ========================
    public IActionResult Increase(int id)
    {
        var cart = HttpContext.Session.GetString("Cart");
        var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItem>>(cart);

        var item = list.FirstOrDefault(x => x.ProductId == id);
        if (item != null)
        {
            item.Quantity++;
        }

        HttpContext.Session.SetString("Cart",
            Newtonsoft.Json.JsonConvert.SerializeObject(list));

        return RedirectToAction("Index");
    }

    // ========================
    // DECREASE
    // ========================
    public IActionResult Decrease(int id)
    {
        var cart = HttpContext.Session.GetString("Cart");
        var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItem>>(cart);

        var item = list.FirstOrDefault(x => x.ProductId == id);
        if (item != null)
        {
            item.Quantity--;

            if (item.Quantity <= 0)
            {
                list.Remove(item);
            }
        }

        HttpContext.Session.SetString("Cart",
            Newtonsoft.Json.JsonConvert.SerializeObject(list));

        return RedirectToAction("Index");
    }
    // ========================
    // CHECKOUT GET
    // ========================
    public IActionResult Checkout()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (userIdStr == null)
            return RedirectToAction("Login", "Account");

        int userId = int.Parse(userIdStr);

        var user = _context.Users.Find(userId);

        var model = new CheckoutVM
        {
            FullName = user.FullName,
            Phone = user.Phone,
            Address = user.Address
        };

        ViewBag.Cart = GetCart(); // t sẽ viết dưới

        return View(model);
    }

    private List<CartItem> GetCart()
    {
        var cart = HttpContext.Session.GetString("Cart");

        if (cart == null)
            return new List<CartItem>();

        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItem>>(cart);
    }

    [HttpPost]
    public IActionResult Checkout(CheckoutVM model)
    {
        var cart = HttpContext.Session.GetString("Cart");

        if (cart == null)
            return RedirectToAction("Index");

        // Kiểm tra user đã đăng nhập chưa
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (userIdStr == null)
            return RedirectToAction("Login", "Account");

        int userId = int.Parse(userIdStr);

        var cartItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItem>>(cart);

        // Lấy user
        //int userId = int.Parse(HttpContext.Session.GetString("UserId"));

        // Tính tổng tiền
        var total = cartItems.Sum(x => x.Price * x.Quantity);

        // Tạo Order
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "Pending",
            TotalAmount = total,
            ShippingAddress = model.Address,
            Note = model.Note
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        // Tạo OrderDetails
        foreach (var item in cartItems)
        {
            var detail = new OrderDetail
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            };

            _context.OrderDetails.Add(detail);
        }

        _context.SaveChanges();

        // Xóa cart
        HttpContext.Session.Remove("Cart");

        return RedirectToAction("Success", new { id = order.Id });
    }

    // ========================
    // CHECKOUT SUCCESS
    // ========================
    public IActionResult Success(int id)
    {
        var order = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefault(o => o.Id == id);

        if (order == null) return NotFound();

        // Lấy category của sản phẩm đầu tiên
        var categoryId = order.OrderDetails.First().Product.CategoryId;

        // Gợi ý 4 sản phẩm cùng category
        var productIds = order.OrderDetails.Select(x => x.ProductId);

        var recommend = _context.Products
            .Where(p => p.CategoryId == categoryId
                     && !productIds.Contains(p.Id)
                     && p.IsActive)
            .Take(4)
            .ToList();

        ViewBag.Recommend = recommend;

        return View(order);
    }

}