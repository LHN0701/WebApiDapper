using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApiDapper.Dtos;
using WebApiDapper.Filters;
using WebApiDapper.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiDapper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly string _connectionString;

        public ProductController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DbConnectionString");
        }

        // GET: api/<ProductController>
        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            throw new Exception("Test");
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var result = await conn.QueryAsync<Product>("Get_Product_All", null, null, null, System.Data.CommandType.StoredProcedure);
                return result;
            }
        }

        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public async Task<Product> Get(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@id", id);
                var result = await conn.QueryAsync<Product>("Get_Product_ById", parameters, null, null, System.Data.CommandType.StoredProcedure);
                return result.Single();
            }
        }

        // GET paging api/<ProductController>/5
        [HttpGet("GetPaging")]
        public async Task<PagedResult<Product>> GetPaging(string keyword, int categoryId, int pageIndex, int pageSize)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@keyword", keyword);
                parameters.Add("@categoryId", categoryId);
                parameters.Add("@pageIndex", pageIndex);
                parameters.Add("@pageSize", pageSize);
                parameters.Add("@totalRow", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                var result = await conn.QueryAsync<Product>("Get_Product_AllPaging", parameters, null, null, System.Data.CommandType.StoredProcedure);

                int totalRow = parameters.Get<int>("@totalRow");
                var pagedResult = new PagedResult<Product>()
                {
                    Items = result.ToList(),
                    TotalRow = totalRow,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                return pagedResult;
            }
        }

        // POST api/<ProductController>
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            int newId = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@sku", product.Sku);
                parameters.Add("@price", product.Price);
                parameters.Add("@isActive", product.IsActive);
                parameters.Add("@imageUrl", product.ImageUrl);
                parameters.Add("@id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                var result = await conn.ExecuteAsync("Create_Product", parameters, null, null, System.Data.CommandType.StoredProcedure);

                newId = parameters.Get<int>("@id");
                return Ok(newId);
            }
        }

        // PUT api/<ProductController>/5
        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@id", id);
                parameters.Add("@sku", product.Sku);
                parameters.Add("@price", product.Price);
                parameters.Add("@isActive", product.IsActive);
                parameters.Add("@imageUrl", product.ImageUrl);
                await conn.ExecuteAsync("Update_Product", parameters, null, null, System.Data.CommandType.StoredProcedure);
                return Ok();
            }
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@id", id);
                await conn.ExecuteAsync("Delete_Product_ById", parameters, null, null, System.Data.CommandType.StoredProcedure);
            }
        }
    }
}