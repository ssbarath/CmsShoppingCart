﻿using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //Declare List Models
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {

                //Intialize the List
                categoryVMList = db.Categories.ToArray().
                                    OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            //Return View with List
            return View(categoryVMList);
        }
    }
}