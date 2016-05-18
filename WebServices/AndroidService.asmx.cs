using System;
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
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class AndroidService : System.Web.Services.WebService
    {
        [WebMethod]
        public Result<APPUSER> DoLoginAndReturnUserInfo(string UsernameOrMail_, string Password_, bool TR)
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
                        if (TR)
                            result_.Message = "Girilen Kullanıcı Bilgileri Hatalı!";
                        else
                            result_.Message = "Wrong User Information!";
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
        public Result<APPUSER> DoInsertAndUpdateUser(bool IsNewUser_, string Username_, string NameSurname_, string Email_, string Phone_, string Password_, bool TR)
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
                            if (TR)
                                result_.Message = "Üyelik Kaydı Başarılı!";
                            else
                                result_.Message = "Registration Successful!";
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
                            if (TR)
                                result_.Message = "Üyelik Bilgileri Güncellendi!";
                            else
                                result_.Message = "User Informations Updated!";
                            result_.Success = true;
                        }
                        else if (userInfo_ == null && !IsNewUser_)
                        {
                            if (TR)
                                result_.Message = "Girilen Bilgilerde Bir Kullanıcı Bulunamadı!";
                            else
                                result_.Message = "User Not Found!";
                            result_.Success = false;
                        }
                        else
                        {
                            if (TR)
                                result_.Message = "Girilen Kullanıcı Adı ve ya Mail Adresi Sistemde Tanımlı!";
                            else
                                result_.Message = "This Username/Mail is Already Registered!";
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
        public Result<bool> DoInsertAndUpdateAdvert(int AdvertID_, long AdvertMainTypeID_, string AdvertSubTypeDescription_, string AdvertDescription_, long UserID_, string Phone_, string Mail_, string Image_, int Price_, bool TR)
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
                            if (TR)
                                advert_.ADVERTSUBTYPE = ent_.ADVERTSUBTYPE.Where(c => c.ABST_DESCRIPTION.Equals(subDesc_)).FirstOrDefault();
                            if (!TR)
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
        public Result<List<Advert>> GetUserAdvertList(long UserID_, bool TR)
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
                                          IsOpen = advert.ADVT_ISOPEN,
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
                            if (File.Exists(HttpContext.Current.Server.MapPath("" + advert_.ID + ".png")))
                                advert_.ImageLink = "http://graduationprojectwebservice.azurewebsites.net/" + advert_.ID + ".png";
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
        public Result<List<Advert>> GetAdvertList(long AdvertMainTypeID_, bool TR)
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
                                      where maintype.ADTT_ID == AdvertMainTypeID_ && advert.ADVT_ISOPEN == true
                                      select new Advert
                                      {
                                          IsOpen = advert.ADVT_ISOPEN,
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
                            if (File.Exists(HttpContext.Current.Server.MapPath("" + advert_.ID + ".png")))
                                advert_.ImageLink = "http://graduationprojectwebservice.azurewebsites.net/" + advert_.ID + ".png";
                            else
                                advert_.ImageLink = "-";
                        }
                        result_.Data = linqSelect;
                        result_.Success = true;
                    }
                    else
                    {
                        if (TR)
                            result_.Message = "İlan Bulunamadı!";
                        else
                            result_.Message = "There Is No Advert!";
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
        public Result<List<Advert>> GetTop15AdvertList(bool TR)
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
                                      where advert.ADVT_ISOPEN == true
                                      select new Advert
                                      {
                                          IsOpen = advert.ADVT_ISOPEN,
                                          MainCategoryCode = maintype.ADTT_CODE,
                                          SubCategoryCode = subtype.ABST_CODE,
                                          Description = advert.ADVT_DESCRIPTION,
                                          Datetime = advert.ADVT_ADVERTDATETIME,
                                          Price = advert.ADVT_PRICE,
                                          Phone = advert.ADVT_PHONE,
                                          Mail = advert.ADVT_MAIL,
                                          ID = advert.ADVT_ID
                                      }).OrderByDescending(x => x.Datetime).Take(15).ToList();

                    if (linqSelect != null && linqSelect.Count > 0)
                    {
                        foreach (Advert advert_ in linqSelect.Take(15))
                        {
                            if (!string.IsNullOrEmpty(advert_.SubCategoryCode) && !isNumeric(advert_.SubCategoryCode))
                                advert_.MainCategoryCode += " - " + advert_.SubCategoryCode;
                            if (File.Exists(HttpContext.Current.Server.MapPath("" + advert_.ID + ".png")))
                                advert_.ImageLink = "http://graduationprojectwebservice.azurewebsites.net/" + advert_.ID + ".png";
                            else
                                advert_.ImageLink = "-";
                        }
                        result_.Data = linqSelect;
                        result_.Success = true;
                    }
                    else
                    {
                        if (TR)
                            result_.Message = "İlan Bulunamadı!";
                        else
                            result_.Message = "There Is No Advert!";
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
        public Result<bool> DoUpdateAdvertStatus(int AdvertID_, bool IsOpen,bool TR)
        {
            Result<bool> result_ = new Result<bool>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    using (TransactionScope scope_ = new TransactionScope())
                    {
                        ADVERT advertInfo_ = ent_.ADVERT.Where(x => x.ADVT_ID == (AdvertID_)).FirstOrDefault();
                        if (advertInfo_ != null)
                        {
                            advertInfo_.ADVT_ISOPEN = IsOpen;

                            ent_.SaveChanges();
                            scope_.Complete();
                            ent_.AcceptAllChanges();
                            if (TR)
                                result_.Message = "İlan Bilgileri Güncellendi!";
                            else
                                result_.Message = "Advert Is Updated!";
                            result_.Success = true;
                        }
                        else
                        {
                            result_.Message = "Advert Couldn't Updated!";
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
            Image_ = Image_.Replace(@"\n", "");
            byte[] productionImage_ = Convert.FromBase64String(Image_);
            File.WriteAllBytes(HttpContext.Current.Server.MapPath("" + AdvertID_ + ".png"), productionImage_);
            return HttpContext.Current.Server.MapPath("" + AdvertID_ + ".png");
        }

        [WebMethod]
        public string serverPath()
        {
            try
            {
                File.WriteAllText(HttpContext.Current.Server.MapPath("1.txt"), "Ozan");
                return HttpContext.Current.Server.MapPath("");
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


    }
}
