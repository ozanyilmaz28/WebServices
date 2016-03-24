using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using WebServices.Entity;
using WebServices.Classes;
using System.Transactions;

namespace WebServices
{

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class AndroidService : System.Web.Services.WebService
    {

        [WebMethod]
        public Result<bool> SaveUser()
        {
            Result<bool> result_ = new Result<bool>();

            try
            {
                using (GRADUATIONEntities ent_ = new GRADUATIONEntities())
                {
                    using (TransactionScope scope_ = new TransactionScope())
                    {
                        object userInfo_ = ent_.APPUSER.Where(x => x.USER_CODE.Equals("TestUser") || x.USER_EMAIL.Equals("testuser@gmail.com")).FirstOrDefault();
                        if (userInfo_ == null)
                        {
                            APPUSER insertUser_ = new APPUSER();
                            insertUser_.USER_CODE = "TestUser";
                            insertUser_.USER_NAMESURNAME = "Test User";
                            insertUser_.USER_EMAIL = "testuser@gmail.com";
                            insertUser_.USER_PHONE = "542 542 42 42";
                            insertUser_.USER_PASSWORD = "test123";
                            ent_.AddToAPPUSER(insertUser_);
                            ent_.SaveChanges();
                            scope_.Complete();
                            ent_.AcceptAllChanges();

                            result_.Message = "Üyelik Kaydı Başarılı!";
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

    }
}
