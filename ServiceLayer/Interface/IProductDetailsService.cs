using DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IProductDetailsService
    {

       ProductDetails GetProductDetail(int id);
    }
}
