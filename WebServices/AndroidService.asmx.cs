﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using WebServices.Entity;
using WebServices.Classes;
using System.Transactions;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;

namespace WebServices
{

    [WebService(Namespace = "http://graduationprojectwebservice.azurewebsites.net/")]
    //[WebService(Namespace = "http://graduationprojectandroidservice.somee.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class AndroidService : System.Web.Services.WebService
    {
        [WebMethod]
        public Result<APPUSER> DoLoginAndReturnUserInfo(string UsernameOrMail_, string Password_)
        {
            Result<APPUSER> result_ = new Result<APPUSER>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    APPUSER userInfo_ = null;

                    if (isEmailValid(UsernameOrMail_))
                        userInfo_ = ent_.APPUSER.Where(x => x.USER_EMAIL.Equals(UsernameOrMail_, StringComparison.InvariantCultureIgnoreCase) && x.USER_PASSWORD.Equals(Password_)).FirstOrDefault();
                    else
                        userInfo_ = ent_.APPUSER.Where(x => x.USER_CODE.Equals(UsernameOrMail_, StringComparison.InvariantCultureIgnoreCase) && x.USER_PASSWORD.Equals(Password_)).FirstOrDefault();

                    if (userInfo_ != null)
                    {
                        result_.Data = userInfo_;
                        result_.Success = true;
                    }
                    else
                    {
                        result_.Message = "Girilen Kullanıcı Bilgileri Hatalı!";
                        result_.Success = false;
                    }
                }
            }
            catch (Exception Ex_)
            {
                result_.Message = Ex_.GetBaseException().ToString();
                result_.Success = false;
            }

            return result_;
        }

        [WebMethod]
        public Result<APPUSER> DoInsertAndUpdateUser(bool IsNewUser_, string Username_, string NameSurname_, string Email_, string Phone_, string Password_)
        {
            Result<APPUSER> result_ = new Result<APPUSER>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    using (TransactionScope scope_ = new TransactionScope())
                    {
                        APPUSER userInfo_ = ent_.APPUSER.Where(x => x.USER_CODE.Equals(Username_, StringComparison.InvariantCultureIgnoreCase) || x.USER_EMAIL.Equals(Email_, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (userInfo_ == null && IsNewUser_)
                        {
                            userInfo_ = new APPUSER();
                            userInfo_.USER_CODE = Username_;
                            userInfo_.USER_NAMESURNAME = NameSurname_;
                            userInfo_.USER_EMAIL = Email_;
                            userInfo_.USER_PHONE = Phone_;
                            userInfo_.USER_PASSWORD = Password_;
                            userInfo_.USER_SIGNUPDATE = DateTime.Now;
                            ent_.AddToAPPUSER(userInfo_);
                            ent_.SaveChanges();
                            scope_.Complete();
                            ent_.AcceptAllChanges();

                            result_.Data = userInfo_;
                            result_.Message = "Üyelik Kaydı Başarılı!";
                            result_.Success = true;
                        }
                        else if (userInfo_ != null && !IsNewUser_)
                        {
                            userInfo_.USER_NAMESURNAME = NameSurname_;
                            userInfo_.USER_EMAIL = Email_;
                            userInfo_.USER_PHONE = Phone_;
                            userInfo_.USER_PASSWORD = Password_;
                            ent_.SaveChanges();
                            scope_.Complete();
                            ent_.AcceptAllChanges();

                            result_.Data = userInfo_;
                            result_.Message = "Üyelik Bilgileri Güncellendi!";
                            result_.Success = true;
                        }
                        else if (userInfo_ == null && !IsNewUser_)
                        {
                            result_.Message = "Girilen Bilgilerde Bir Kullanıcı Bulunamadı!";
                            result_.Success = false;
                        }
                        else
                        {
                            result_.Message = "Girilen Kullanıcı Adı ve ya Mail Adresi Sistemde Tanımlı!";
                            result_.Success = false;
                        }
                    }
                }
            }
            catch (Exception Ex_)
            {
                result_.Message = Ex_.GetBaseException().ToString();
                result_.Success = false;
            }

            return result_;
        }

        [WebMethod]
        public Result<bool> DoInsertAndUpdateAdvert(int AdvertID_, long AdvertMainTypeID_, string AdvertSubTypeDescription_, string AdvertDescription_, long UserID_, string Phone_, string Mail_, string Image_, int Price_, bool TR_)
        {
            Result<bool> result_ = new Result<bool>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    using (TransactionScope scope_ = new TransactionScope())
                    {
                        if (AdvertID_ <= 0)
                        {
                            ADVERT advert_ = new ADVERT();
                            string subDesc_ = "";
                            if (string.IsNullOrEmpty(AdvertSubTypeDescription_))
                                subDesc_ = Convert.ToString(AdvertMainTypeID_);
                            else
                                subDesc_ = AdvertSubTypeDescription_;

                            advert_.ADVERTMAINTYPE = ent_.ADVERTMAINTYPE.Where(c => c.ADTT_ID == AdvertMainTypeID_).FirstOrDefault();
                            if (TR_)
                                advert_.ADVERTSUBTYPE = ent_.ADVERTSUBTYPE.Where(c => c.ABST_DESCRIPTION.Equals(subDesc_)).FirstOrDefault();
                            if (!TR_)
                                advert_.ADVERTSUBTYPE = ent_.ADVERTSUBTYPE.Where(c => c.ABST_CODE.Equals(subDesc_)).FirstOrDefault();
                            if (UserID_ > 0)
                                advert_.APPUSER = ent_.APPUSER.Where(c => c.USER_ID == UserID_).FirstOrDefault();

                            advert_.ADVT_DESCRIPTION = AdvertDescription_;
                            advert_.ADVT_ISOPEN = true;
                            advert_.ADVT_MAIL = Mail_;
                            advert_.ADVT_PHONE = Phone_;
                            advert_.ADVT_ADVERTDATETIME = DateTime.Now;
                            advert_.ADVT_PRICE = Price_;

                            ent_.AddToADVERT(advert_);
                            ent_.SaveChanges();
                            scope_.Complete();
                            ent_.AcceptAllChanges();

                            result_.Message = "İlan Başarıyla Kaydedildi!";
                            result_.Success = true;

                            try
                            {
                                if (!string.IsNullOrEmpty(Image_))
                                    createImageAndReturnURL(Image_, advert_.ADVT_ID);
                            }
                            catch (Exception ex_)
                            {
                                result_.Message += "-----" + ex_.ToString();
                            }
                        }
                        else
                        {
                            ADVERT advertInfo_ = ent_.ADVERT.Where(x => x.ADVT_ID == (AdvertID_)).FirstOrDefault();
                            if (advertInfo_ != null)
                            {

                                ent_.SaveChanges();
                                scope_.Complete();
                                ent_.AcceptAllChanges();

                                result_.Message = "İlan Bilgileri Güncellendi!";
                                result_.Success = true;
                            }
                            else
                            {
                                result_.Message = "İlan Bilgileri Güncellenemedi!";
                                result_.Success = false;
                            }
                        }
                    }
                }
            }
            catch (Exception Ex_)
            {
                result_.Message = Ex_.GetBaseException().ToString();
                result_.Success = false;
            }

            return result_;
        }

        [WebMethod]
        public Result<List<APPUSER>> GetUserList()
        {
            Result<List<APPUSER>> result_ = new Result<List<APPUSER>>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    result_.Data = ent_.APPUSER.ToList();
                    if (result_.Data.Count > 0)
                        result_.Success = true;
                    else
                    {
                        result_.Message = "Kullanıcı Bulunamadı!";
                        result_.Success = false;
                    }
                }
            }
            catch (Exception Ex_)
            {
                result_.Message = Ex_.GetBaseException().ToString();
                result_.Success = false;
            }

            return result_;
        }

        [WebMethod]
        public Result<List<Advert>> GetUserAdvertList(long UserID_)
        {
            Result<List<Advert>> result_ = new Result<List<Advert>>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {

                    var linqSelect = (from advert in ent_.ADVERT
                                      join maintype in ent_.ADVERTMAINTYPE on advert.ADVERTMAINTYPE equals maintype
                                      join subtype in ent_.ADVERTSUBTYPE on advert.ADVERTSUBTYPE equals subtype
                                      join user in ent_.APPUSER on advert.APPUSER equals user
                                      where user.USER_ID == UserID_
                                      select new Advert
                                      {
                                          MainCategoryCode = maintype.ADTT_CODE,
                                          SubCategoryCode = subtype.ABST_CODE,
                                          Description = advert.ADVT_DESCRIPTION,
                                          Datetime = advert.ADVT_ADVERTDATETIME,
                                          Price = advert.ADVT_PRICE,
                                          Phone = advert.ADVT_PHONE,
                                          Mail = advert.ADVT_MAIL,
                                          ID = advert.ADVT_ID
                                      }).ToList();

                    if (linqSelect != null && linqSelect.Count > 0)
                    {
                        foreach (Advert advert_ in linqSelect)
                        {
                            if (!string.IsNullOrEmpty(advert_.SubCategoryCode) && !isNumeric(advert_.SubCategoryCode))
                                advert_.MainCategoryCode += " - " + advert_.SubCategoryCode;
                            if (File.Exists(VirtualPathUtility.ToAbsolute("~/" + advert_.ID + ".png")))
                            {
                                string path_ = VirtualPathUtility.ToAbsolute("~/" + advert_.ID + ".png");
                                int len1 = path_.IndexOf('w');
                                int len2 = path_.Length;
                                advert_.ImageLink = path_.Substring(len1, len2 - len1);
                            }
                            else
                                advert_.ImageLink = "-";
                        }
                        result_.Data = linqSelect;
                        result_.Success = true;
                    }
                    else
                    {
                        result_.Message = "İlan Bulunamadı!";
                        result_.Success = false;
                    }
                }
            }
            catch (Exception Ex_)
            {
                result_.Message = Ex_.GetBaseException().ToString();
                result_.Success = false;
            }

            return result_;
        }

        [WebMethod]
        public Result<List<Advert>> GetAdvertList(long AdvertMainTypeID_)
        {
            Result<List<Advert>> result_ = new Result<List<Advert>>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    var linqSelect = (from advert in ent_.ADVERT
                                      join maintype in ent_.ADVERTMAINTYPE on advert.ADVERTMAINTYPE equals maintype
                                      join subtype in ent_.ADVERTSUBTYPE on advert.ADVERTSUBTYPE equals subtype
                                      join user in ent_.APPUSER on advert.APPUSER equals user
                                      where maintype.ADTT_ID == AdvertMainTypeID_ 
                                      select new Advert
                                      {
                                          MainCategoryCode = maintype.ADTT_CODE,
                                          SubCategoryCode = subtype.ABST_CODE,
                                          Description = advert.ADVT_DESCRIPTION,
                                          Datetime = advert.ADVT_ADVERTDATETIME,
                                          Price = advert.ADVT_PRICE,
                                          Phone = advert.ADVT_PHONE,
                                          Mail = advert.ADVT_MAIL,
                                          ID = advert.ADVT_ID
                                      }).ToList();

                    if (linqSelect != null && linqSelect.Count > 0)
                    {
                        foreach (Advert advert_ in linqSelect)
                        {
                            if (!string.IsNullOrEmpty(advert_.SubCategoryCode) && !isNumeric(advert_.SubCategoryCode))
                                advert_.MainCategoryCode += " - " + advert_.SubCategoryCode;
                            if (File.Exists(VirtualPathUtility.ToAbsolute("~/" + advert_.ID + ".png")))
                            {
                                string path_ = VirtualPathUtility.ToAbsolute("~/" + advert_.ID + ".png");
                                int len1 = path_.IndexOf('w');
                                int len2 = path_.Length;
                                advert_.ImageLink = path_.Substring(len1, len2 - len1);
                            }
                            else
                                advert_.ImageLink = "-";
                        }
                        result_.Data = linqSelect;
                        result_.Success = true;
                    }
                    else
                    {
                        result_.Message = "İlan Bulunamadı!";
                        result_.Success = false;
                    }
                }
            }
            catch (Exception Ex_)
            {
                result_.Message = Ex_.GetBaseException().ToString();
                result_.Success = false;
            }

            return result_;
        }


        [WebMethod]
        public Result<List<Advert>> GetTop15AdvertList()
        {
            Result<List<Advert>> result_ = new Result<List<Advert>>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    var linqSelect = (from advert in ent_.ADVERT
                                      join maintype in ent_.ADVERTMAINTYPE on advert.ADVERTMAINTYPE equals maintype
                                      join subtype in ent_.ADVERTSUBTYPE on advert.ADVERTSUBTYPE equals subtype
                                      join user in ent_.APPUSER on advert.APPUSER equals user
                                      select new Advert
                                      {
                                          MainCategoryCode = maintype.ADTT_CODE,
                                          SubCategoryCode = subtype.ABST_CODE,
                                          Description = advert.ADVT_DESCRIPTION,
                                          Datetime = advert.ADVT_ADVERTDATETIME,
                                          Price = advert.ADVT_PRICE,
                                          Phone = advert.ADVT_PHONE,
                                          Mail = advert.ADVT_MAIL,
                                          ID = advert.ADVT_ID
                                      }).OrderByDescending(x => x.Datetime).Take(15).ToList();

                    if (linqSelect!= null && linqSelect.Count > 0)
                    {
                        foreach (Advert advert_ in linqSelect.Take(15))
                        {
                            if (!string.IsNullOrEmpty(advert_.SubCategoryCode) && !isNumeric(advert_.SubCategoryCode))
                                advert_.MainCategoryCode += " - " + advert_.SubCategoryCode;
                            if (File.Exists(VirtualPathUtility.ToAbsolute("~/" + advert_.ID + ".png")))
                            {
                                string path_ = VirtualPathUtility.ToAbsolute("~/" + advert_.ID + ".png");
                                //int len1 = path_.IndexOf('w');
                                //int len2 = path_.Length;
                                advert_.ImageLink = path_;
                            }
                            else
                                advert_.ImageLink = "-";
                        }
                        result_.Data = linqSelect;
                        result_.Success = true;
                    }
                    else
                    {
                        result_.Message = "İlan Bulunamadı!";
                        result_.Success = false;
                    }
                }
            }
            catch (Exception Ex_)
            {
                result_.Message = Ex_.GetBaseException().ToString();
                result_.Success = false;
            }

            return result_;
        }

        public bool isEmailValid(string eMail_)
        {
            string eMailPattern_ = @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+.(com|org|net|edu|gov|mil|biz|info|mobi)(.[A-Z]{2})?$";
            Regex regex = new Regex(eMailPattern_, RegexOptions.IgnoreCase);
            return regex.IsMatch(eMail_);
        }

        public static bool isNumeric(string value)
        {
            double oReturn = 0;
            return double.TryParse(value, out oReturn);
        }

        [WebMethod]
        public string createImageAndReturnURL(string Image_, long AdvertID_)
        {
            //string productionImagePath_ = "~/Documents/RecordImages/" + AdvertID_;
            //if (!Directory.Exists(HttpContext.Current.Server.MapPath(productionImagePath_)))
            //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(productionImagePath_));
            //productionImagePath_ += "/" + AdvertID_ + ".png";
            //Image_ = Image_.Replace(@"\n", "");
            //byte[] productionImage_ = Convert.FromBase64String(Image_);
            //File.WriteAllBytes(HttpContext.Current.Server.MapPath(productionImagePath_), productionImage_);
            //return HttpContext.Current.Server.MapPath(productionImagePath_);
            Image_ = Image_.Replace(@"\n", "");
            byte[] productionImage_ = Convert.FromBase64String(Image_);
            File.WriteAllBytes(VirtualPathUtility.ToAbsolute("~/" + AdvertID_ + ".png"), productionImage_);
            return VirtualPathUtility.ToAbsolute("~/" + AdvertID_ + ".png");
        }

        [WebMethod]
        public string serverPath()
        {
            return VirtualPathUtility.ToAbsolute("~/");
            //return HttpContext.Current.Request.ApplicationPath;
           
        }
    }
}
