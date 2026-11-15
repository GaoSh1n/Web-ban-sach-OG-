using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Ban_Sach.Models;

namespace Web_Ban_Sach.Controllers
{
    public class HomeController : Controller
    {

        private DBWebBanSachEntities db = new DBWebBanSachEntities();
        [HttpGet]
        public ActionResult Index(int? categoryId, string searchString)
        {
            var books = db.Books.Include("Category").Include("Author").AsQueryable();

            if (categoryId.HasValue)
            {
                books = books.Where(b => b.MaTheLoai == categoryId.Value);

                // Lấy tên thể loại hiện tại để hiển thị ra tiêu đề trang
                var currentCategory = db.Categories.Find(categoryId.Value);
                if (currentCategory != null)
                {
                    ViewBag.CategoryTitle = currentCategory.TenTheLoai;
                }
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                books = books.Where(b => b.TenSach.Contains(searchString) || b.Author.TenTacGia.Contains(searchString));
                ViewBag.SearchString = searchString;
                ViewBag.CategoryTitle = "Kết quả tìm kiếm cho: \"" + searchString + "\"";
            }

            // 3. Trả về danh sách sách đã được lọc
            return View(books.ToList());
        }





        [HttpGet]
        public ActionResult CategoryPartial()
        {
            var categories = db.Categories.ToList();
            return PartialView("_CategoryPartial", categories);
        }
    }
}