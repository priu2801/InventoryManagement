using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceLayer.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Controllers
{
   // [Route("api/[controller]")]
   [ApiVersion("1")]
   [ApiVersion("2")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        #region Fields
        private readonly IProductService productService;
        private readonly IProductDetailsService productDetailsService;
        private readonly ILoggerService _loggerService;
        #endregion


        #region Ctor
        public ProductController(IProductService productService, IProductDetailsService productDetailsService, ILoggerService loggerService)
        {
            this.productService = productService;
            this.productDetailsService = productDetailsService;
            this._loggerService = loggerService;
        }
        #endregion

        [HttpGet]
        [MapToApiVersion("1")]
        [SwaggerOperation(Tags = new[] { "Product Management" })]
        [Route("~/api/v{version:apiVersion}/Product/GetAllProducts")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var product = await productService.GetProduct();
                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _loggerService.Error(ex);
                return BadRequest();
            }
           
        }

        [HttpPost]
        [MapToApiVersion("2")]
        [SwaggerOperation(Tags = new[] { "Product Management" })]
        [Route("~/api/v{version:apiVersion}/Product/AddProduct")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddProduct([FromBody] Product model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await productService.InsertProduct(model);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _loggerService.Error(ex);
                    return BadRequest();
                }

            }

            return BadRequest();
        }

        [HttpDelete]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [SwaggerOperation(Tags = new[] { "Product Management" })]
        [Route("~/api/v{version:apiVersion}/Product/DeleteProduct")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteProduct(int? productId)
        {
            int result = 0;

            if (productId == null)
            {
                return BadRequest();
            }

            try
            {
                result = await productService.DeleteProduct(Convert.ToInt64(productId));
                if (result == 0)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex.GetType().FullName ==
                             "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException")
                {
                    return NotFound();
                }

                _loggerService.Error(ex);
                return BadRequest();
            }
        }


        [HttpPut]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [SwaggerOperation(Tags = new[] { "Product Management" })]
        [Route("~/api/v{version:apiVersion}/Product/UpdateProduct")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateProduct([FromBody] Product model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await productService.UpdateProduct(model);

                    return Ok();
                }
                catch (Exception ex)
                {
                    if (ex.GetType().FullName ==
                             "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException")
                    {
                        return NotFound();
                    }
                    _loggerService.Error(ex);
                    return BadRequest();
                }
            }

            return BadRequest();
        }


    }
}
