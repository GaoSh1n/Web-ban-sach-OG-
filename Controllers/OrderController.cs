using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Web_Ban_Sach.Models;

namespace Web_Ban_Sach.Controllers
{
    public class OrderController : Controller
    {
        private DBWebBanSachEntities db = new DBWebBanSachEntities();

        // ----------------------------------------------------
        // GET: Order/Checkout - Hiển thị trang xác nhận đặt hàng
        // ----------------------------------------------------
        [HttpGet]
        public ActionResult Checkout()
        {
            // 1. Kiểm tra đăng nhập (bắt buộc khách phải đăng nhập trước khi thanh toán)
            if (Session["CustomerId"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản để tiếp tục thanh toán.";
                return RedirectToAction("Login", "Account");
            }

            // 2. Kiểm tra giỏ hàng
            if (Session["CartId"] == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            int cartId = (int)Session["CartId"];
            var cartItems = db.CartItems.Include(c => c.Book)
                                       .Where(c => c.MaGioHang == cartId)
                                       .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // 3. Lấy thông tin khách hàng để điền sẵn vào form nhận hàng
            int customerId = (int)Session["CustomerId"];
            var customer = db.Customers.Find(customerId);

            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = cartItems.Sum(c => c.SoLuong * c.Book.GiaBan);

            // Gán dữ liệu mặc định vào model Order
            var order = new Order
            {
                MaKH = customerId,
                
            };

            return View(order);
        }

        // ----------------------------------------------------
        // POST: Order/Checkout - Lưu đơn hàng và chi tiết đơn hàng
        // ----------------------------------------------------
        [HttpPost]
        public ActionResult Checkout(Order orderModel, string paymentMethod)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int cartId = (int)Session["CartId"];
            var cartItems = db.CartItems.Include("Book")
                                       .Where(c => c.MaGioHang == cartId)
                                       .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            int customerId = (int)Session["CustomerId"];

            // Đơn hàng mới tạo luôn có TinhTrangGiaoHang = 0 (Chờ xử lý)
            var newOrder = new Order
            {
                MaKH = customerId,
                NgayDat = DateTime.Now,
                TongTien = cartItems.Sum(c => c.SoLuong * c.Book.GiaBan),
                TinhTrangGiaoHang = 0
            };

            db.Orders.Add(newOrder);
            db.SaveChanges(); // Sinh MaDonHang

            // Lưu OrderDetail và trừ kho sách
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    MaDonHang = newOrder.MaDonHang,
                    MaSach = item.MaSach,
                    SoLuong = item.SoLuong,
                    DonGia = item.Book.GiaBan
                };
                db.OrderDetails.Add(orderDetail);

                var book = db.Books.Find(item.MaSach);
                if (book != null)
                {
                    book.SoLuongTon = Math.Max(0, book.SoLuongTon - item.SoLuong);
                }
            }

            // Xóa giỏ hàng
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            Session["CartCount"] = 0;

            // Chuyển hướng sang trang Success
            return RedirectToAction("Success", new { id = newOrder.MaDonHang, method = paymentMethod });
        }

        // ----------------------------------------------------
        // GET: Order/Success - Thông báo đặt hàng thành công
        // ----------------------------------------------------
        [HttpGet]
        public ActionResult Success(int id, string method = "COD")
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            ViewBag.PaymentMethod = method;
            return View(order);
        }


        // GET: Order/OrderHistory - Lịch sử đơn hàng của khách hàng
        [HttpGet]
        public ActionResult OrderHistory()
        {
            // Kiểm tra đăng nhập
            if (Session["CustomerId"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử mua hàng.";
                return RedirectToAction("Login", "Account");
            }

            int customerId = (int)Session["CustomerId"];

            // Lấy danh sách các đơn hàng của khách hàng này, sắp xếp đơn mới nhất lên đầu
            var orders = db.Orders.Where(o => o.MaKH == customerId)
                                  .OrderByDescending(o => o.NgayDat)
                                  .ToList();

            return View(orders);
        }



    }
}