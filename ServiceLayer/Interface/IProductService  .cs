using DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IProductService
    {
        IEnumerable<Product> GetProduct();
        Product GetProduct(int id);

    }
}