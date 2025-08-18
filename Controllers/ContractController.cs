using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chinhsachso.Data;
using Chinhsachso.Models;
using Tesseract;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using ImageMagick;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Chinhsachso.Controllers
{
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContractController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Danh sách hợp đồng
        public IActionResult Index()
        {
            var contracts = _context.Contracts.ToList();
            return View(contracts);
        }

        // GET: Upload
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string ContractName, IFormFile File)
        {
            if (File == null || File.Length == 0)
            {
                ModelState.AddModelError("File", "Vui lòng chọn file.");
                return View();
            }

            if (File.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("File", "File vượt quá 10MB.");
                return View();
            }

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(File.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("File", "Chỉ chấp nhận PDF, JPG, JPEG, PNG.");
                return View();
            }

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "temp");
            Directory.CreateDirectory(uploadsPath);

            var uniqueId = Guid.NewGuid().ToString();
            var fileName = $"{uniqueId}_{File.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await File.CopyToAsync(stream);
            }

            string pdfPath = filePath;
            StringBuilder ocrText = new StringBuilder();
            string tempImagesPath = "";

            try
            {
                var tessDataPath = Path.Combine(_env.ContentRootPath, "tessdata");
                if (!Directory.Exists(tessDataPath))
                    throw new DirectoryNotFoundException("Không tìm thấy thư mục tessdata.");

                if (fileExtension != ".pdf")
                {
                    pdfPath = Path.ChangeExtension(filePath, ".pdf");
                    using (var doc = new PdfDocument())
                    {
                        var page = doc.AddPage();
                        using (var gfx = XGraphics.FromPdfPage(page))
                        using (var img = XImage.FromFile(filePath))
                        {
                            gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                        }
                        doc.Save(pdfPath);
                    }

                    using var engine = new TesseractEngine(tessDataPath, "vie+eng", EngineMode.Default);
                    using var imgPix = Pix.LoadFromFile(filePath);
                    using var pageOCR = engine.Process(imgPix);
                    ocrText.Append(pageOCR.GetText());
                }
                else
                {
                    tempImagesPath = Path.Combine(uploadsPath, $"pdf_images_{uniqueId}");
                    Directory.CreateDirectory(tempImagesPath);

                    var settings = new MagickReadSettings { Density = new Density(200) };
                    using (var images = new MagickImageCollection())
                    {
                        images.Read(filePath, settings);
                        int pageIndex = 1;
                        foreach (var image in images)
                        {
                            image.Format = MagickFormat.Png;
                            var imgPath = Path.Combine(tempImagesPath, $"page_{pageIndex}.png");
                            image.Write(imgPath);
                            pageIndex++;
                        }
                    }

                    using var engine = new TesseractEngine(tessDataPath, "vie+eng", EngineMode.Default);
                    foreach (var imgFile in Directory.GetFiles(tempImagesPath, "*.png"))
                    {
                        using var imgPix = Pix.LoadFromFile(imgFile);
                        using var pageOCR = engine.Process(imgPix);
                        ocrText.Append(pageOCR.GetText()).Append("\n");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi OCR: {ex.Message}";
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempImagesPath) && Directory.Exists(tempImagesPath))
                {
                    Directory.Delete(tempImagesPath, true);
                }
            }

            var newContract = new Contract
            {
                ContractName = ContractName,
                FileName = fileName,
                FilePath = pdfPath,
                OCRText = ocrText.ToString(),
                UploadedAt = DateTime.UtcNow.AddHours(7),
                CreatedBy = User.Identity?.Name ?? "admin"
            };

            _context.Contracts.Add(newContract);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm hợp đồng thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Sửa hợp đồng
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var contract = _context.Contracts.Find(id);
            if (contract == null)
                return NotFound();

            return View(contract);
        }

        // POST: Sửa hợp đồng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string ContractName, string OCRText)
        {
            var existingContract = await _context.Contracts.FindAsync(id);
            if (existingContract == null)
                return NotFound();

            existingContract.ContractName = ContractName;
            existingContract.OCRText = OCRText;
            existingContract.CreatedBy = User.Identity?.Name ?? "admin";

            _context.Update(existingContract);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thành công.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Xóa hợp đồng
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var contract = _context.Contracts.Find(id);
            if (contract == null)
                return NotFound();

            return View(contract);
        }

        // POST: Xóa hợp đồng
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            if (System.IO.File.Exists(contract.FilePath))
                System.IO.File.Delete(contract.FilePath);

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa hợp đồng.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Xóa toàn bộ hợp đồng
        [HttpGet]
        public IActionResult DeleteAll()
        {
            return View();
        }

        // POST: Xóa toàn bộ hợp đồng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllConfirmed()
        {
            var contracts = _context.Contracts.ToList();
            foreach (var contract in contracts)
            {
                if (System.IO.File.Exists(contract.FilePath))
                    System.IO.File.Delete(contract.FilePath);
            }
            _context.Contracts.RemoveRange(contracts);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa toàn bộ hợp đồng.";
            return RedirectToAction(nameof(Index));
        }
    }
}
