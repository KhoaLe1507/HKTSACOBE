using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using HKT_OJ.Models;
using HKT_OJ.Data;
using HKT_OJ.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Security.Claims;
namespace HKT_OJ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoadmapController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoadmapController(AppDbContext context)
        {
            _context = context;
        }

        // ===== SECTION =====
        [Authorize(Roles = "2")]
        [HttpPost("AddSection")]
        public IActionResult AddSection([FromBody] AddSectionRequest request)
        {
            var reference = _context.Sections.FirstOrDefault(s => s.Id == request.ReferenceId);
            int newOrder = 0;

            if (request.Position == "Front" && reference != null)
            {
                newOrder = reference.Order;
                var affected = _context.Sections.Where(s => s.Order >= newOrder).ToList();
                foreach (var s in affected) s.Order++;
            }
            else if (request.Position == "Behind" && reference != null)
            {
                newOrder = reference.Order + 1;
                var affected = _context.Sections.Where(s => s.Order > reference.Order).ToList();
                foreach (var s in affected) s.Order++;
            }
            else // At the end
            {
                newOrder = _context.Sections.Any() ? _context.Sections.Max(s => s.Order) + 1 : 1;
            }

            var newSection = new Sections
            {
                Name = request.Name,
                Description = request.Description,
                Order = newOrder
            };

            _context.Sections.Add(newSection);
            _context.SaveChanges();

            return Ok("Section added successfully.");
        }

        // ===== MODULE =====
        [Authorize(Roles = "2")]
        [HttpPost("AddModule")]
        public IActionResult AddModule([FromBody] AddModuleRequest request)
        {
            var reference = _context.Modules.FirstOrDefault(m => m.Id == request.ReferenceId && m.SectionId == request.SectionId);
            int newOrder = 0;

            if (request.Position == "Front" && reference != null)
            {
                newOrder = reference.Order;
                var affected = _context.Modules.Where(m => m.SectionId == request.SectionId && m.Order >= newOrder).ToList();
                foreach (var m in affected) m.Order++;
            }
            else if (request.Position == "Behind" && reference != null)
            {
                newOrder = reference.Order + 1;
                var affected = _context.Modules.Where(m => m.SectionId == request.SectionId && m.Order > reference.Order).ToList();
                foreach (var m in affected) m.Order++;
            }
            else // At the end
            {
                var modules = _context.Modules.Where(m => m.SectionId == request.SectionId);
                newOrder = modules.Any() ? modules.Max(m => m.Order) + 1 : 1;
            }

            var newModule = new Modules
            {
                Name = request.Name,
                SectionId = request.SectionId,
                Order = newOrder
            };

            _context.Modules.Add(newModule);
            _context.SaveChanges();

            return Ok("Module added successfully.");
        }

        // ===== MODULE CONTENT =====
        [Authorize(Roles = "2")]
        [HttpPost("AddModuleContent")]
        public IActionResult AddModuleContent([FromBody] AddModuleContentRequest request)
        {
            var reference = _context.ModuleContent.FirstOrDefault(mc => mc.Id == request.ReferenceId && mc.ModuleId == request.ModuleId);
            int newOrder = 0;

            if (request.Position == "Front" && reference != null)
            {
                newOrder = reference.Order;
                var affected = _context.ModuleContent.Where(mc => mc.ModuleId == request.ModuleId && mc.Order >= newOrder).ToList();
                foreach (var mc in affected) mc.Order++;
            }
            else if (request.Position == "Behind" && reference != null)
            {
                newOrder = reference.Order + 1;
                var affected = _context.ModuleContent.Where(mc => mc.ModuleId == request.ModuleId && mc.Order > reference.Order).ToList();
                foreach (var mc in affected) mc.Order++;
            }
            else // At the end
            {
                var contents = _context.ModuleContent.Where(mc => mc.ModuleId == request.ModuleId);
                newOrder = contents.Any() ? contents.Max(mc => mc.Order) + 1 : 1;
            }

            var newContent = new ModuleContent
            {
                Title = request.Title,
                Frequent = request.Frequent,
                Description = request.Description,
                HtmlContentPath = request.HtmlContentPath,
                AuthorId = request.AuthorId,
                ModuleId = request.ModuleId,
                Order = newOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.ModuleContent.Add(newContent);
            _context.SaveChanges();

            return Ok("ModuleContent added successfully.");
        }

        // ===== GET: Select Section for Dropdown =====
        [Authorize(Roles = "2")]
        [HttpGet("GetAllSections")]
        public IActionResult GetAllSections()
        {
            var sections = _context.Sections
                .OrderBy(s => s.Order)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToList();

            return Ok(sections);
        }

        // ===== GET: Select Module theo Section =====
        [Authorize(Roles = "2")]
        [HttpGet("GetModulesBySection/{sectionId}")]
        public IActionResult GetModulesBySection(int sectionId)
        {
            var modules = _context.Modules
                .Where(m => m.SectionId == sectionId)
                .OrderBy(m => m.Order)
                .Select(m => new
                {
                    m.Id,
                    m.Name
                })
                .ToList();

            return Ok(modules);
        }

        // ===== GET: Select ModuleContent theo Module =====
        [Authorize(Roles = "2")]
        [HttpGet("GetModuleContentsByModule/{moduleId}")]
        public IActionResult GetModuleContentsByModule(int moduleId)
        {
            var contents = _context.ModuleContent
                .Where(mc => mc.ModuleId == moduleId)
                .OrderBy(mc => mc.Order)
                .Select(mc => new
                {
                    mc.Id,
                    mc.Title
                })
                .ToList();

            return Ok(contents);
        }

        // ===== GET: Select all Professors (role = 1) =====
        [Authorize(Roles = "2")]
        [HttpGet("GetAllProfessors")]
        public IActionResult GetAllProfessors()
        {
            var professors = _context.User
                .Where(u => u.Role == 1) // Role = 1 là Professor
                .Select(u => new
                {
                    u.UserId,
                    u.FullName
                })
                .ToList();

            return Ok(professors);
        }

        //======= Dropdown all Sections ======
        [HttpGet("ListAllSectionsDropdown")]
        public IActionResult ListAllSectionsDropdown()
        {
            var sections = _context.Sections
                .OrderBy(s => s.Order)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToList();

            return Ok(sections);
        }

        //========== List All Module Content and Module by Section ID ( Student ) ========
        //[Authorize(Roles = "2")]
        [HttpGet("ListAllModuleContentAndModuleBySectionId/{sectionId}")]
        public IActionResult ListAllModuleContentAndModuleBySectionId(int sectionId)
        {
            // Lấy thông tin Section
            var sectionInfo = _context.Sections
                .Where(s => s.Id == sectionId)
                .Select(s => new
                {
                    SectionId = s.Id,
                    SectionName = s.Name,
                    SectionDescription = s.Description
                })
                .FirstOrDefault();

            if (sectionInfo == null)
                return NotFound("Section not found");

            // Lấy danh sách Module và ModuleContent
            var modules = _context.Modules
                .Where(m => m.SectionId == sectionId)
                .OrderBy(m => m.Order)
                .Select(m => new
                {
                    ModuleId = m.Id,
                    ModuleName = m.Name,
                    ModuleOrder = m.Order,
                    Contents = _context.ModuleContent
                        .Where(mc => mc.ModuleId == m.Id)
                        .OrderBy(mc => mc.Order)
                        .Select(mc => new
                        {
                            mc.Id,
                            mc.Title,
                            mc.Description,
                            mc.Order
                        })
                        .ToList()
                })
                .ToList();

            return Ok(new
            {
                sectionInfo.SectionId,
                sectionInfo.SectionName,
                sectionInfo.SectionDescription,
                Modules = modules
            });
        }


        //============== List All Sections Details ( Admin ) ===========
        [Authorize(Roles = "2")]
        [HttpGet("ListAllSectionsDetails")]
        public IActionResult ListAllSectionsDetails()
        {
            var sections = _context.Sections
                .OrderBy(s => s.Order)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Order
                })
                .ToList();

            return Ok(sections);
        }

        //============== List All Modules By Section Id Details ===========
        [Authorize(Roles = "2")]
        [HttpGet("ListAllModulesBySectionIdDetails/{sectionId}")]
        public IActionResult ListAllModulesBySectionIdDetails(int sectionId)
        {
            var modules = _context.Modules
                .Where(m => m.SectionId == sectionId)
                .OrderBy(m => m.Order)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.SectionId,
                    m.Order
                })
                .ToList();

            return Ok(modules);
        }

        //============== List All Module Contents By Module Id Detail===========
        [Authorize(Roles = "2")]
        [HttpGet("ListAllModuleContentsByModuleIdDetails/{moduleId}")]
        public IActionResult ListAllModuleContentsByModuleIdDetails(int moduleId)
        {
            var contents = _context.ModuleContent
                .Where(mc => mc.ModuleId == moduleId)
                .OrderBy(mc => mc.Order)
                .Select(mc => new
                {
                    mc.Id,
                    mc.Title,
                    mc.Description,
                    mc.Frequent,
                    mc.HtmlContentPath,
                    mc.AuthorId,
                    mc.ModuleId,
                    mc.Order,
                    mc.CreatedAt
                })
                .ToList();

            return Ok(contents);
        }


        //=== GET DETAIL MODULE CONTENT =====
        [HttpGet("GetDetailModuleContent/{id}")]
        public IActionResult GetDetailModuleContent(int id)
        {
            var mc = _context.ModuleContent
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Frequent,
                    c.HtmlContentPath,
                    c.ModuleId,
                    c.AuthorId,
                    c.Order,
                    c.CreatedAt
                })
                .FirstOrDefault();

            if (mc == null) return NotFound("ModuleContent not found");
            return Ok(mc);
        }
        ///c => c.Type == ClaimTypes.NameIdentifier

        [Authorize(Roles = "1")]
        [HttpGet("MyModuleContents")]
        public IActionResult GetMyModuleContents()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int authorId = int.Parse(userIdClaim.Value);

            // Lấy tất cả ModuleContent có AuthorId trùng
            var contents = _context.ModuleContent
                .Where(mc => mc.AuthorId == authorId)
                .ToList();

            // Nhóm theo Section -> Module -> ModuleContent
            var grouped = contents
                .Join(_context.Modules, mc => mc.ModuleId, m => m.Id, (mc, m) => new { mc, m })
                .Join(_context.Sections, mm => mm.m.SectionId, s => s.Id, (mm, s) => new { mm.mc, mm.m, s })
                .GroupBy(x => new { x.s.Id, x.s.Name, x.s.Order })
                .Select(gSection => new
                {
                    SectionId = gSection.Key.Id,
                    SectionName = gSection.Key.Name,
                    SectionOrder = gSection.Key.Order,
                    Modules = gSection
                        .GroupBy(x => new { x.m.Id, x.m.Name })
                        .Select(gModule => new
                        {
                            ModuleId = gModule.Key.Id,
                            ModuleName = gModule.Key.Name,
                            Contents = gModule.Select(x => new
                            {
                                x.mc.Id,
                                x.mc.Title,
                                x.mc.Description,
                                x.mc.Frequent,
                                x.mc.HtmlContentPath,
                                x.mc.Order,
                                x.mc.CreatedAt
                            }).OrderBy(mc => mc.Order).ToList()
                        }).OrderBy(m => m.ModuleId).ToList()
                })
                .OrderBy(s => s.SectionOrder) // ✅ sắp theo Order
                .ToList();


            return Ok(grouped);
        }




        // --- Xoá Section và cập nhật lại Order ---
        private void RemoveSectionAndUpdateOrder(Sections section)
        {
            int deletedOrder = section.Order;
            _context.Sections.Remove(section);

            var affected = _context.Sections.Where(s => s.Order > deletedOrder).ToList();
            foreach (var s in affected) s.Order--;
        }

        // --- Xoá Module và cập nhật lại Order ---
        private void RemoveModuleAndUpdateOrder(Modules module)
        {
            int deletedOrder = module.Order;
            int sectionId = module.SectionId;
            _context.Modules.Remove(module);

            var affected = _context.Modules.Where(m => m.SectionId == sectionId && m.Order > deletedOrder).ToList();
            foreach (var m in affected) m.Order--;
        }

        // --- Xoá ModuleContent và cập nhật lại Order ---
        private void RemoveModuleContentAndUpdateOrder(ModuleContent mc)
        {
            int deletedOrder = mc.Order;
            int moduleId = mc.ModuleId;
            _context.ModuleContent.Remove(mc);

            var affected = _context.ModuleContent.Where(c => c.ModuleId == moduleId && c.Order > deletedOrder).ToList();
            foreach (var c in affected) c.Order--;
        }



        // ===== EDIT SECTION =====
        [Authorize(Roles = "2")]
        [HttpPut("EditSection/{id}")]
        public IActionResult EditSection(int id, [FromBody] AddSectionRequest request)
        {

            if (request.Position == "At the end") request.ReferenceId = -1;
            var section = _context.Sections.FirstOrDefault(s => s.Id == id);
            if (section == null)
                return NotFound("Section not found");

            if (string.IsNullOrEmpty(request.Position) || request.ReferenceId == 0)
            {
                // Không có yêu cầu thay đổi vị trí → chỉ update Name, Description
                section.Name = request.Name;
                section.Description = request.Description;
                _context.SaveChanges();
                return Ok("Section updated (no position change).");
            }
            section.Name = request.Name;
            section.Description = request.Description;

            var sections = _context.Sections.OrderBy(s => s.Order).ToList();

            // Tìm vị trí hiện tại
            int currentIndex = sections.FindIndex(s => s.Id == section.Id);

            // Remove phần tử đang chỉnh
            sections.RemoveAt(currentIndex);

            int insertIndex = sections.Count; // mặc định chèn cuối

            if (request.Position == "Front")
            {
                insertIndex = sections.FindIndex(s => s.Id == request.ReferenceId);

                // Nếu phần tử gốc nằm TRƯỚC reference → cần trừ 1 index
                if (currentIndex < insertIndex)
                    insertIndex--;
            }
            else if (request.Position == "Behind")
            {
                insertIndex = sections.FindIndex(s => s.Id == request.ReferenceId);

                // Nếu phần tử gốc nằm SAU reference → giữ nguyên
                // Nếu nằm TRƯỚC → cần trừ 1
                if (currentIndex < insertIndex)
                    insertIndex++;
                else
                    insertIndex++;
            }
            else if (request.Position == "At the end")
            {
                insertIndex = sections.Count; // chèn vào cuối danh sách
            }

            sections.Insert(insertIndex, section);

            // Cập nhật lại Order
            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].Order = i + 1;
            }

            _context.SaveChanges();
            return Ok("Section updated successfully.");
        }



        // ===== EDIT MODULE =====
        [Authorize(Roles = "2")]
        [HttpPut("EditModule/{id}")]
        public IActionResult EditModule(int id, [FromBody] AddModuleRequest request)
        {
            if (request.Position == "At the end") request.ReferenceId = -1;

            var module = _context.Modules.FirstOrDefault(m => m.Id == id);
            if (module == null)
                return NotFound("Module not found");

            if (string.IsNullOrEmpty(request.Position) || request.ReferenceId == 0)
            {
                module.Name = request.Name;
                module.SectionId = request.SectionId;
                _context.SaveChanges();
                return Ok("Module updated (no position change).");
            }

            module.Name = request.Name;
            module.SectionId = request.SectionId;

            var modules = _context.Modules
                .Where(m => m.SectionId == request.SectionId)
                .OrderBy(m => m.Order)
                .ToList();

            int currentIndex = modules.FindIndex(m => m.Id == module.Id);
            modules.RemoveAt(currentIndex);

            int insertIndex = modules.Count;

            if (request.Position == "Front")
            {
                insertIndex = modules.FindIndex(m => m.Id == request.ReferenceId);
                if (currentIndex < insertIndex)
                    insertIndex--;
            }
            else if (request.Position == "Behind")
            {
                insertIndex = modules.FindIndex(m => m.Id == request.ReferenceId);
                if (currentIndex < insertIndex)
                    insertIndex++;
                else
                    insertIndex++;
            }
            else if (request.Position == "At the end")
            {
                insertIndex = modules.Count;
            }

            modules.Insert(insertIndex, module);

            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].Order = i + 1;
            }

            _context.SaveChanges();
            return Ok("Module updated successfully.");
        }




        // ===== EDIT MODULE CONTENT =====
        [Authorize(Roles = "1,2")]
        [HttpPut("EditModuleContent/{id}")]
        public IActionResult EditModuleContent(int id, [FromBody] AddModuleContentRequest request)
        {
            if (request.Position == "At the end") request.ReferenceId = -1;

            var mc = _context.ModuleContent.FirstOrDefault(m => m.Id == id);
            if (mc == null)
                return NotFound("ModuleContent not found");

            if (string.IsNullOrEmpty(request.Position) || request.ReferenceId == 0)
            {
                mc.Title = request.Title;
                mc.Description = request.Description;
                mc.Frequent = request.Frequent;
                mc.HtmlContentPath = request.HtmlContentPath;
                mc.AuthorId = request.AuthorId;
                mc.ModuleId = request.ModuleId;

                _context.SaveChanges();
                return Ok("ModuleContent updated (no position change).");
            }

            mc.Title = request.Title;
            mc.Description = request.Description;
            mc.Frequent = request.Frequent;
            mc.HtmlContentPath = request.HtmlContentPath;
            mc.AuthorId = request.AuthorId;
            mc.ModuleId = request.ModuleId;

            var contents = _context.ModuleContent
                .Where(c => c.ModuleId == request.ModuleId)
                .OrderBy(c => c.Order)
                .ToList();

            int currentIndex = contents.FindIndex(c => c.Id == mc.Id);
            contents.RemoveAt(currentIndex);

            int insertIndex = contents.Count;

            if (request.Position == "Front")
            {
                insertIndex = contents.FindIndex(c => c.Id == request.ReferenceId);
                if (currentIndex < insertIndex)
                    insertIndex--;
            }
            else if (request.Position == "Behind")
            {
                insertIndex = contents.FindIndex(c => c.Id == request.ReferenceId);
                if (currentIndex < insertIndex)
                    insertIndex++;
                else
                    insertIndex++;
            }
            else if (request.Position == "At the end")
            {
                insertIndex = contents.Count;
            }

            contents.Insert(insertIndex, mc);

            for (int i = 0; i < contents.Count; i++)
            {
                contents[i].Order = i + 1;
            }

            _context.SaveChanges();
            return Ok("ModuleContent updated successfully.");
        }



        // ===== Delete Section =====
        [Authorize(Roles = "2")]
        [HttpDelete("DeleteSection/{id}")]
        public IActionResult DeleteSection(int id)
        {
            var section = _context.Sections.FirstOrDefault(s => s.Id == id);
            if (section == null)
                return NotFound("Section not found");

            RemoveSectionAndUpdateOrder(section);
            _context.SaveChanges();

            return Ok("Section deleted successfully.");
        }

        // ===== Delete Module =====
        [Authorize(Roles = "2")]
        [HttpDelete("DeleteModule/{id}")]
        public IActionResult DeleteModule(int id)
        {
            var module = _context.Modules.FirstOrDefault(m => m.Id == id);
            if (module == null)
                return NotFound("Module not found");

            RemoveModuleAndUpdateOrder(module);
            _context.SaveChanges();

            return Ok("Module deleted successfully.");
        }

        // ===== Delete Module Content =====
        [Authorize(Roles = "2")]
        [HttpDelete("DeleteModuleContent/{id}")]
        public IActionResult DeleteModuleContent(int id)
        {
            var mc = _context.ModuleContent.FirstOrDefault(m => m.Id == id);
            if (mc == null)
                return NotFound("ModuleContent not found");

            RemoveModuleContentAndUpdateOrder(mc);
            _context.SaveChanges();

            return Ok("ModuleContent deleted successfully.");
        }


    }


}
