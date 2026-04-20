using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShopBanQuanAo.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // ========================
        // PENDING
        // ========================
        //public IActionResult Pending()
        //{
        //    int userId = int.Parse(HttpContext.Session.GetString("UserId"));

        //    var orders = _context.Orders
        //        .Where(o => o.UserId == userId &&
        //               (o.Status == "Pending" || o.Status == "Confirmed" || o.Status == "Shipping"))
        //        .OrderByDescending(o => o.OrderDate)
        //        .Include(o => o.OrderDetails)
        //        .ThenInclude(od => od.Product)
        //        .ToList();

        //    return View(orders);
        //}
        // ========================
        // PENDING
        // ========================
        public IActionResult Pending()
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
                return RedirectToAction("Login", "Account");

            var orders = _context.Orders
                .Where(o => o.UserId == userId &&
                       (o.Status == "Pending" || o.Status == "Confirmed" || o.Status == "Shipping"))
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToList();

            return View(orders);
        }

        // ========================
        // Order Detail
        // ========================
        public IActionResult Detail(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // ========================
        // HISTORY
        // ========================
        public IActionResult History()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (userIdStr == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            var orders = _context.Orders
                .Where(o => o.UserId == userId &&
                       (o.Status == "Completed" || o.Status == "Cancelled"))
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // ========================
        // Cancel Order
        // ========================
        public IActionResult Cancel(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (userIdStr == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            var order = _context.Orders
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            // chỉ cho hủy khi đang Pending
            if (order.Status != "Pending")
            {
                TempData["Error"] = "Không thể hủy đơn này!";
                return RedirectToAction("Pending");
            }

            order.Status = "Cancelled";

            _context.SaveChanges();

            TempData["Success"] = "Đã hủy đơn thành công!";

            return RedirectToAction("Pending");
        }
    }
}