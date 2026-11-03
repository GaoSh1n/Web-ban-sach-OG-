using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Ban_Sach.Models;

namespace Web_Ban_Sach.Controllers
{
    public class AdminController : Controller
    {

        private DBWebBanSachEntities db = new DBWebBanSachEntities();
        // GET: Admin
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["AdminRole"] != null)
            {
                return RedirectToAction("Books");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var admin = db.AdminUsers.SingleOrDefault(u => u.Username == username && u.Password == password);
            if( admin!= null)
            {
                Session["AdminId"] = admin.ID;
                Session["AdminUsername"] = admin.Username;
                Session["AdminRole"] = admin.Role;
                return RedirectToAction("Books");
            }
            ViewBag.Error = "Tên tài khoảng hoặc mật khẩu khống đúng.";
            return View();
        }
        [HttpGet]
        public ActionResult Logout()
        {
            Session["AdminId"] = null;
            Session["AdminUsername"] = null;
            Session["AdminRole"] = null;
            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult Books()
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }
            var books = db.Books.Include("Category").OrderByDescending(b => b.MaSach).ToList();
            return View(books);
        }


        // GET: Admin/CreateBook - Hiển thị form thêm sách
        [HttpGet]
        public ActionResult CreateBook()
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            // Lấy danh sách Thể loại và Tác giả truyền sang DropdownList
            ViewBag.MaTheLoai = new SelectList(db.Categories, "MaTheLoai", "TenTheLoai");
            ViewBag.MaTacGia = new SelectList(db.Authors, "MaTacGia", "TenTacGia");
            return View();
        }

        // POST: Admin/CreateBook - Lưu sách mới vào CSDL
        [HttpPost]
        public ActionResult CreateBook(Book book)
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Books"); // Lưu xong quay về danh sách sách
            }

            // Nếu dữ liệu lỗi, nạp lại DropdownList và giữ nguyên form
            ViewBag.MaTheLoai = new SelectList(db.Categories, "MaTheLoai", "TenTheLoai", book.MaTheLoai);
            ViewBag.MaTacGia = new SelectList(db.Authors, "MaTacGia", "TenTacGia", book.MaTacGia);
            return View(book);
        }

        [HttpGet]
        public ActionResult EditBook(int id)
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            var book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            // Nạp DropdownList và chọn sẵn giá trị hiện tại của sách
            ViewBag.MaTheLoai = new SelectList(db.Categories, "MaTheLoai", "TenTheLoai", book.MaTheLoai);
            ViewBag.MaTacGia = new SelectList(db.Authors, "MaTacGia", "TenTacGia", book.MaTacGia);
            return View(book);
        }

        // POST: Admin/EditBook - Lưu thay đổi thông tin và số lượng tồn kho
        [HttpPost]
        public ActionResult EditBook(Book book)
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Books");
            }

            ViewBag.MaTheLoai = new SelectList(db.Categories, "MaTheLoai", "TenTheLoai", book.MaTheLoai);
            ViewBag.MaTacGia = new SelectList(db.Authors, "MaTacGia", "TenTacGia", book.MaTacGia);
            return View(book);
        }

        [HttpGet]
        public ActionResult DeleteBook(int id)
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            var book = db.Books.Include("Category")
                               .Include("Author")
                               .SingleOrDefault(b => b.MaSach == id);
            if (book == null)
            {
                return HttpNotFound();
            }

            return View(book);
        }

        // POST: Admin/DeleteBook/5 - Thực hiện xóa sách
        [HttpPost, ActionName("DeleteBook")]
        public ActionResult DeleteBookConfirmed(int id)
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            var book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            // 1. Kiểm tra xem sách đã nằm trong đơn hàng (OrderDetail) nào chưa
            bool hasOrders = db.OrderDetails.Any(od => od.MaSach == id);
            if (hasOrders)
            {
                // Nếu đã có đơn hàng mua sách này, không được xóa vật lý mà chỉ đưa tồn kho về 0
                ViewBag.Error = "Sách này đã phát sinh trong đơn hàng của khách nên không thể xóa khỏi CSDL! Bạn có thể chỉnh số lượng tồn kho về 0.";
                return View(book);
            }

            // 2. Nếu sách đang nằm tạm trong giỏ hàng (CartItem) của khách, xóa khỏi giỏ trước
            var cartItems = db.CartItems.Where(ci => ci.MaSach == id).ToList();
            if (cartItems.Any())
            {
                db.CartItems.RemoveRange(cartItems);
            }

            // 3. Xóa sách khỏi bảng Book
            db.Books.Remove(book);
            db.SaveChanges();

            return RedirectToAction("Books");
        }

        [HttpPost]

        public ActionResult UpdateOrderStatus(int maDonHang, int tinhTrangGiaoHang, bool daThanhToan = false)
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            var order = db.Orders.Find(maDonHang);
            if (order != null)
            {
                order.TinhTrangGiaoHang = tinhTrangGiaoHang;
                order.DaThanhToan = daThanhToan;
                db.SaveChanges();
            }

            return RedirectToAction("Orders");
        }

        [HttpGet]
        public ActionResult Orders()
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            // Nạp danh sách đơn hàng kèm thông tin khách hàng, xếp đơn mới nhất lên đầu
            var orders = db.Orders.Include("Customer")
                                  .OrderByDescending(o => o.NgayDat)
                                  .ToList();
            return View(orders);
        }


        [HttpGet]
        public ActionResult OrderDetail(int id)
        {
            if (Session["AdminRole"] == null)
            {
                return RedirectToAction("Login");
            }

            var order = db.Orders.Include("Customer")
                                 .Include("OrderDetails.Book")
                                 .SingleOrDefault(o => o.MaDonHang == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        [HttpPost]
        public ActionResult QuickCreateAuthor(string tenTacGia, string returnUrl)
        {
            if (Session["AdminRole"] == null) return RedirectToAction("Login");

            if (!string.IsNullOrWhiteSpace(tenTacGia))
            {
                // Kiểm tra xem tác giả đã tồn tại chưa
                var existingAuthor = db.Authors.FirstOrDefault(a => a.TenTacGia.ToLower() == tenTacGia.Trim().ToLower());
                if (existingAuthor == null)
                {
                    var author = new Author { TenTacGia = tenTacGia.Trim() };
                    db.Authors.Add(author);
                    db.SaveChanges();
                    TempData["Success"] = "Đã thêm tác giả mới: " + author.TenTacGia;
                }
                else
                {
                    TempData["Error"] = "Tác giả này đã tồn tại trong danh sách!";
                }
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("CreateBook");
        }

        // POST: Admin/QuickCreateCategory - Thêm nhanh Thể loại
        [HttpPost]
        public ActionResult QuickCreateCategory(string tenTheLoai, string returnUrl)
        {
            if (Session["AdminRole"] == null) return RedirectToAction("Login");

            if (!string.IsNullOrWhiteSpace(tenTheLoai))
            {
                // Kiểm tra xem thể loại đã tồn tại chưa
                var existingCat = db.Categories.FirstOrDefault(c => c.TenTheLoai.ToLower() == tenTheLoai.Trim().ToLower());
                if (existingCat == null)
                {
                    var cat = new Category { TenTheLoai = tenTheLoai.Trim() };
                    db.Categories.Add(cat);
                    db.SaveChanges();
                    TempData["Success"] = "Đã thêm thể loại mới: " + cat.TenTheLoai;
                }
                else
                {
                    TempData["Error"] = "Thể loại này đã tồn tại trong danh sách!";
                }
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("CreateBook");
        }

    }
}