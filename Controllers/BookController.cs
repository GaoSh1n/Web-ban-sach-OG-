using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Ban_Sach.Models;
using System.Net;
using System.Data.Entity;

namespace Web_Ban_Sach.Controllers
{
    public class BookController : Controller
    {
        // GET: Book

        private DBWebBanSachEntities db = new DBWebBanSachEntities();
        public ActionResult Details(int? id)
        {
            // Kiểm tra nếu không truyền id lên URL
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var book = db.Books.Include("Category")
                               .Include("Author")
                               .FirstOrDefault(b => b.MaSach == id.Value);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.RelatedBooks = db.Books
                                    .Where(b => b.MaTheLoai == book.MaTheLoai && b.MaSach != book.MaSach)
                                    .Take(3)
                                    .ToList();

            return View(book);
        }
    }
}