using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HKT_OJ.Data;
using HKT_OJ.Models;
using HKT_OJ.Services;
using HKT_OJ.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HKT_OJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProblemController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Judge0Service _judge0Service;

        public ProblemController(AppDbContext context , Judge0Service judge0Service)
        {
            _context = context;
            _judge0Service = judge0Service;
        }

        [HttpGet("GetAllProblems")]
        public IActionResult GetAllProblems(
            string? searchName,
            string? difficulty,
            string? status,
            string? section,
            string? frequent,
            int userId,
            int? moduleContentId) // 🔥 thêm filter mới
        {
            var query = from problem in _context.Problem
                        join moduleContent in _context.ModuleContent
                            on problem.ModuleContentId equals moduleContent.Id
                        join module in _context.Modules
                            on moduleContent.ModuleId equals module.Id
                        join sec in _context.Sections
                            on module.SectionId equals sec.Id
                        join prog in _context.ProblemProgress
                            on new { ProblemId = problem.ProblemId, UserId = userId } equals new { prog.ProblemId, prog.UserId }
                                into progressJoin
                        from progress in progressJoin.DefaultIfEmpty()
                        select new ProblemCardViewModel
                        {
                            ProblemId = problem.ProblemId,
                            Name = problem.Name,
                            ModuleContentTitle = moduleContent.Title,
                            Difficulty = problem.Difficulty,
                            AuthorId=problem.AuthorId
                            //Frequent = problem.Frequent,
                        };

            // ✅ Các filter hiện có
            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(p => p.Name.Contains(searchName));

            if (!string.IsNullOrEmpty(difficulty))
                query = query.Where(p => p.Difficulty == difficulty);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(section))
                query = query.Where(p => p.Section == section);

            if (!string.IsNullOrEmpty(frequent))
                query = query.Where(p => p.Frequent == frequent);

            // ✅ Thêm filter theo moduleContentId
            if (moduleContentId.HasValue)
                query = query.Where(p => _context.ModuleContent
                                        .Where(mc => mc.Id == moduleContentId.Value)
                                        .Select(mc => mc.Title)
                                        .Contains(p.ModuleContentTitle));

            var result = query.ToList();
            return Ok(result);
        }


        // API phụ để lấy danh sách tất cả ModuleContent
        [HttpGet("modulecontents")]
        public IActionResult GetModuleContents()
        {
            var moduleContents = _context.ModuleContent
                .Select(mc => new {
                    Id = mc.Id,
                    Title = mc.Title
                })
                .ToList();

            return Ok(moduleContents);
        }



        [Authorize(Roles = "1,2")]
        [HttpPost("add")]
        public async Task<IActionResult> AddProblem([FromBody] ProblemCreateRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User not logged in.");

            int authorId = int.Parse(userIdClaim.Value);

            var author = await _context.User.FindAsync(authorId);
            if (author != null)
            {
                author.ProblemCreated += 1;
                _context.User.Update(author);
                await _context.SaveChangesAsync();
            }


            var problem = new Problem
            {
                Name = request.Name,
                Frequent = request.Frequent,
                ModuleContentId = request.ModuleContentId,
                Difficulty = request.Difficulty,
                TimeLimit = request.TimeLimit,
                MemoryLimit = request.MemoryLimit,
                ProblemStatement = request.ProblemStatement,
                FormatInput = request.FormatInput,
                FormatOutput = request.FormatOutput,
                SolutionId = 0, // cập nhật sau
                AuthorId = authorId
            };

            _context.Problem.Add(problem);
            await _context.SaveChangesAsync();

            // Lưu Solution
            var solution = new Solution
            {
                ProblemId = problem.ProblemId,
                Source = request.Solution.Source,
                Language = request.Solution.Language,
                Explanation = request.Solution.Explanation
            };
            _context.Solution.Add(solution);
            await _context.SaveChangesAsync();

            problem.SolutionId = solution.SolutionId;
            _context.Problem.Update(problem);
            await _context.SaveChangesAsync();

            // Lưu Constraints
            foreach (var c in request.Constraints)
            {
                _context.ProblemConstraint.Add(new ProblemConstraint
                {
                    ProblemId = problem.ProblemId,
                    Variable = c.Variable,
                    MinValue = c.MinValue,
                    MaxValue = c.MaxValue
                });
            }

            // Lưu Testcase Sample
            foreach (var t in request.SampleTestcases)
            {
                var testcase = new Testcase
                {
                    ProblemId = problem.ProblemId,
                    InputPath = SaveToFile(t.Input, problem.ProblemId, "input"),
                    OutputPath = SaveToFile(t.ExpectedOutput, problem.ProblemId, "output"),
                    IsSample = true,
                    Explanation = t.Explanation
                };
                _context.Testcase.Add(testcase);
            }

            // 🔥 Sinh Testcase Random 
            if (request.NumberOfGeneratedTestcases > 0)
            {
                int generatorLangId = _judge0Service.MapLanguageToId(request.TestGeneratorLanguage);
                int solutionLangId = _judge0Service.MapLanguageToId(request.Solution.Language);

                for (int i = 0; i < request.NumberOfGeneratedTestcases; i++)
                {
                    Console.WriteLine("🔧 [TESTGEN] Generating input with the following source code:");
                    Console.WriteLine(request.TestGeneratorSource);
                    string generatedInput = await _judge0Service.RunCodeAndGetOutputWithoutInput(
                        request.TestGeneratorSource,
                        generatorLangId.ToString()
                    );
                    Console.WriteLine("FIXBUGGGG"+generatedInput);
                    string expectedOutput = await _judge0Service.ExecuteAndGetOutputAsync(
                        request.Solution.Source,
                        solutionLangId.ToString(),
                        generatedInput
                    );

                    var testcase = new Testcase
                    {
                        ProblemId = problem.ProblemId,
                        InputPath = SaveToFile(generatedInput, problem.ProblemId, "input"),
                        OutputPath = SaveToFile(expectedOutput, problem.ProblemId, "output"),
                        IsSample = false,
                        Explanation = null
                    };

                    _context.Testcase.Add(testcase);
                }

            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Problem created successfully", problemId = problem.ProblemId });
        }


        [Authorize(Roles ="0")]
        [HttpPost("submit/{problemId}")]
        public async Task<IActionResult> SubmitProblem(int problemId, [FromBody] SubmitRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User not logged in.");

            int userId = int.Parse(userIdClaim.Value);

            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null) return NotFound("Problem not found.");

            // Tạo submission mới
            var submission = new Submission
            {
                ProblemId = problemId,
                UserId = userId,
                SubmitAtTime = DateTime.UtcNow,
                Language = request.LanguageId,
                Time = 0,
                Memory = 0,
                Result = "Pending",
                UserSourceCode = request.SourceCode
            };
            _context.Submission.Add(submission);
            await _context.SaveChangesAsync();

            var testcases = _context.Testcase
                .Where(t => t.ProblemId == problemId)
                .ToList();

            var submissionResults = new List<SubmissionResult>();
            var finalResult = "Accepted";

            foreach (var tc in testcases)
            {
                string input = System.IO.File.ReadAllText(Path.Combine("wwwroot", tc.InputPath.TrimStart('/')));
                string expectedOutput = System.IO.File.ReadAllText(Path.Combine("wwwroot", tc.OutputPath.TrimStart('/')));

                var judgeRequest = new SubmissionRequest
                {
                    SourceCode = request.SourceCode,
                    LanguageId = request.LanguageId,
                    Input = input,
                    ExpectedOutput = expectedOutput,
                    TimeLimit = problem.TimeLimit ,
                    MemoryLimit = problem.MemoryLimit 

                };

                string judgeResponse = await _judge0Service.SubmitCodeAsync(judgeRequest);
                var resultJson = JsonSerializer.Deserialize<JsonElement>(judgeResponse);


                // ✅ In log JSON gốc trả về từ Judge0 để debug
                Console.WriteLine("──── Judge0 Raw Response ────");
                Console.WriteLine(judgeResponse);
                Console.WriteLine("─────────────────────────────");


                string status = resultJson.GetProperty("status").GetProperty("description").GetString() ?? "Unknown";
                string stdoutBase64 = resultJson.TryGetProperty("stdout", out var stdoutProp)
                    ? stdoutProp.GetString() ?? ""
                    : "";

                string stdout = Encoding.UTF8.GetString(Convert.FromBase64String(stdoutBase64));
                string timeStr = resultJson.GetProperty("time").GetString() ?? "0.0";
                string memoryStr = resultJson.GetProperty("memory").ToString();

                int executedTime = (int)(float.Parse(timeStr)); // ms
                int memory = int.TryParse(memoryStr, out var parsedMem) ? parsedMem : 0; // KB

                string userOutputPath = SaveStdoutToFile(stdout, problemId, submission.SubmissionId, tc.TestcaseId);

                // Ghi từng testcase vào SubmissionResult
                submissionResults.Add(new SubmissionResult
                {
                    SubmissionId = submission.SubmissionId,
                    TestcaseId = tc.TestcaseId,
                    InputPath = tc.InputPath,
                    ExpectedOutputPath = tc.OutputPath,
                    UserOutputPath = userOutputPath,
                    ExecutedTime = executedTime,
                    Memory = memory,
                    Result = status,
                });


            }
            var allResults = submissionResults.Select(r => r.Result).ToList();

            if (allResults.Any(r => r == "Compilation Error"))
                finalResult = "Compilation Error";
            else if (allResults.Any(r => r == "Runtime Error (NZEC)"))
                finalResult = "Runtime Error";
            else if (allResults.Any(r => r == "Time Limit Exceeded"))
                finalResult = "Time Limit Exceeded";
            else if (allResults.Any(r => r == "Memory Limit Exceeded"))
                finalResult = "Memory Limit Exceeded";
            else if (allResults.Any(r => r == "Wrong Answer"))
                finalResult = "Wrong Answer";
            else if (allResults.All(r => r == "Accepted"))
                finalResult = "Accepted";
            else
                finalResult = "Unknown";


            submission.Result = finalResult;
            submission.Time = submissionResults.Max(r => r.ExecutedTime);
            submission.Memory = submissionResults.Max(r => r.Memory);

            _context.SubmissionResult.AddRange(submissionResults);
            _context.Submission.Update(submission);
            await _context.SaveChangesAsync();

            if (finalResult == "Accepted")
            {
                var user = await _context.User.FindAsync(userId);
                if (user != null)
                {
                    user.ProblemSolved += 1;
                    _context.User.Update(user);
                    await _context.SaveChangesAsync();
                }
            }


            return Ok(new
            {
                message = "Submission completed.",
                submissionId = submission.SubmissionId,
                finalResult = finalResult
            });
        }

        [Authorize(Roles = "0")]
        [HttpGet("submissiondetails/{submissionId}")]
        public async Task<IActionResult> GetSubmissionDetails(int submissionId)
        {
            var submission = await _context.Submission.FindAsync(submissionId);
            if (submission == null) return NotFound("Submission not found.");

            var user = await _context.User.FindAsync(submission.UserId);
            var problem = await _context.Problem.FindAsync(submission.ProblemId);

            var submissionResults = await _context.SubmissionResult
                .Where(r => r.SubmissionId == submissionId)
                .ToListAsync();

            var detailList = submissionResults.Select((sr, index) => new
            {
                TestcaseId = sr.TestcaseId,
                Index = index + 1,
                Result = sr.Result,
                ExecutedTime = sr.ExecutedTime,
                Memory = sr.Memory,
                Input = System.IO.File.ReadAllText(Path.Combine("wwwroot", sr.InputPath.TrimStart('/'))),
                ExpectedOutput = System.IO.File.ReadAllText(Path.Combine("wwwroot", sr.ExpectedOutputPath.TrimStart('/'))),
                UserOutput = System.IO.File.ReadAllText(Path.Combine("wwwroot", sr.UserOutputPath.TrimStart('/')))
            }).ToList();

            return Ok(new
            {
                ProblemTitle = problem.Name,
                SubmissionNumber = submission.SubmissionId,
                UserName = user.UserName,
                Language = submission.Language,
                MaxTime = detailList.Max(r => r.ExecutedTime),
                MaxMemory = detailList.Max(r => r.Memory),
                FinalResult = submission.Result,
                SourceCode = submission.UserSourceCode,
                Results = detailList
            });
        }


        [HttpGet("getallsubmission/{problemId}")]
        [Authorize]
        public async Task<IActionResult> GetAllSubmissionsForProblem(int problemId)
        {
            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null)
                return NotFound("Problem not found.");

            var submissions = await _context.Submission
                .Where(s => s.ProblemId == problemId)
                .OrderByDescending(s => s.SubmitAtTime)
                .Select(s => new
                {
                    SubmissionId = s.SubmissionId,
                    UserName = _context.User
                        .Where(u => u.UserId == s.UserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    SubmitAtTime = s.SubmitAtTime,
                    ProblemName = problem.Name,
                    TimeExecuted = s.Time,
                    Memory = s.Memory,
                    Language = s.Language,
                    Result = s.Result
                })
                .ToListAsync();

            return Ok(submissions);
        }
        [HttpGet("solution/{problemId}")]
        public async Task<IActionResult> GetProblemSolution(int problemId)
        {
            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null)
                return NotFound("Problem not found.");

            var solution = await _context.Solution
                .FirstOrDefaultAsync(s => s.ProblemId == problemId);
            if (solution == null)
                return NotFound("Solution not found.");

            var author = await _context.User
                .Where(u => u.UserId == problem.AuthorId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                ProblemTitle = problem.Name,
                Author = author,
                Explanation = solution.Explanation,
                Language = solution.Language,
                SourceCode = solution.Source
            });
        }

        [HttpGet("details/{problemId}")]
        public async Task<IActionResult> GetProblemDetails(int problemId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User not logged in.");
            int userId = int.Parse(userIdClaim.Value);

            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null) return NotFound("Problem not found.");

            var solution = await _context.Solution.FirstOrDefaultAsync(s => s.ProblemId == problemId);
            var moduleContent = await _context.ModuleContent.FindAsync(problem.ModuleContentId);
            var module = await _context.Modules.FindAsync(moduleContent.ModuleId);
            var section = await _context.Sections.FindAsync(module.SectionId);

            // 🔍 Xác định Status của bài với user hiện tại
            var userSubmissions = _context.Submission
                .Where(s => s.ProblemId == problemId && s.UserId == userId)
                .ToList();

            string status = "Not Started";
            if (userSubmissions.Any(s => s.Result == "Accepted"))
                status = "Completed";
            else if (userSubmissions.Any())
                status = "In Progress";

            // 🔍 Constraints
            var constraints = await _context.ProblemConstraint
                .Where(c => c.ProblemId == problemId)
                .Select(c => new
                {
                    c.Variable,
                    c.MinValue,
                    c.MaxValue
                })
                .ToListAsync();

            // 🔍 Sample Testcases
            var sampleTests = await _context.Testcase
                .Where(t => t.ProblemId == problemId && t.IsSample == true)
                .Select(t => new
                {
                    TestcaseId = t.TestcaseId,
                    Input = System.IO.File.ReadAllText(Path.Combine("wwwroot", t.InputPath.TrimStart('/'))),
                    Output = System.IO.File.ReadAllText(Path.Combine("wwwroot", t.OutputPath.TrimStart('/'))),
                    Explanation = t.Explanation
                })
                .ToListAsync();

            //Non Sample Testcase
            // 🔍 Non-sample Testcases
            var regularTests = await _context.Testcase
                .Where(t => t.ProblemId == problemId && t.IsSample == false)
                .Select(t => new
                {
                    TestcaseId = t.TestcaseId,
                    Input = System.IO.File.ReadAllText(Path.Combine("wwwroot", t.InputPath.TrimStart('/'))),
                    Output = System.IO.File.ReadAllText(Path.Combine("wwwroot", t.OutputPath.TrimStart('/'))),
                    Explanation = t.Explanation
                })
                .ToListAsync();


            return Ok(new
            {
                    Name = problem.Name,
                    Frequent = problem.Frequent,
                    ModuleContentName = moduleContent.Title,
                    SectionName = section.Name,
                    Difficulty = problem.Difficulty,
                    TimeLimit = problem.TimeLimit,
                    MemoryLimit = problem.MemoryLimit,
                    Status = status,
                    ProblemStatement = problem.ProblemStatement,
                    FormatInput = problem.FormatInput,
                    FormatOutput = problem.FormatOutput,
                    Constraints = constraints,
                    SampleTestcases = sampleTests,
                    Testcases = regularTests,
                    Solution = new
                    {
                        Language = solution?.Language ?? "cpp",
                        Explanation = solution?.Explanation ?? "",
                        Source = solution?.Source ?? ""
                    },
                    SectionId = section.Id,
                    ModuleId = module.Id,
                    ModuleContentId=moduleContent.Id
            });
        }



        private string SaveStdoutToFile(string stdout, int problemId, int submissionId, int testcaseId)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "submission_outputs", $"Problem_{problemId}");
            Directory.CreateDirectory(folderPath);

            var fileName = $"submission_{submissionId}_testcase_{testcaseId}.txt";
            var fullPath = Path.Combine(folderPath, fileName);

            System.IO.File.WriteAllText(fullPath, stdout ?? "");

            var relativePath = Path.Combine("submission_outputs", $"Problem_{problemId}", fileName);
            return "/" + relativePath.Replace("\\", "/");
        }


        private string SaveToFile(string content, int problemId, string fileType)
        {
            // Tạo đường dẫn thư mục: wwwroot/testcases/Problem_{id}/
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "testcases", $"Problem_{problemId}");
            Directory.CreateDirectory(folderPath); // Nếu chưa có thì tạo

            // Tạo file với tên duy nhất và rõ ràng: input_xxx.txt hoặc output_xxx.txt
            var fileName = $"{fileType}_{Guid.NewGuid()}.txt";
            var fullPath = Path.Combine(folderPath, fileName);

            // Ghi nội dung vào file
            System.IO.File.WriteAllText(fullPath, content);

            // Trả về đường dẫn tương đối để lưu vào DB (VD: /testcases/Problem_1/input_xyz.txt)
            var relativePath = Path.Combine("testcases", $"Problem_{problemId}", fileName);
            return "/" + relativePath.Replace("\\", "/");
        }


        // =================== Edit ===================
        [Authorize(Roles = "1,2")]
        [HttpPut("edit/{problemId}")]
        public async Task<IActionResult> EditProblem(int problemId, [FromBody] ProblemCreateRequest request)
        {
            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null) return NotFound("Problem not found");

            problem.Name = request.Name;
            problem.Frequent = request.Frequent;
            problem.ModuleContentId = request.ModuleContentId;
            problem.Difficulty = request.Difficulty;
            problem.TimeLimit = request.TimeLimit;
            problem.MemoryLimit = request.MemoryLimit;
            problem.ProblemStatement = request.ProblemStatement;
            problem.FormatInput = request.FormatInput;
            problem.FormatOutput = request.FormatOutput;
            _context.Problem.Update(problem);

            // Update solution
            var solution = await _context.Solution.FirstOrDefaultAsync(s => s.ProblemId == problemId);
            if (solution != null)
            {
                solution.Source = request.Solution.Source;
                solution.Language = request.Solution.Language;
                solution.Explanation = request.Solution.Explanation;
                _context.Solution.Update(solution);
            }

            // Remove old constraints and add new
            var oldConstraints = _context.ProblemConstraint.Where(c => c.ProblemId == problemId);
            _context.ProblemConstraint.RemoveRange(oldConstraints);
            foreach (var c in request.Constraints)
            {
                _context.ProblemConstraint.Add(new ProblemConstraint
                {
                    ProblemId = problemId,
                    Variable = c.Variable,
                    MinValue = c.MinValue,
                    MaxValue = c.MaxValue
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Problem updated successfully" });
        }

        // =================== Edit ===================
        [HttpGet("edit-details/{problemId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> GetProblemEditDetails(int problemId)
        {
            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null) return NotFound("Problem not found");

            var constraints = await _context.ProblemConstraint
                .Where(c => c.ProblemId == problemId)
                .ToListAsync();

            var solution = await _context.Solution.FirstOrDefaultAsync(s => s.ProblemId == problemId);

            var testcases = await _context.Testcase
                .Where(t => t.ProblemId == problemId)
                .Select(t => new
                {
                    t.TestcaseId,
                    t.InputPath,
                    t.OutputPath,
                    t.IsSample,
                    t.Explanation
                })
                .ToListAsync();

            return Ok(new
            {
                problem.ProblemId,
                problem.Name,
                problem.Frequent,
                problem.ModuleContentId,
                problem.Difficulty,
                problem.TimeLimit,
                problem.MemoryLimit,
                problem.ProblemStatement,
                problem.FormatInput,
                problem.FormatOutput,
                Constraints = constraints,
                Solution = solution,
                Testcases = testcases
            });
        }

        // =================== Edit ===================
        [HttpGet("get-testcase-file")]
        public IActionResult GetTestcaseFile([FromQuery] string filePath)
        {
            var fullPath = Path.Combine("wwwroot", filePath);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            string content = System.IO.File.ReadAllText(fullPath);
            return Ok(new { content });
        }

        // =================== Edit ===================
        [HttpPut("update-testcase-file")]
        public async Task<IActionResult> UpdateTestcaseFile([FromBody] FileUpdateRequest req)
        {
            var fullPath = Path.Combine("wwwroot", req.FilePath);
            System.IO.File.WriteAllText(fullPath, req.NewContent);

            var testcase = _context.Testcase.FirstOrDefault(t => t.InputPath == req.FilePath || t.OutputPath == req.FilePath);
            if (testcase != null)
            {
                int problemId = testcase.ProblemId;
                int testcaseId = testcase.TestcaseId;

                var submissionIds = _context.Submission
                    .Where(s => s.ProblemId == problemId)
                    .Select(s => s.SubmissionId)
                    .ToList();

                var resultsToDelete = _context.SubmissionResult
                    .Where(sr => sr.TestcaseId == testcaseId && submissionIds.Contains(sr.SubmissionId));

                _context.SubmissionResult.RemoveRange(resultsToDelete);
                await _context.SaveChangesAsync();
            }

            return Ok("Updated");
        }

        // =================== Edit ===================
        [HttpDelete("delete-testcase/{testcaseId}")]
        public async Task<IActionResult> DeleteTestcase(int testcaseId)
        {
            var tc = await _context.Testcase.FindAsync(testcaseId);
            if (tc == null) return NotFound();

            var inputPath = Path.Combine("wwwroot", tc.InputPath);
            var outputPath = Path.Combine("wwwroot", tc.OutputPath);
            if (System.IO.File.Exists(inputPath)) System.IO.File.Delete(inputPath);
            if (System.IO.File.Exists(outputPath)) System.IO.File.Delete(outputPath);

            var submissionIds = _context.Submission
                .Where(s => s.ProblemId == tc.ProblemId)
                .Select(s => s.SubmissionId)
                .ToList();

            var resultsToDelete = _context.SubmissionResult
                .Where(sr => sr.TestcaseId == tc.TestcaseId && submissionIds.Contains(sr.SubmissionId));

            _context.SubmissionResult.RemoveRange(resultsToDelete);
            _context.Testcase.Remove(tc);
            await _context.SaveChangesAsync();

            return Ok("Testcase deleted");
        }

        // =================== Edit ===================
        [HttpPost("add-manual-testcase")]
        public async Task<IActionResult> AddManualTestcase([FromBody] ManualTestcaseRequest req)
        {
            var testcase = new Testcase
            {
                ProblemId = req.ProblemId,
                InputPath = SaveToFile(req.Input, req.ProblemId, "input"),
                OutputPath = SaveToFile(req.Output, req.ProblemId, "output"),
                IsSample = true,
                Explanation = req.Explanation
            };

            _context.Testcase.Add(testcase);
            await _context.SaveChangesAsync();
            return Ok("Manual testcase added");
        }

        // =================== Edit ===================
        [HttpPost("add-auto-testcase")]
        public async Task<IActionResult> AddAutoTestcase([FromBody] AutoTestcaseRequest req)
        {
            int generatorLangId = _judge0Service.MapLanguageToId(req.GeneratorLanguage);
            int solutionLangId = _judge0Service.MapLanguageToId(req.SolutionLanguage);

            for (int i = 0; i < req.NumberOfTestcases; i++)
            {
                string generatedInput = await _judge0Service.RunCodeAndGetOutputWithoutInput(
                    req.GeneratorSource, generatorLangId.ToString());

                string expectedOutput = await _judge0Service.ExecuteAndGetOutputAsync(
                    req.SolutionSource, solutionLangId.ToString(), generatedInput);

                var testcase = new Testcase
                {
                    ProblemId = req.ProblemId,
                    InputPath = SaveToFile(generatedInput, req.ProblemId, "input"),
                    OutputPath = SaveToFile(expectedOutput, req.ProblemId, "output"),
                    IsSample = false
                };

                _context.Testcase.Add(testcase);
            }

            await _context.SaveChangesAsync();
            return Ok("Auto testcases added");
        }



        //==================NEW EDIT===================

        [Authorize(Roles = "1,2")]
        [HttpPut("update/{problemId}")]
        public async Task<IActionResult> UpdateProblem(int problemId, [FromBody] ProblemUpdateRequest request)
        {
            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null) return NotFound("Problem not found");

            // Cập nhật thông tin cơ bản
            problem.Name = request.Name;
            problem.ProblemStatement = request.ProblemStatement;
            problem.TimeLimit = request.TimeLimit;
            problem.MemoryLimit = request.MemoryLimit;
            problem.FormatInput = request.FormatInput;
            problem.FormatOutput = request.FormatOutput;
            problem.Difficulty = request.Difficulty;
            problem.ModuleContentId = request.ModuleContentId;
            problem.Frequent = request.Frequent;

            // Cập nhật Solution
            var solution = await _context.Solution.FirstOrDefaultAsync(s => s.ProblemId == problemId);
            if (solution == null)
            {
                solution = new Solution { ProblemId = problemId };
                _context.Solution.Add(solution);
            }

            solution.Language = request.SolutionLanguage;
            solution.Explanation = request.SolutionExplanation;
            solution.Source = request.SolutionSource;

            // Cập nhật Constraints
            var oldConstraints = _context.ProblemConstraint.Where(c => c.ProblemId == problemId);
            _context.ProblemConstraint.RemoveRange(oldConstraints);
            foreach (var c in request.Constraints)
            {
                _context.ProblemConstraint.Add(new ProblemConstraint
                {
                    ProblemId = problemId,
                    Variable = c.Variable,
                    MinValue = c.MinValue,
                    MaxValue = c.MaxValue
                });
            }

            await _context.SaveChangesAsync();
            return Ok("✅ Problem updated successfully");
        }


        [Authorize(Roles = "1,2")]
        [HttpPut("update-sample-testcases/{problemId}")]
        public async Task<IActionResult> UpdateSampleTestcases(int problemId, [FromBody] SampleTestcaseUpdateRequest request)
        {
            string basePath = Path.Combine("wwwroot", "testcases", $"Problem_{problemId}");
            Directory.CreateDirectory(basePath);

            // 1. Update existing
            foreach (var t in request.ToUpdate)
            {
                var tc = await _context.Testcase.FindAsync(t.TestcaseId);
                if (tc == null) continue;

                var relatedResults = _context.SubmissionResult.Where(r => r.TestcaseId == tc.TestcaseId);
                _context.SubmissionResult.RemoveRange(relatedResults);

                System.IO.File.WriteAllText(Path.Combine("wwwroot", tc.InputPath.TrimStart('/')), t.Input);
                System.IO.File.WriteAllText(Path.Combine("wwwroot", tc.OutputPath.TrimStart('/')), t.ExpectedOutput);
                tc.Explanation = t.Explanation;
            }

            // 2. Delete by ID
            foreach (var id in request.ToDelete)
            {
                var tc = await _context.Testcase.FindAsync(id);
                if (tc == null) continue;

                var relatedResults = _context.SubmissionResult.Where(r => r.TestcaseId == id);
                _context.SubmissionResult.RemoveRange(relatedResults);

                string inputFile = Path.Combine("wwwroot", tc.InputPath.TrimStart('/'));
                string outputFile = Path.Combine("wwwroot", tc.OutputPath.TrimStart('/'));
                if (System.IO.File.Exists(inputFile)) System.IO.File.Delete(inputFile);
                if (System.IO.File.Exists(outputFile)) System.IO.File.Delete(outputFile);

                _context.Testcase.Remove(tc);
            }

            // 3. Add new
            foreach (var t in request.ToAdd)
            {
                string Save(string content, string type)
                {
                    var fileName = $"{type}_{Guid.NewGuid()}.txt";
                    var filePath = Path.Combine(basePath, fileName);
                    System.IO.File.WriteAllText(filePath, content);
                    return $"/testcases/Problem_{problemId}/{fileName}";
                }

                _context.Testcase.Add(new Testcase
                {
                    ProblemId = problemId,
                    InputPath = Save(t.Input, "input"),
                    OutputPath = Save(t.ExpectedOutput, "output"),
                    IsSample = true,
                    Explanation = t.Explanation
                });
            }

            await _context.SaveChangesAsync();
            return Ok("✅ Sample testcases updated");
        }


        [Authorize(Roles = "1,2")]
        [HttpPut("update-non-sample-testcases/{problemId}")]
        public async Task<IActionResult> UpdateNonSampleTestcases(int problemId, [FromBody] NonSampleTestcaseUpdateRequest request)
        {
            string basePath = Path.Combine("wwwroot", "testcases", $"Problem_{problemId}");
            Directory.CreateDirectory(basePath);

            // 1. Update existing testcases
            foreach (var t in request.ToUpdate)
            {
                var tc = await _context.Testcase.FindAsync(t.TestcaseId);
                if (tc == null || tc.IsSample) continue; // bỏ qua nếu là sample hoặc không tồn tại

                // 🔥 Xóa các bản ghi SubmissionResult liên quan
                var relatedResults = _context.SubmissionResult.Where(r => r.TestcaseId == tc.TestcaseId);
                _context.SubmissionResult.RemoveRange(relatedResults);

                // Ghi lại file
                var inputPath = Path.Combine("wwwroot", tc.InputPath.TrimStart('/'));
                var outputPath = Path.Combine("wwwroot", tc.OutputPath.TrimStart('/'));

                System.IO.File.WriteAllText(inputPath, t.Input);
                System.IO.File.WriteAllText(outputPath, t.ExpectedOutput);

                tc.Explanation = t.Explanation;
            }

            // 2. Xóa testcase theo id
            foreach (var id in request.ToDelete)
            {
                var tc = await _context.Testcase.FindAsync(id);
                if (tc == null || tc.IsSample) continue;

                var relatedResults = _context.SubmissionResult.Where(r => r.TestcaseId == id);
                _context.SubmissionResult.RemoveRange(relatedResults);

                var inputPath = Path.Combine("wwwroot", tc.InputPath.TrimStart('/'));
                var outputPath = Path.Combine("wwwroot", tc.OutputPath.TrimStart('/'));

                if (System.IO.File.Exists(inputPath)) System.IO.File.Delete(inputPath);
                if (System.IO.File.Exists(outputPath)) System.IO.File.Delete(outputPath);

                _context.Testcase.Remove(tc);
            }

            // 3. Thêm testcase mới
            foreach (var t in request.ToAdd)
            {
                string Save(string content, string type)
                {
                    var fileName = $"{type}_{Guid.NewGuid()}.txt";
                    var filePath = Path.Combine(basePath, fileName);
                    System.IO.File.WriteAllText(filePath, content);
                    return $"/testcases/Problem_{problemId}/{fileName}";
                }

                _context.Testcase.Add(new Testcase
                {
                    ProblemId = problemId,
                    InputPath = Save(t.Input, "input"),
                    OutputPath = Save(t.ExpectedOutput, "output"),
                    IsSample = false,
                    Explanation = t.Explanation
                });
            }

            await _context.SaveChangesAsync();
            return Ok("✅ Non-sample testcases updated successfully");
        }

        [Authorize(Roles = "1,2")]
        [HttpPost("generate-random-testcases/{problemId}")]
        public async Task<IActionResult> GenerateRandomTestcases(int problemId, [FromBody] GenerateTestcaseRequest request)
        {
            var problem = await _context.Problem.FindAsync(problemId);
            if (problem == null)
                return NotFound("Problem not found");

            if (request.NumberOfTestcases <= 0)
                return BadRequest("Invalid number of testcases");

            string folder = Path.Combine("wwwroot", "testcases", $"Problem_{problemId}");
            Directory.CreateDirectory(folder);

            int generatorLangId = _judge0Service.MapLanguageToId(request.TestGeneratorLanguage);
            int solutionLangId = _judge0Service.MapLanguageToId(request.SolutionLanguage);

            for (int i = 0; i < request.NumberOfTestcases; i++)
            {
                // Sinh input từ generator
                string generatedInput = await _judge0Service.RunCodeAndGetOutputWithoutInput(
                    request.TestGeneratorSource, generatorLangId.ToString());

                if (string.IsNullOrWhiteSpace(generatedInput)) continue;

                // Sinh output từ solution
                string expectedOutput = await _judge0Service.ExecuteAndGetOutputAsync(
                    request.SolutionSource, solutionLangId.ToString(), generatedInput);

                if (expectedOutput == null) expectedOutput = "";

                // Ghi file
                string Save(string content, string type)
                {
                    var fileName = $"{type}_{Guid.NewGuid()}.txt";
                    var filePath = Path.Combine(folder, fileName);
                    System.IO.File.WriteAllText(filePath, content);
                    return $"/testcases/Problem_{problemId}/{fileName}";
                }

                _context.Testcase.Add(new Testcase
                {
                    ProblemId = problemId,
                    InputPath = Save(generatedInput, "input"),
                    OutputPath = Save(expectedOutput, "output"),
                    IsSample = false,
                    Explanation = null 
                });
            }

            await _context.SaveChangesAsync();
            return Ok("✅ Random testcases generated");
        }




    }



}
