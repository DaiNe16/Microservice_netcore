using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        private IWebHostEnvironment _environment;
        public ProductAPIController(AppDbContext appDbContext, IMapper mapper, IWebHostEnvironment environment)
        {
            _db = appDbContext;
            _mapper = mapper;
            _response = new ResponseDto();
            _environment = environment; 

        }
        [HttpPost("UploadProductImage")]
        public async Task<ResponseDto> UploadProductImage(int productId, IFormFile source)
        {
            try
            {
                var product = _db.Products.FirstOrDefault(u => u.ProductId == productId);
                if(product == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Product with id "+ productId + " is invalid.";
                    return _response;
                }
                string fileName = source.FileName;
                string folderPath = GetFolderPath(productId);
                if(!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                //string imagePath = "";
                //if (fileName.EndsWith(".png")){
                //    imagePath = folderPath + "\\image.png";
                //}
                //else if (fileName.EndsWith(".jpg"))
                //{
                //    imagePath = folderPath + "\\image.jpg";
                //}
                //else
                //{
                //    _response.IsSuccess = false;
                //    _response.Message = "Type of image is invalid.";
                //    return _response;
                //}
                string imagePath = folderPath + "\\image.png";
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                using (FileStream stream = System.IO.File.Create(imagePath)) 
                {
                    await source.CopyToAsync(stream);
                }

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete("RemoveProductImage")]
        public async Task<ResponseDto> RemoveProductImage(int productId)
        {
            try
            {
                var product = _db.Products.FirstOrDefault(u => u.ProductId == productId);
                if (product == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Product with id " + productId + " is invalid.";
                    return _response;
                }
                string folderPath = GetFolderPath(productId);
                if(Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [NonAction]
        private string GetFolderPath(int productId)
        {
            return _environment.WebRootPath + "\\Uploads\\Product\\" + productId;
        }

        [NonAction]
        private string GetImagePathByProductId(int productId)
        {
            string hostPath = "https://localhost:7003";
            string imagePath = GetFolderPath(productId) + "\\image.png";
            string imageUrl = "";
            if (System.IO.File.Exists(imagePath))
            {
                imageUrl = hostPath + "/Uploads/Product/"+productId+"/image.png";
            }
            return imageUrl;
        }


        [HttpGet("GetAll")]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<ProductDto> listProductDto = _mapper.Map<IEnumerable<ProductDto>>(_db.Products.ToList());
                List<ProductDto> listPro = new List<ProductDto>(listProductDto);
                listPro.ForEach(p =>
                {
                    p.ImageUrl = GetImagePathByProductId(p.ProductId);
                });
                _response.Result = listPro;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet("GetProductById")]
        [Authorize]
        public ResponseDto Get(int id)
        {
            try
            {
                ProductDto productDto = _mapper.Map<ProductDto>(_db.Products.FirstOrDefault(u => u.ProductId == id));
                _response.Result = productDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
            
        }

        [HttpPost("CreateProduct")]
        [Authorize(Roles = "Admin")]
        public ResponseDto Post(ProductDto productDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);
                _db.Add(product);
                _db.SaveChanges();
                _response.Message = "Successfully to create product";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;

        }

        [HttpPut("UpdateProduct")]
        [Authorize(Roles = "Admin")]
        public ResponseDto Put(ProductDto productDto)
        {
            try
            {
                var product_db = _db.Products.AsNoTracking().FirstOrDefault(u => u.ProductId == productDto.ProductId);
                if(product_db == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Product is not found";
                    return _response;
                }
                Product product = _mapper.Map<Product>(productDto);
                _db.Update(product);
                _db.SaveChanges();
                _response.Message = "Successfully to update product";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;

        }

        [HttpDelete("DeleteProduct")]
        [Authorize(Roles = "Admin")]
        public ResponseDto Delete(int id)
        {
            try
            {
                var product_db = _db.Products.FirstOrDefault(u => u.ProductId == id);
                if (product_db == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Product is not found";
                    return _response;
                }
                string folderPath = GetFolderPath(id);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
                _db.Remove(product_db);
                _db.SaveChanges();
                _response.Message = "Successfully to delete product";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;

        }

    }
}
