using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Web_Ban_Sach.Models;

namespace Web_Ban_Sach.Controllers
{
    public class CartController : Controller
    {

        private DBWebBanSachEntities db = new DBWebBanSachEntities();
        // GET: Cart
        // Hàm phụ: Lấy giỏ hàng hiện tại hoặc tạo mới nếu chưa có
        private Cart GetOrCreateCart()
        {
            Cart cart = null;

            // 1. Kiểm tra session giỏ hàng đã có chưa
            if (Session["CartId"] != null)
            {
                int cartId = (int)Session["CartId"];
                cart = db.Carts.Find(cartId);
            }

            // 2. Nếu chưa có giỏ hàng trong session hoặc không tìm thấy trong DB -> Tạo mới
            if (cart == null)
            {
                cart = new Cart
                {
                    NgayTao = DateTime.Now,
                    MaKH = Session["CustomerId"] as int? // Gán nếu khách đã đăng nhập
                };
                db.Carts.Add(cart);
                db.SaveChanges();

                Session["CartId"] = cart.MaGioHang;
            }

            return cart;
        }

        private void UpdateCartCount(int cartId)
        {
            int totalCount = db.CartItems.Where(c => c.MaGioHang == cartId).Sum(c => (int?)c.SoLuong) ?? 0;
            Session["CartCount"] = totalCount;
        }

        // 1. Xử lý nút "Mua ngay" ở Trang chủ (Gửi bằng GET)
        [HttpGet]
        public ActionResult AddToCart(int id)
        {
            return ProcessAddToCart(id, 1);
        }

        // 2. Xử lý nút "Thêm vào giỏ hàng" ở trang Details (Gửi bằng POST kèm số lượng)
        [HttpPost]
        public ActionResult AddToCart(int id, int? quantity)
        {
            int qty = (quantity.HasValue && quantity.Value > 0) ? quantity.Value : 1;
            return ProcessAddToCart(id, qty);
        }

        // Hàm xử lý chung thêm sách hoặc cộng dồn số lượng
        private ActionResult ProcessAddToCart(int bookId, int qty)
        {
            var book = db.Books.Find(bookId);
            if (book == null)
            {
                return HttpNotFound();
            }

            Cart cart = GetOrCreateCart();

            // Tìm cuốn sách này đã có trong giỏ chưa
            var cartItem = db.CartItems.FirstOrDefault(c => c.MaGioHang == cart.MaGioHang && c.MaSach == bookId);

            if (cartItem != null)
            {
                // Nếu đã có -> Cộng dồn số lượng
                cartItem.SoLuong += qty;
            }
            else
            {
                // Nếu chưa có -> Tạo dòng mới trong bảng CartItem
                cartItem = new CartItem
                {
                    MaGioHang = cart.MaGioHang,
                    MaSach = bookId,
                    SoLuong = qty
                };
                db.CartItems.Add(cartItem);
            }

            db.SaveChanges();
            UpdateCartCount(cart.MaGioHang);

            // Chuyển hướng người dùng thẳng đến trang xem giỏ hàng
            return RedirectToAction("Index");
        }

        // Trang hiển thị toàn bộ giỏ hàng
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["CartId"] == null)
            {
                return View(new System.Collections.Generic.List<CartItem>());
            }

            int cartId = (int)Session["CartId"];
            var items = db.CartItems.Include(c => c.Book)
                                    .Where(c => c.MaGioHang == cartId)
                                    .ToList();

            UpdateCartCount(cartId);
            return View(items);
        }

        [HttpGet]
        public ActionResult RemoveFromCart(int id)
        {
            if (Session["CartId"] != null)
            {
                int cartId = (int)Session["CartId"];
                var item = db.CartItems.FirstOrDefault(c => c.MaGioHang == cartId && c.MaSach == id);
                if (item != null)
                {
                    db.CartItems.Remove(item);
                    db.SaveChanges();
                    UpdateCartCount(cartId);
                }
            }
            return RedirectToAction("Index");
        }



    }
}