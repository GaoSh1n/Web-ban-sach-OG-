using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Ban_Sach.Models;

namespace Web_Ban_Sach.Controllers
{
    public class AccountController : Controller
    {

        private DBWebBanSachEntities db = new DBWebBanSachEntities();

        // GET: Account

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Customer customer, string NhapLaiMatKhau)
        {

            // Kiểm tra mật khẩu xác nhận
            if (customer.MatKhau != NhapLaiMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View(customer);
            }

            var checkUsername = db.Customers.FirstOrDefault(c => c.TenTaiKhoan == customer.TenTaiKhoan);
            if(checkUsername != null)
            {
                ViewBag.Error = "Tên tài khoản đã tồn tại. Vui lòng chọn tên khác.";
                return View(customer);
            }

            db.Customers.Add(customer);
            db.SaveChanges();
            // Đăng ký thành công -> chuyển sang trang Đăng nhập
            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");

        }





        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(string TaiKhoan, string MatKhau)
        {
            if (string.IsNullOrEmpty(TaiKhoan) || string.IsNullOrEmpty(MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tên tài khoản và mật khẩu để đăng nhập.";
                return View();
            }

            var customer = db.Customers.FirstOrDefault(c => c.TenTaiKhoan == TaiKhoan && c.MatKhau == MatKhau);
            if(customer != null)
            {
                Session["Customer"] = customer;
                Session["CustomerId"] = customer.MaKH;
                Session["CustomerName"] = customer.HoTen;

                if (Session["CartId"] != null) // Nếu trước đó đang có giỏ hàng (Cart) chưa gán MaKH thì cập nhật vào
                {
                    int cartId = (int)Session["CartId"];
                    var cart = db.Carts.Find(cartId);
                    if (cart != null && cart.MaKH == null)
                    {
                        cart.MaKH = customer.MaKH;
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác.";
            return View();


        }


        [HttpGet]
        public ActionResult Logout()
        {

            // Xóa thông tin đăng nhập khỏi Session
            Session["Customer"] = null;
            Session["CustomerId"] = null;
            Session["CustomerName"] = null;
            return RedirectToAction("Index", "Home");
        }
    }
}