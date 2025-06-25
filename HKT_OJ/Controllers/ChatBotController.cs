using System.Text;
using HKT_OJ.Data;
using HKT_OJ.Models;
using HKT_OJ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HKT_OJ.ViewModels;
using System.Text.RegularExpressions;


namespace HKT_OJ.Controllers
{
    [Route("api/chatbot")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly GeminiService _gemini;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChatBotController(GeminiService gemini, AppDbContext context, IWebHostEnvironment env)
        {
            _gemini = gemini;
            _context = context;
            _env = env;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            string userMessage = request.UserMessage?.Trim() ?? "";
            if (string.IsNullOrEmpty(userMessage))
                return BadRequest("Message cannot be empty.");

            string contextText = string.Empty;

            if (request.Context.Type == "problem")
            {
                contextText = await GetProblemContext(request.Context.Id);
            }
            else if (request.Context.Type == "module")
            {
                contextText = await GetModuleContentContext(request.Context.Id);
            }

            // Nếu có context hợp lệ → gửi context + câu hỏi
            // Ngược lại → chỉ gửi userMessage
            var systemContext =
                "📌 Giới thiệu hệ thống:\n" +
                "Tôi là trợ lý AI-AlgoExpert thuộc hệ thống **HKT_OJ** — một nền tảng học lập trình thuật toán hiện đại được phát triển nội bộ.\n\n" +
                "HKT_OJ kết hợp 2 chức năng chính:\n" +
                "- 🎓 **Học lý thuyết có lộ trình (roadmap)** theo module, tương tự cách tổ chức của USACO.\n" +
                "- 🧪 **Giải bài tập có hệ thống chấm tự động theo test case**, giống như các nền tảng LeetCode, AtCoder, hay Codeforces.\n\n" +
                "HKT_OJ hỗ trợ theo dõi tiến độ học, xem lại lời giải, và đặc biệt là tích hợp AI để giải thích bài toán, phân tích lời giải, hoặc hỗ trợ lý thuyết khi cần.\n\n" +
                "Bạn có thể hỏi bất kỳ điều gì về bài học hoặc bài tập hiện tại. Tôi sẽ cố gắng trả lời theo dữ liệu và khả năng của mình.";

            string promptToSend = !string.IsNullOrWhiteSpace(contextText)
                ? $"{systemContext}\n\n{contextText}\n\n💬 Câu hỏi của học viên:\n{userMessage}"
                : $"{systemContext}\n\n💬 Câu hỏi của học viên:\n{userMessage}";

            string geminiReply = await _gemini.AskGeminiAsync(promptToSend);
            return Ok(new ChatResponse { Reply = geminiReply });

        }

        private async Task<string> GetProblemContext(int problemId)
        {
            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null) return string.Empty;

            var constraints = await _context.ProblemConstraint
                .Where(c => c.ProblemId == problemId).ToListAsync();

            var samples = await _context.Testcase
                .Where(t => t.ProblemId == problemId && t.IsSample).ToListAsync();

            var solution = await _context.Solution
                .FirstOrDefaultAsync(s => s.ProblemId == problemId);

            var sb = new StringBuilder();
            sb.AppendLine($"📌 Tên bài toán: {problem.Name}");
            sb.AppendLine($"⏱️ Giới hạn thời gian: {problem.TimeLimit} ms");
            sb.AppendLine($"📖 Đề bài:\n{problem.ProblemStatement}");
            sb.AppendLine($"\n📥 Input Format:\n{problem.FormatInput}");
            sb.AppendLine($"\n📤 Output Format:\n{problem.FormatOutput}");

            if (constraints.Any())
            {
                sb.AppendLine("\n🔒 Ràng buộc:");
                foreach (var c in constraints)
                    sb.AppendLine($"- {c.Variable}: {c.MinValue} ≤ {c.Variable} ≤ {c.MaxValue}");
            }

            if (samples.Any())
            {
                sb.AppendLine("\n🧪 Ví dụ mẫu:");
                foreach (var s in samples)
                {
                    string inputPath = Path.Combine(_env.WebRootPath, s.InputPath.TrimStart('/'));
                    string outputPath = Path.Combine(_env.WebRootPath, s.OutputPath.TrimStart('/'));

                    string input = System.IO.File.Exists(inputPath) ? await System.IO.File.ReadAllTextAsync(inputPath) : "[Không tìm thấy file input]";
                    string output = System.IO.File.Exists(outputPath) ? await System.IO.File.ReadAllTextAsync(outputPath) : "[Không tìm thấy file output]";

                    sb.AppendLine("📘 Input:");
                    sb.AppendLine(input);
                    sb.AppendLine("📗 Output:");
                    sb.AppendLine(output);

                    if (!string.IsNullOrEmpty(s.Explanation))
                    {
                        sb.AppendLine("📙 Giải thích:");
                        sb.AppendLine(s.Explanation);
                    }

                    sb.AppendLine("---");
                }
            }

            if (solution != null)
            {
                sb.AppendLine("\n🧠 Lời giải chuẩn:");
                sb.AppendLine(solution.Explanation);
                sb.AppendLine("\n💻 Code chuẩn:");
                sb.AppendLine("```" + solution.Language.ToLower());
                sb.AppendLine(solution.Source);
                sb.AppendLine("```");
            }

            return sb.ToString();
        }

        private async Task<string> GetModuleContentContext(int moduleContentId)
        {
            var moduleContent = await _context.ModuleContent.FindAsync(moduleContentId);
            if (moduleContent == null) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"📚 Tiêu đề bài học: {moduleContent.Title}");
            sb.AppendLine($"📄 Mô tả: {moduleContent.Description}");

            // Đọc file HTML
            string htmlPath = Path.Combine(_env.WebRootPath, moduleContent.HtmlContentPath.TrimStart('/'));
            if (System.IO.File.Exists(htmlPath))
            {
                string htmlContent = await System.IO.File.ReadAllTextAsync(htmlPath);
                sb.AppendLine("\n📝 Nội dung chi tiết:");
                sb.AppendLine(htmlContent);
            }
            else
            {
                sb.AppendLine("\n❗Không tìm thấy file nội dung bài học.");
            }

            return sb.ToString();
        }

        [Authorize(Roles = "1,2")]
        [HttpPost("generate-input-code")]
        public async Task<IActionResult> GenerateInputCode([FromBody] GenerateInputCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.InputFormat) ||
                string.IsNullOrWhiteSpace(request.Language) ||
                request.Constraints == null || request.Constraints.Count == 0)
            {
                return BadRequest("⚠️ Missing Input Format, Language, or Constraints.");
            }

            string prompt = $@"
                📌 HKT_OJ là nền tảng học lập trình thuật toán nội bộ.
                - Học lý thuyết theo roadmap giống USACO
                - Giải bài chấm tự động như LeetCode, AtCoder
                - Có AI trợ giảng phân tích đề bài và giải thích lời giải

                Bạn là trợ lý AI giúp giáo viên sinh code để tạo input ngẫu nhiên.

                Thông tin đề bài:
                - Format input:
                {request.InputFormat}

                - Constraints:
                {string.Join("\n", request.Constraints.Select(c => $"{c.Variable}: {c.MinValue} <= {c.Variable} <= {c.MaxValue}"))}

                - Mô tả từ giáo viên:
                {request.Description ?? "(Không có mô tả riêng)"}

                - Sample Input:
                {request.SampleInput ?? "(Không có sample input)"}

                - Ngôn ngữ mong muốn: {request.Language}

                🎯 Yêu cầu:
                - Viết code sinh input random đúng định dạng và giới hạn trên, không cần xử lý nhập xuất file , chỉ in ra như bình thường và đúng format 
                - ❗ **Chỉ gửi code, không giải thích**
                - ❗ **Không bọc trong ``` hay markdown, chỉ gửi code thô dạng Text**
                ";


            try
            {
                string reply = await _gemini.AskGeminiAsync(prompt, "gemini-2.5-pro-preview-06-05");
                return Ok(new { code = reply });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Gemini error: {ex.Message}");
            }
        }

        [HttpPost("analyze-blog")]
        public async Task<IActionResult> AnalyzeBlog([FromBody] BlogAnalysisRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest("Content is required.");

            string prompt = $"Please analyze the following blog post:\n\n" +
                            $"{req.Content}\n\n" +
                            (string.IsNullOrEmpty(req.Criteria) ? "" : $"Approval Criteria: {req.Criteria}\n\n") +
                            "Tasks:\n" +
                            "1. Summarize the content.\n" +
                            "2. Detect inappropriate or spammy content.\n" +
                            "3. Analyze sentiment.\n" +
                            "4. Decide: Approve or Reject. Provide Reason.\n\n" +
                            "Output format:\n" +
                            "Decision: [Approved or Rejected]\n" +
                            "Reason: [Explanation why]";

            string rawResponse = await _gemini.AskGeminiAsync(prompt, "gemini-2.5-pro-preview-06-05");

            Console.WriteLine("====== Gemini Raw Response ======");
            Console.WriteLine(rawResponse);

            var parsed = ParseGeminiOutput(rawResponse);

            return Ok(new
            {
                decision = parsed.Decision,
                reason = parsed.Reason
            });
        }

        [HttpPost("analyze-blog-full")]
        public async Task<IActionResult> AnalyzeBlogFull([FromBody] BlogAnalysisRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest("Content is required.");

            string prompt = $"Please analyze the following blog post (Phản hồi bằng Tiếng Việt ) :\n\n" +
                            $"{req.Content}\n\n" +
                            (string.IsNullOrEmpty(req.Criteria) ? "" : $"Approval Criteria: {req.Criteria}\n\n") +
                            "Tasks:\n" +
                            "1. Summarize the content.\n" +
                            "2. Detect inappropriate, sensitive, spam, or offensive content.\n" +
                            "3. Analyze sentiment (Positive, Neutral, Negative).\n" +
                            "4. Decision: Approve or Reject. Provide Reason.\n\n" +
                            "Output format:\n" +
                            "Tóm tắt: ...\n" +
                            "Vi phạm: ...\n" +
                            "Cảm xúc: ...\n" +
                            "Đề xuất: ...\n" +
                            "Lý do: ...";

            string raw = await _gemini.AskGeminiAsync(prompt, "gemini-2.5-pro-preview-06-05");

            Console.WriteLine("====== FULL ANALYSIS RESPONSE ======");
            Console.WriteLine(raw);

            string Clean(string input)
            {
                return input.Replace("**", "").Trim();
            }

            var result = new
            {
                Summary = Clean(Regex.Match(raw, @"Tóm tắt:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value),
                Violations = Clean(Regex.Match(raw, @"Vi phạm:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value),
                Sentiment = Clean(Regex.Match(raw, @"Cảm xúc:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value),
                Decision = Clean(Regex.Match(raw, @"Đề xuất:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value),
                Reason = Clean(Regex.Match(raw, @"Lý do:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value)
            };

            return Ok(result);
        }


        private AIAnalysisResult ParseGeminiOutput(string response)
        {
            var result = new AIAnalysisResult();

            string lower = response.ToLower();

            if (lower.Contains("approved"))
                result.Decision = "Approved";
            else if (lower.Contains("rejected"))
                result.Decision = "Rejected";
            else
                result.Decision = "Rejected"; // fallback

            // Cắt Reason thủ công nếu có
            var reasonMatch = Regex.Match(response, @"[Rr]eason[:：]\s*\**\[?(.*?)]?$", RegexOptions.Singleline);
            result.Reason = reasonMatch.Success ? reasonMatch.Groups[1].Value.Trim() : "No reason provided.";

            return result;
        }






    }



}
