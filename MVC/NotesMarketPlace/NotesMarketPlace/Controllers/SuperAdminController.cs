﻿using System.Linq;
using System.Web.Mvc;
using NotesMarketPlace.DB;
using NotesMarketPlace.Models.Admin;
using System.Web.Routing;
using System.Web.Security;
using System;
using System.Collections.Generic;
using NotesMarketPlace.Models;

namespace NotesMarketPlace.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminController : Controller
    {
        #region Default Constructor
        public SuperAdminController()
        {
            using (var context = new NotesMarketPlaceEntities())
            {
                // set social URL
                var socialUrl = context.SystemConfigurations.Where(m => m.Key == "Facebook" || m.Key == "Twitter" || m.Key == "Linkedin").ToList();
                ViewBag.URLs = socialUrl;
            }
        }
        #endregion Default Constructor

        #region Initialize User Information

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (requestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                using (var context = new NotesMarketPlaceEntities())
                {
                    // get current user
                    var currentUser = context.Users.FirstOrDefault(m => m.EmailID == User.Identity.Name);

                    //current user profile image
                    var img = (from Details in context.UserProfile
                               join Users in context.Users on Details.UserID equals Users.UserID
                               where Users.EmailID == requestContext.HttpContext.User.Identity.Name
                               select Details.ProfilePicture).FirstOrDefault();

                    string fileName = System.IO.Path.GetFileName(img);

                    string filePath = "Members/" + currentUser.UserID + "/" + fileName;

                    if (img == null)
                    {
                        // set default image
                        var defaultImg = context.SystemConfigurations.FirstOrDefault(m => m.Key == "DefaultProfileImage").Value;
                        ViewBag.UserProfile = defaultImg;
                    }
                    else
                    {
                        ViewBag.UserProfile = filePath;
                    }
                }
            }
        }

        #endregion Initialize User Information

        #region Manage Administrator

        public ActionResult ManageAdministrator(string txtSearch, string SortOrder, string SortBy, int PageNumber = 1)
        {
            using (var context = new NotesMarketPlaceEntities())
            {
                var model = (from User in context.Users
                             where User.RoleID == 2
                             join Profile in context.UserProfile on User.UserID equals Profile.UserID
                             select new ManageAdministratorModel
                             {
                                 Id = User.UserID,
                                 FirstName = User.FirstName,
                                 LastName = User.LastName,
                                 Email = User.EmailID,
                                 Phone = Profile.PhoneNumber,
                                 Active = User.IsActive == true ? "Yes" : "No",
                                 DateAdded = User.CreatedDate
                             }).OrderByDescending(x => x.DateAdded).ToList();

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                var ManageAdministratorresult = model;

                if (txtSearch != null)
                {
                    if (ManageAdministratorresult.Any(x => x.FirstName.ToLower().Contains(txtSearch.ToLower())))
                    {
                        ManageAdministratorresult = ManageAdministratorresult.Where(x => x.FirstName.ToLower().Contains(txtSearch.ToLower())).ToList();
                    }
                    else if (ManageAdministratorresult.Any(x => x.LastName.ToLower().Contains(txtSearch.ToLower())))
                    {
                        ManageAdministratorresult = ManageAdministratorresult.Where(x => x.LastName.ToLower().Contains(txtSearch.ToLower())).ToList();
                    }
                    else if (ManageAdministratorresult.Any(x => x.Email.ToLower().Contains(txtSearch.ToLower())))
                    {
                        ManageAdministratorresult = ManageAdministratorresult.Where(x => x.Email.ToLower().Contains(txtSearch.ToLower())).ToList();
                    }
                    else
                    {
                        ManageAdministratorresult = ManageAdministratorresult.Where(x => x.FirstName.ToLower().Contains(txtSearch.ToLower())).ToList();
                    }

                    //Sorting
                    ManageAdministratorresult = ApplySorting(SortOrder, SortBy, ManageAdministratorresult);

                    //Pagination
                    ManageAdministratorresult = ApplyPagination(ManageAdministratorresult, PageNumber);
                }
                else
                {
                    //Sorting
                    ManageAdministratorresult = ApplySorting(SortOrder, SortBy, ManageAdministratorresult);

                    //Pagination
                    ManageAdministratorresult = ApplyPagination(ManageAdministratorresult, PageNumber);
                }

                return View(ManageAdministratorresult);
            }
        }

        #endregion Manage Administrator

        #region Add Administrator

        public ActionResult AddAdministrator(int? id)
        {
            using (var context = new NotesMarketPlaceEntities())
            {
                // Get Country List
                var country = context.Countries.ToList();

                var CountryCodeList = country;
                ViewBag.CountryCodeList = new SelectList(CountryCodeList, "CountriesID", "CountryCode");

                if (!id.Equals(null))
                {
                    var model = (from User in context.Users
                                 where User.UserID == id && User.IsActive == true
                                 join Detail in context.UserProfile on User.UserID equals Detail.UserID
                                 select new AddAdministratorModel
                                 {
                                     Id = User.UserID,
                                     FirstName = User.FirstName,
                                     LastName = User.LastName,
                                     Email = User.EmailID,
                                     CountryCode = Detail.PhoneNumberCountryCode,
                                     Phone = Detail.PhoneNumber
                                 }).Single();

                    var countryCode = context.Countries.Where(m => m.IsActive == true).ToList();

                    ViewBag.PhoneCode = countryCode;
                    ViewBag.Edit = true;

                    return View(model);
                }
                else
                {
                    var countryCode = context.Countries.Where(m => m.IsActive == true).ToList();
                    ViewBag.PhoneCode = countryCode;
                    AddAdministratorModel model = new AddAdministratorModel();

                    ViewBag.Edit = false;
                    return View();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAdministrator(int? id, AddAdministratorModel model)
        {
            if (!ModelState.IsValid)
            {
                if (id.Equals(null))
                {
                    return RedirectToAction("AddAdministrator");
                }
                else
                {
                    return RedirectToAction("AddAdministrator", id);
                }
            }

            using (var context = new NotesMarketPlaceEntities())
            {
                var CurrentUser = context.Users.Single(m => m.EmailID == User.Identity.Name).UserID;

                // Get Country List
                var country = context.Countries.ToList();

                var CountryCodeList = country;
                ViewBag.CountryCodeList = new SelectList(CountryCodeList, "CountriesID", "CountryCode");

                //for edit details
                if (!id.Equals(null))
                {
                    var data = context.Users.Single(m => m.UserID == id);
                    var details = context.UserProfile.Single(m => m.UserID == id);

                    data.FirstName = model.FirstName;
                    data.LastName = model.LastName;
                    data.EmailID = model.Email;
                    details.PhoneNumberCountryCode = model.CountryCode;
                    details.PhoneNumber = model.Phone;

                    data.ModifiedBy = CurrentUser;
                    data.ModifiedDate = DateTime.Now;
                    details.ModifiedDate = DateTime.Now;

                    context.SaveChanges();

                    return RedirectToAction("ManageAdministrator");
                }
                //add new Admin
                else
                {
                    var create = context.Users;
                    create.Add(new Users
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        EmailID = model.Email,
                        RoleID = 2,
                        Password = "",
                        CreatedBy = CurrentUser,
                        CreatedDate = DateTime.Now,
                        ModifiedBy = CurrentUser,
                        ModifiedDate = DateTime.Now
                    });

                    context.SaveChanges();

                    var newAdmin = context.Users.Single(m => m.EmailID == model.Email);

                    var details = context.UserProfile;
                    details.Add(new UserProfile
                    {
                        PhoneNumberCountryCode = model.CountryCode,
                        PhoneNumber = model.Phone,
                        AddressLine1 = "",
                        City = "",
                        State = "",
                        ZipCode = "",
                        Country = ""
                    });

                    context.SaveChanges();

                    return RedirectToAction("ManageAdministrator");
                }
            }
        }

        #endregion Add Administrator

        #region Delete Administrator

        public void DeleteAdministrator(int id)
        {
            using (var _Context = new NotesMarketPlaceEntities())
            {
                var CurrentAdmin = _Context.Users.Single(m => m.EmailID == User.Identity.Name).UserID;

                var Admin = _Context.Users.Single(m => m.UserID == id);
                Admin.IsActive = false;
                Admin.ModifiedBy = CurrentAdmin;
                Admin.ModifiedDate = DateTime.Now;

                _Context.SaveChanges();
            }
        }

        #endregion Delete Administrator

        #region Apply Sorting

        public List<ManageAdministratorModel> ApplySorting(string SortOrder, string SortBy, List<ManageAdministratorModel> result)
        {
            switch (SortBy)
            {
                case "FirstName":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    result = result.OrderBy(x => x.FirstName).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    result = result.OrderByDescending(x => x.FirstName).ToList();
                                    break;
                                }
                            default:
                                {
                                    result = result.OrderBy(x => x.FirstName).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "LastName":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    result = result.OrderBy(x => x.LastName).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    result = result.OrderByDescending(x => x.LastName).ToList();
                                    break;
                                }
                            default:
                                {
                                    result = result.OrderBy(x => x.LastName).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "Email":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    result = result.OrderBy(x => x.Email).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    result = result.OrderByDescending(x => x.Email).ToList();
                                    break;
                                }
                            default:
                                {
                                    result = result.OrderBy(x => x.Email).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    result = result.OrderByDescending(x => x.DateAdded).ToList();
                    break;
            }
            return result;
        }

        #endregion Apply Sorting

        #region Apply Pagination

        public List<ManageAdministratorModel> ApplyPagination(List<ManageAdministratorModel> result, int PageNumber)
        {
            ViewBag.TotalPages = Math.Ceiling(result.Count() / 5.0);
            ViewBag.PageNumber = PageNumber;

            result = result.Skip((PageNumber - 1) * 5).Take(5).ToList();

            return result;
        }

        #endregion Apply Pagination

        public ActionResult LogOut()
        {
            //if (Session["emailID"] != null)
            //{
            //    Session.Abandon();
            //    return RedirectToAction("Login", "Account");
            //}
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
    }
}