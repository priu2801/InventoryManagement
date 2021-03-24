using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public class ProductDetails : BaseEntity
    {
        public int StockAvailable { get; set; }
        public decimal Price { get; set; }
        public virtual Product Product { get; set; }
    }
}
