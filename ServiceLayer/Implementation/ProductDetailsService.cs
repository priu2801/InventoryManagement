using DataAccess;
using Repository;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Implementation
{
    public class ProductDetailsService : IProductDetailsService
    {
        private IRepository<ProductDetails> productDetailsRepository;

        public ProductDetailsService(IRepository<ProductDetails> productDetailsRepository)
        {
            this.productDetailsRepository = productDetailsRepository;
        }

        public ProductDetails GetProductDetail(int id)
        {
            return productDetailsRepository.Get(id);
        }
    }
}
