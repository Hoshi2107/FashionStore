using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    // Register
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(User model, string Password, string ConfirmPassword)
    {
        if (Password != ConfirmPassword)
        {
            ViewBag.Error = "Mật khẩu không khớp";
            return View(model);
        }

        var exist = _context.Users.Any(u => u.Email == model.Email);
        if (exist)
        {
            ViewBag.Error = "Email đã tồn tại";
            return View(model);
        }

        model.PasswordHash = Password; // (plain cho ASM)
        model.Role = "Customer";

        _context.Users.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }


    // Login

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginVM model)
    {

        if (!ModelState.IsValid)
            return View(model);

        var user = _context.Users
            .FirstOrDefault(x => x.Email == model.Email && x.PasswordHash == model.Password);



        if (user == null)
        {
            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
            return View(model);
        }

        if (!user.IsActive)
        {
            ViewBag.Error = "Tài khoản đã bị khóa!";
            return View();
        }

        // TODO: set session
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.FullName);
        HttpContext.Session.SetString("Role", user.Role);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult Profile()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (userIdStr == null)
            return RedirectToAction("Login");

        int userId = int.Parse(userIdStr);

        var user = _context.Users.Find(userId);

        return View(user);
    }

    [HttpPost]
    public IActionResult Profile(User model)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (userIdStr == null)
            return RedirectToAction("Login");

        int userId = int.Parse(userIdStr);

        var user = _context.Users.Find(userId);

        if (user == null) return NotFound();

        // update field
        user.FullName = model.FullName;
        user.Phone = model.Phone;
        user.Address = model.Address;

        _context.SaveChanges();

        TempData["Success"] = "Cập nhật thành công!";

        return RedirectToAction("Profile");
    }

    // Change password
    public IActionResult ChangePassword()
    {
        if (HttpContext.Session.GetString("UserId") == null)
            return RedirectToAction("Login");

        return View();
    }

    [HttpPost]
    public IActionResult ChangePassword(ChangePasswordVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (userIdStr == null)
            return RedirectToAction("Login");

        int userId = int.Parse(userIdStr);

        var user = _context.Users.Find(userId);
        if (user == null) return NotFound();

        // ❗ vì m đang dùng plain password
        if (user.PasswordHash != model.OldPassword)
        {
            ModelState.AddModelError("", "Mật khẩu cũ không đúng");
            return View(model);
        }

        // update password
        user.PasswordHash = model.NewPassword;

        _context.SaveChanges();

        TempData["Success"] = "Đổi mật khẩu thành công!";

        return RedirectToAction("Profile");
    }

}