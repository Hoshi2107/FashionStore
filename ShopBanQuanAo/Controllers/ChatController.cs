using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

public class ChatController : Controller
{
    //private readonly string apiKey = "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"; // ⚠️ đổi lại key mới nha
    private readonly AppDbContext _context;

    public ChatController(AppDbContext context)
    {
        _context = context;
    }

    private async Task<IActionResult> AskAI(string message)
    {
        try
        {
            using var client = new HttpClient();

            var url = "https://openrouter.ai/api/v1/chat/completions";

            client.DefaultRequestHeaders.Add("Authorization", "Bearer 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");

            var body = new
            {
                model = "meta-llama/llama-3-8b-instruct",
                messages = new[]
                {
                new {
                    role = "system",
                    //content = "Bạn là chatbot bán quần áo, luôn trả lời tiếng Việt."
                    content = "Bạn là nhân viên bán hàng của shop FashionStore. QUY TẮC: - Luôn trả lời bằng tiếng Việt, đúng chính tả - Chỉ nói về sản phẩm của shop - Không được nhắc tới shop khác - Trả lời như nhân viên tư vấn chuyên nghiệp - Nếu câu hỏi chung chung → gợi ý sản phẩm cụ thể - Văn phong thân thiện, ngắn gọn Ví dụ: User: hôm nay nóng nên mặc gì Bot: Hôm nay nóng thì bạn nên chọn áo thun cotton hoặc áo sơ mi mỏng bên shop mình, vừa thoáng vừa dễ phối 😄.Nếu không chắc → nói:\r\n'Shop chưa rõ nhu cầu của bạn, bạn mô tả cụ thể hơn nha 😄' "
                },
                new {
                    role = "user",
                    content = message
                }
            }
            };

            var res = await client.PostAsJsonAsync(url, body);
            var result = await res.Content.ReadAsStringAsync();

            Console.WriteLine("AI RAW: " + result); // 👈 QUAN TRỌNG

            using var doc = JsonDocument.Parse(result);

            // ❌ nếu có lỗi
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var msg = error.GetProperty("message").GetString();
                return Json(new { type = "text", reply = "AI lỗi: " + msg });
            }

            // ❌ nếu không có choices
            if (!doc.RootElement.TryGetProperty("choices", out var choices))
            {
                return Json(new { type = "text", reply = "Bot chưa trả lời 😢" });
            }

            var reply = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Json(new { type = "text", reply });
        }
        catch (Exception ex)
        {
            return Json(new { type = "text", reply = "Server lỗi: " + ex.Message });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }


    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] ChatRequest req)
    {
        var msg = req.Message.ToLower();

        // 🔥 xử lý sản phẩm trước (ưu tiên DB)
        if (msg.Contains("áo") || msg.Contains("quần"))
        {
            var query = _context.Products.AsQueryable();

            if (msg.Contains("áo"))
                query = query.Where(x => x.Name.ToLower().Contains("áo"));

            if (msg.Contains("đen"))
                query = query.Where(x => x.Name.ToLower().Contains("đen"));

            // lọc giá (đơn giản)
            if (msg.Contains("300"))
                query = query.Where(x => x.Price <= 300000);

            var products = query.Take(4).Select(x => new
            {
                x.Id,
                x.Name,
                x.Price,
                x.ImageUrl
            }).ToList();

            return Json(new
            {
                type = "product",
                data = products,
                reply = "Shop tìm được mấy sản phẩm này nè 😍"
            });
        }

        // 🔥 fallback AI (OpenRouter)
        return await AskAI(req.Message);
    }
}