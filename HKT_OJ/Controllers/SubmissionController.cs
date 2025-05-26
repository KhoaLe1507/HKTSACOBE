using Microsoft.AspNetCore.Mvc;
using HKT_OJ.Models;
using HKT_OJ.Services;
using Microsoft.AspNetCore.Authorization;
using HKT_OJ.ViewModels;

namespace HKT_OJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : ControllerBase
    {
        private readonly Judge0Service _judge0Service;

        public SubmissionController()
        {
            _judge0Service = new Judge0Service();
        }

        //[Authorize(Roles = "0")]
        [HttpPost]
        public async Task<IActionResult> SubmitCode([FromBody] SubmissionRequest request)
        {
            var result = await _judge0Service.SubmitCodeAsync(request);
            return Ok(result);
        }
        //Co ham ExecuteAndGetOutputAsync() chi dung cho viec chay code va lay KQ , K SO SANH KQ -> Co the mo rong
        [HttpPost("RunAndGetOutput")]
        public async Task<IActionResult> RunAndGetOutput([FromBody] SubmissionRequest request)
        {
            var output = await _judge0Service.ExecuteAndGetOutputAsync(
                request.SourceCode,
                request.LanguageId,
                request.Input
            );

            return Ok(new { stdout = output });
        }
        [HttpPost("Generateinput")]
        public async Task<IActionResult> Generateinput([FromBody] SubmissionRequest request)
        {
            string generatedInput = await _judge0Service.RunCodeAndGetOutputWithoutInput(
                request.SourceCode,
                request.LanguageId.ToString()
            );

            return Ok(generatedInput);
        }
    }
}
