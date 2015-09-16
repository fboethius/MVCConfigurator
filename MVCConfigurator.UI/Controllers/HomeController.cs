﻿using MVCConfigurator.Domain.Models;
using MVCConfigurator.Domain.Services;
using MVCConfigurator.UI.Models;
using MVCConfigurator.UI.Security;
using MVCConfigurator.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace MVCConfigurator.UI.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOrderService _orderService;

        private static User CurrentUser;

        public HomeController(IUserService userService, IProductService productService, IAuthenticationService authenticationService, IOrderService orderService)
        {
            _userService = userService;
            _productService = productService;
            _authenticationService = authenticationService;
            _orderService = orderService;
        }
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(UserViewModel viewModel)
        {
            var login = _userService.Login(viewModel.Username, viewModel.Password);

            if (login.Success)
            {
                _authenticationService.LoginUser(login.Entity, HttpContext, false);

                CurrentUser = HttpContext.User as User;

                if (login.Entity.IsAdmin)
                {
                    return RedirectToAction("Admin");
                }

                return RedirectToAction("User");
            }

            ModelState.AddModelError("username", login.Error.ToString());
            return View();
        }

        #region Admin
        [Authorize]
        public ActionResult CreateProduct()
        {
            var viewModel = new ProductViewModel();
            viewModel.Categories = _productService.GetAllProductCategories().Select(c => new SelectListItem() { Value = c.Id.ToString(), Text = c.Name });

            return View("~/Views/Admin/CreateProduct.cshtml", viewModel);
        }

        [HttpPost]
        public ActionResult CreateProduct(ProductViewModel model)
        {
            var categories = _productService.GetAllProductCategories();

            if (categories.Any(c => c.Name == model.Product.Category))
            {
                return View("~/Views/Admin/CreateProduct.cshtml");
            }

            var product = new Product()
            {
                Name = model.Product.Category,
                Category = new ProductCategory { Name = model.Product.Category },
                ImagePath = model.Product.ImagePath
            };

            product = _productService.AddProduct(product);

            return RedirectToAction("ProductPartList", new { id = product.Id });
        }
        public ActionResult AddPart(int id)
        {
            var product = _productService.GetProduct(id);

            var viewModel = new PartViewModel()
            {
                Categories = _productService.GetAllPartCategoriesByProduct(product).Select(c => new SelectListItem() { Value = c.Id.ToString(), Text = c.Name }),
                ExistingParts = product.Parts.Select(p => new PartModel { Id = p.Id, Name = p.Name, Category = p.Category.Name, LeadTime = p.LeadTime, ImagePath = p.ImagePath, Price = p.Price, StockKeepingUnit = p.StockKeepingUnit }).ToList(),
                ProductId = id
            };

            return View(viewModel);
        }

        public ActionResult DeleteProduct(int id)
        {
            var product = _productService.GetProduct(id);

            _productService.DeleteProduct(product);

            return RedirectToAction("ProductList");
        }

        [HttpPost]
        public ActionResult AddPart(PartViewModel model)
        {
            var product = _productService.GetProduct(model.ProductId);

            var partCategory = product.Parts.FirstOrDefault(p => p.Category.Id == model.PartDetails.CategoryId);

            var incompatibleParts = new List<Part>();

            if (model.ExistingParts != null && model.ExistingParts.Count > 0)
            {
                foreach (var item in model.ExistingParts)
                {
                    if (item.IsIncompatible)
                    {
                        incompatibleParts.Add(product.Parts.First(p => p.Id == item.Id));
                    }
                }
            }

            var part = new Part()
            {
                Category = partCategory.Category ?? new PartCategory { Name = model.PartDetails.Category },
                ImagePath = model.PartDetails.ImagePath,
                LeadTime = model.PartDetails.LeadTime,
                Name = model.PartDetails.Name,
                Price = model.PartDetails.Price,
                StockKeepingUnit = model.PartDetails.StockKeepingUnit,
                IncompatibleParts = incompatibleParts
            };

            product.Parts.Add(part);
            _productService.UpdateProduct(product);

            return RedirectToAction("ProductPartList", new { id = product.Id });
        }
        public ActionResult ProductPartList(int id)
        {
            var product = _productService.GetProduct(id);
            var viewModel = new ProductViewModel
            {
                Product = new ProductModel
                {
                    Id = id,
                    Category = product.Name,
                    Parts = product.Parts,
                    ImagePath = product.ImagePath,
                    ProductCode = product.ProductCode
                },
            };
            return View(viewModel);
        }
        [CustomAuthAttribute]
        public ActionResult ProductDetails()
        {
            return View("~/Views/Admin/ProductDetails.cshtml");
        }

        [CustomAuthAttribute]
        public ActionResult CustomerList()
        {
            var model = new CustomerListViewModel()
            {
                Users = _userService.GetAllUsers().Where(u => u.IsAdmin == true).ToList()
            };
            return View("~/Views/Admin/CustomerList.cshtml", model);
        }
        #endregion

        #region Customer
        [Authorize]
        public ActionResult CustomerDetails(string userName)
        {
            var customer = _userService.Get(userName);
            var orders = _orderService.GetOrdersByCustomer(customer.Entity.Id);
            var model = new CustomerDetailsViewModel()
            {
                Orders = orders.Select(o => new OrderModel { Id = o.Id, DeliveryDate = o.DeliveryDate, IsReady = o.IsReady }).ToList(),
                Customer = customer.Entity
            };
            return View("~/Views/Admin/CustomerDetails.cshtml", model);
        }
        [HttpPost]
        public ActionResult CustomerDetails(CustomerDetailsViewModel model)
        {
            foreach (var item in model.Orders)
            {
                var order = _orderService.GetOrderById(item.Id);
                order.IsReady = item.IsReady;
                _orderService.UpdateOrder(order);
            }

            return RedirectToAction("CustomerList");
        }

        [Authorize]
        public ActionResult SelectParts()
        {
            return View("~/Views/User/SelectParts.cshtml");
        }

        [Authorize]
        public ActionResult ConfirmOrder()
        {
            return View("~/Views/User/ConfirmOrder.cshtml");
        }

        [Authorize]
        public ActionResult OrderList()
        {
            return View("~/Views/User/OrderList.cshtml");
        }

        [Authorize]
        public ActionResult Profile()
        {
            return View("~/Views/User/Profile.cshtml");
        }

        #endregion

        public ActionResult ProductList()
        {
            if (CurrentUser.IsAdmin)
            {
                var viewModel = new ProductListViewModel
                {
                    Products = _productService.GetAllProducts()
                };

                return View("~/Views/Admin/AdminProductList.cshtml", viewModel);
            }

            return View("~/Views/User/ProductList.cshtml");
        }

        [CustomAuthAttribute]
        public ActionResult Admin()
        {
            return View();
        }

        public ActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(RegisterViewModel viewModel)
        {
            var userDetails = new UserDetails()
            {
                FirstName = viewModel.UserDetails.FirstName,
                LastName = viewModel.UserDetails.LastName,
                Company = viewModel.UserDetails.Company,
                Address = viewModel.UserDetails.Address,
                Phone = viewModel.UserDetails.Phone
            };

            var response = _userService.RegisterUser(viewModel.Username, viewModel.Password, viewModel.ConfirmPassword, userDetails);

            if (response.Success)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("username", response.Error.ToString());

            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(UserViewModel user)
        {
            _authenticationService.ResetPassword(user.Username);

            return RedirectToAction("Index");
        }

        public ActionResult CreateNewPassword(string token)
        {
            return View();
        }

        public ActionResult LogOut()
        {
            _authenticationService.LogOut();

            return RedirectToAction("Index");
        }

        protected override void OnAuthentication(AuthenticationContext filterContext)
        {
            if (filterContext.HttpContext.User != null && filterContext.HttpContext.User.Identity.AuthenticationType == AuthenticationMode.Forms.ToString())
            {
                _authenticationService.AuthenticateRequest(filterContext.HttpContext);
            }
        }
    }
}