﻿using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;

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

        //POST: Admin/Shop/AddNewCategory

        [HttpPost]
        public string AddNewCategory(string catName)
        {

            //Declare id
            string id;

            using (Db db = new Db())
            {

                //Check the category name is unique
                if(db.Categories.Any(x=>x.Name == catName))
                {
                    return "titletaken";
                }

                //Initialize DTO
                CategoryDTO dto = new CategoryDTO();

                //Add to DTO
                dto.Name = catName;
                dto.Slug = catName.Trim().Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //save DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                //Get the id
                id = dto.Id.ToString();
            }

            //Return the id

            return id;
        }

        //POST: Admin/Shop/ReorderCategories

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {

                //set intial count
                int cout = 1;

                //Declare CatergoryDTO
                CategoryDTO dto;

                //set sort for each page
                foreach (var catid in id)
                {
                    dto = db.Categories.Find(catid);
                    dto.Sorting = cout;

                    db.SaveChanges();

                    cout++;
                }
            }
        }


        //GET : Admin/Shop/DeleteCategory/id

        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {

                //Get Category
                CategoryDTO dto = db.Categories.Find(id);

                //Remove Page
                db.Categories.Remove(dto);

                //Save
                db.SaveChanges();

            }

            //redirect
            return RedirectToAction("Categories");
        }

        //POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public string RenameCategory(string newCatName,int id)
        {
            using (Db db = new Db())
            {
                //check category name unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //get DTO
                CategoryDTO dto = db.Categories.Find(id);

                //Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Save
                db.SaveChanges();
            }

            //Return
            return "success";
        }

        //GET : Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct ()
        {
            //Initialize the Model
            ProductVM model = new ProductVM();

            using (Db db=new Db())
            {
                //Add Category to Model
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            //Return View with model

            return View(model);
        }

        //POST : Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Check model state
            if (! ModelState.IsValid)
            {
                using (Db db=new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //Make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "The Product Name is taken!");
                    return View(model);
                }
                
            }

            //Declare Product id
            int id;

            //Initislize and save product DTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Price = model.Price;
                product.Description = model.Description;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //get inserted id
                id = product.Id;
            }

            //set tempdata message
            TempData["SM"] = "You have added a Product!";

            #region Upload Image

            //Create directory
            var originalDirectory = new DirectoryInfo(string.Format("{0}Image\\Uploads", Server.MapPath(@"\")));

            
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Check if a file uploaded
            if (file != null && file.ContentLength > 0)
            {
                //get file extension
                string ext = file.ContentType.ToLower();

                //Verfiy extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The Image was not uploaded - wrong image format!");
                        return View(model);


                    }
                }

                //Initialize image name
                string imageName = file.FileName;

                //save image name
                using (Db db=new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //set original and thumb image path
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //save the original image
                file.SaveAs(path);

                //create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }



            #endregion

            //Redirect

            return RedirectToAction("AddProduct");
        }


        //GET : Admin/Shop/Products

        public ActionResult Products(int? page,int? catId)
        {
            //Declare a list of Product VM
            List<ProductVM> listOfProductVM;

            //Set Page Number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Initialize the List
                listOfProductVM = db.Products.ToArray().
                    Where(x => catId == null || catId == 0 || x.CategoryId == catId).
                    Select(x => new ProductVM(x)).ToList();

                //Populate the Category List
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Set Selected Category
                ViewBag.SelectedCat = catId.ToString();
            }
            //Set Pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

            //return view with list
            return View(listOfProductVM);
        }

    }
}