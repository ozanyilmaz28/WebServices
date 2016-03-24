using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServices.Classes
{
    public class Result<Type>
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public Type Data { get; set; }

        private List<string> _Errors = new List<string>();
        public List<string> ErrorMessages { get { return _Errors; } }

        public Result()
        {
        }
    }
}