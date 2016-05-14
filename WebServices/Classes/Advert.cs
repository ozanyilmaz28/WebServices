using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServices.Classes
{
    public class Advert
    {
        public long ID { get; set; }

        public string Description { get; set; }

        public int? Price { get; set; }

        public DateTime? Datetime { get; set; }

        public string ImageLink { get; set; }

        public string Phone { get; set; }

        public string Mail { get; set; }

        public string MainCategoryCode { get; set; }

        public string SubCategoryCode { get; set; }

        public bool? IsOpen { get; set; }

        public Advert()
        {
        }
    }
}