using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AdminProductController : Controller
{
    private readonly AppDbContext _context;

    public AdminProductController(AppDbContext context)
    {
        _context = context;
    }

    // ========================
    // LIST
    // ========================
    //public IActionResult Index()
    //{
    //    var list = _context.Products
    //        .Include(p => p.Category)
    //        .OrderByDescending(p => p.CreatedAt)
    //        .ToList();

    //    return View(list);
    //}

    public IActionResult Index(int page = 1)
    {
        int pageSize = 6;

        var totalItems = _context.Products.Count();

        var products = _context.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return View(products);
    }

    // ========================
    // CREATE
    // ========================
    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.ToList();
        return View();
    }

    //[HttpPost]
    //public IActionResult Create(Product model)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        ViewBag.Categories = _context.Categories.ToList();
    //        return View(model);
    //    }

    //    model.CreatedAt = DateTime.Now;
    //    model.IsActive = true;

    //    _context.Products.Add(model);
    //    _context.SaveChanges();

    //    return RedirectToAction("Index");
    //}

    [HttpPost]
    public IActionResult Create(Product model, IFormFile ImageFile)
    {
        // ✅ check ảnh trước
        if (ImageFile == null)
        {
            ModelState.AddModelError("", "Vui lòng chọn ảnh");
        }

        // 👉 nếu có ảnh thì validate
        if (ImageFile != null && ImageFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var ext = Path.GetExtension(ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("", "Chỉ cho phép ảnh jpg, png");
            }
        }

        // 🔥 CHECK SAU CÙNG
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        // ================= upload =================
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            ImageFile.CopyTo(stream);
        }

        model.ImageUrl = fileName;

        model.CreatedAt = DateTime.Now;
        model.IsActive = true;

        _context.Products.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }



    // ========================
    // EDIT
    // ========================
    public IActionResult Edit(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null) return NotFound();

        ViewBag.Categories = _context.Categories.ToList();
        return View(product);
    }

    //[HttpPost]
    //public IActionResult Edit(Product model)
    //{
    //    var product = _context.Products.Find(model.Id);
    //    if (product == null) return NotFound();

    //    if (!ModelState.IsValid)
    //    {
    //        ViewBag.Categories = _context.Categories.ToList();
    //        return View(model);
    //    }

    //    // update đúng field
    //    product.Name = model.Name;
    //    product.Description = model.Description;
    //    product.Price = model.Price;
    //    product.Stock = model.Stock;
    //    product.CategoryId = model.CategoryId;
    //    product.ImageUrl = model.ImageUrl;
    //    product.Size = model.Size;
    //    product.Color = model.Color;

    //    _context.SaveChanges();

    //    return RedirectToAction("Index");
    //}

    [HttpPost]
    public IActionResult Edit(Product model, IFormFile? ImageFile)
    {
        var product = _context.Products.Find(model.Id);
        if (product == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        product.Name = model.Name;
        product.Description = model.Description;
        product.Price = model.Price;
        product.Stock = model.Stock;
        product.CategoryId = model.CategoryId;
        product.Size = model.Size;
        product.Color = model.Color;

        // 👇 Chỉ đổi ảnh nếu user upload ảnh mới
        if (ImageFile != null && ImageFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var ext = Path.GetExtension(ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("", "Chỉ cho phép ảnh jpg, png");
                ViewBag.Categories = _context.Categories.ToList();
                return View(model);
            }

            string fileName = Guid.NewGuid().ToString() + ext;
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                ImageFile.CopyTo(stream);
            }

            product.ImageUrl = fileName;
        }

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    // ========================
    // TOGGLE (Còn/Hết hàng)
    // ========================
    public IActionResult Toggle(int id)
    {
        var product = _context.Products.Find(id);

        if (product != null)
        {
            product.IsActive = !product.IsActive;
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}