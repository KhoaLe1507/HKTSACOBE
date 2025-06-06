using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class FileUpdateRequest
    {
        public string FilePath { get; set; }
        public string NewContent { get; set; }
    }

}
