using Microsoft.AspNetCore.Mvc;
using SV_CV.Models;
using SV_CV.Services;
using System.Diagnostics;

namespace SV_CV.Controllers
{
    public class HomeController : Controller
    {
        private readonly CvDraftStore _draftStore;

        public HomeController(CvDraftStore draftStore)
        {
            _draftStore = draftStore;
        }

        public IActionResult Index()
        {
            return View(new CvDocument
            {
                JobTitle = "Frontend Developer",
                Location = "Hà Nội",
                Summary = "Sinh viên đam mê xây dựng sản phẩm web thân thiện và dễ sử dụng.",
                Education = "Công nghệ thông tin — Đại học của bạn (2022 - 2026)",
                Skills = "HTML, CSS, JavaScript\nASP.NET Core\nGit và làm việc nhóm",
                Projects = "Website tạo CV — Xây dựng bằng ASP.NET Core MVC\nỨng dụng quản lý công việc — Thiết kế giao diện và API"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(CvDocument document, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Vui lòng kiểm tra lại họ tên và email." });
            }

            await _draftStore.SaveAsync(document, cancellationToken);
            return Ok(new { message = "Đã lưu bản nháp thành công." });
        }

        [HttpGet]
        public async Task<IActionResult> LoadDraft(CancellationToken cancellationToken)
        {
            var document = await _draftStore.LoadAsync(cancellationToken);
            return document is null ? NotFound() : Json(document);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
