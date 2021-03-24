using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public class Product : BaseEntity
    {
        public string ProductName { get; set; }
        public virtual ProductDetails ProductDetails { get; set; }
    }
}
