﻿using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: /account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //Confirm User Login

            string UserName = User.Identity.Name;

            if (!string.IsNullOrEmpty(UserName))
                return RedirectToAction("user-profile");

            //Return View
            return View();
        }

        // POST: /account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //Check Model state
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            //check User is valid

            bool isValid = false;

            using (Db db = new Db())
            {
                if(db.Users.Any(x => x.Username.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if(!isValid)
            {
                ModelState.AddModelError("", "Invalid User name or Password");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }

            
        }

        // GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }

        // POST: /account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Check Model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            //check password Matches
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password entered mismatch!");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //Make sure username is unique
                if(db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "User name already taken!");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //Create User DTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Password = model.Password,
                    Username = model.Username
                };

                //Add the DTO
                db.Users.Add(userDTO);

                //Save
                db.SaveChanges();

                //Add UserRole DTO
                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();
            }
            //Create a temp data message
            TempData["SM"] = "You are now registered and can login.";

            //Redirect
            return Redirect("~/account/login");
        }

        // GET: /account/loguout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        public ActionResult UserNavPartial()
        {
            //Get the Username
            string userName = User.Identity.Name;

            //Declare the model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            //return the partial view with model
            return PartialView(model);
        }

        //GET: /account/user-profile
        [ActionName("user-profile")]
        [HttpGet]
        public ActionResult UserProfile()
        {
            //Get username
            string userName = User.Identity.Name;

            //Declare Model
            UserProfileVM model;

            using (Db db = new Db())
            {
                //Get User
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //Build model
                model = new UserProfileVM(dto);
            }
            //Return View with model
            return View("UserProfile", model);
        }

        //POST: /account/user-profile
        [ActionName("user-profile")]
        [HttpPost]
        public ActionResult UserProfile(UserProfileVM model)
        {
            //Check model state
            if(!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //Check if password match if needed
            if(!string.IsNullOrWhiteSpace(model.Password))
            {
                if(!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Password does not match!");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //Get username
                string userName = User.Identity.Name;

                //Make sure username is unique
                if(db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
                {
                    ModelState.AddModelError("", "Username" + model.Username + "already exsists!");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                //Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //save
                db.SaveChanges();
            }

            //set tempdata message
            TempData["SM"] = "The profile has been updated successfully";

            //redirect
            return Redirect("~/account/user-profile");
        }
    }
}       