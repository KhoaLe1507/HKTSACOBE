using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class ChatRequest
    {
        public string UserMessage { get; set; } = string.Empty;
        public ChatContext Context { get; set; } = new();
    }

    public class ChatContext
    {
        public string Type { get; set; } = string.Empty; // "problem" hoặc "module"
        public int Id { get; set; }
    }

}
