using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using WebServices.Entity;
using WebServices.Classes;
using System.Transactions;
using System.Text.RegularExpressions;

namespace WebServices
{

    [WebService(Namespace = "http://192.168.2.179/AndroidService/")]
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
                        userInfo_ = ent_.APPUSER.Where(x => x.USER_EMAIL.Equals(UsernameOrMail_) && x.USER_PASSWORD.Equals(Password_)).FirstOrDefault();
                    else
                        userInfo_ = ent_.APPUSER.Where(x => x.USER_CODE.Equals(UsernameOrMail_) && x.USER_PASSWORD.Equals(Password_)).FirstOrDefault();

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
        public Result<bool> SaveUser(bool IsNewUser_, string Username_, string NameSurname_, string Email_, string Phone_, string Password_)
        {
            Result<bool> result_ = new Result<bool>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    using (TransactionScope scope_ = new TransactionScope())
                    {
                        APPUSER userInfo_ = ent_.APPUSER.Where(x => x.USER_CODE.Equals(Username_) || x.USER_EMAIL.Equals(Email_)).FirstOrDefault();
                        if (userInfo_ == null && IsNewUser_)
                        {
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

                            result_.Message = "Üyelik Bilgileri Güncellendi!";
                            result_.Success = true;
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

        public bool isEmailValid(string eMail_)
        {
            string eMailPattern_ = @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+.(com|org|net|edu|gov|mil|biz|info|mobi)(.[A-Z]{2})?$";
            Regex regex = new Regex(eMailPattern_, RegexOptions.IgnoreCase);
            return regex.IsMatch(eMail_);
        }

    }
}
