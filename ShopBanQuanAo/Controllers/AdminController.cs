using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // Dashboard
    public IActionResult Dashboard()
    {
        // 👥 tổng user (trừ admin)
        var totalUsers = _context.Users.Count(u => u.Role != "Admin");

        // 📦 đơn completed
        var completedOrders = _context.Orders.Count(o => o.Status == "Completed");

        // ⏳ đơn chưa completed (trừ cancelled)
        var pendingOrders = _context.Orders
            .Count(o => o.Status != "Completed" && o.Status != "Cancelled");

        // 💰 doanh thu
        var revenue = _context.Orders
            .Where(o => o.Status == "Completed")
            .Sum(o => (decimal?)o.TotalAmount) ?? 0;

        // 📈 chart theo tháng
        //var chartData = _context.Orders
        //    .Where(o => o.Status == "Completed")
        //    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
        //    .Select(g => new
        //    {
        //        Month = g.Key.Month,
        //        Total = g.Sum(x => x.TotalAmount)
        //    })
        //    .OrderBy(x => x.Month)
        //    .ToList();
        // 👉 lấy data theo tháng
        var rawData = _context.Orders
            .Where(o => o.Status == "Completed")
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                Total = g.Sum(x => x.TotalAmount)
            })
            .ToList();

        // 👉 tạo đủ 12 tháng
        var chartData = new decimal[12];

        for (int i = 1; i <= 12; i++)
        {
            var monthData = rawData.FirstOrDefault(x => x.Month == i);
            chartData[i - 1] = monthData != null ? monthData.Total : 0;
        }

        ViewBag.TotalUsers = totalUsers;
        ViewBag.CompletedOrders = completedOrders;
        ViewBag.PendingOrders = pendingOrders;
        ViewBag.Revenue = revenue;

        //ViewBag.ChartLabels = chartData.Select(x => "Tháng " + x.Month).ToList();
        //ViewBag.ChartData = chartData.Select(x => x.Total).ToList();
        ViewBag.ChartLabels = Enumerable.Range(1, 12)
        .Select(m => "Tháng " + m)
        .ToList();

        ViewBag.ChartData = chartData;

        return View();
    }
    // User Management
    public IActionResult Users()
    {
        var users = _context.Users
            .Where(u => u.Role != "Admin")
            .OrderByDescending(u => u.CreatedAt)
            .ToList();

        return View(users);
    }

    // Toggle user active status
    public IActionResult ToggleUser(int id)
    {
        var user = _context.Users.Find(id);

        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;

        _context.SaveChanges();

        return RedirectToAction("Users");
    }

    // View user orders
    public IActionResult UserOrders(int id)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Id == id);

        if (user == null) return NotFound();

        var orders = _context.Orders
        .Where(o => o.UserId == id && o.Status == "Completed")
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
        .OrderByDescending(o => o.OrderDate)
        .ToList();

        ViewBag.User = user;

        return View(orders);
    }

    // Order Management
    public IActionResult Orders()
    {
        var orders = _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View(orders);
    }

    // Update order status
    public IActionResult UpdateStatus(int id, string status)
    {
        var order = _context.Orders.Find(id);
        if (order == null) return NotFound();

        order.Status = status;

        _context.SaveChanges();

        return RedirectToAction("Orders");
    }

    // Category Management
    public IActionResult Categories()
    {
        var categories = _context.Categories.ToList();
        return View(categories);
    }

    // Create category
    [HttpGet]
    public IActionResult CreateCategory()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateCategory(Category model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Categories.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Categories");
    }


}