using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private ApplicationDbcontext _db;
        public ProductImageRepository(ApplicationDbcontext db) : base(db)
        {
            _db = db;
        }

     

        public void Update(ProductImage obj)
        {
            _db.productImages.Update(obj);
        }
    }
}
