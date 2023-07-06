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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbcontext _db;
        public OrderHeaderRepository(ApplicationDbcontext db) : base(db)
        {
            _db = db;
        }

     

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }


        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id); // retrieve orderheader from database based on Id
            if (orderFromDb != null)      // work on order status
            {
                orderFromDb.OrderStatus = orderStatus;   // if order from db is not null we can update orderstatus drom db  

                if (!string.IsNullOrEmpty(paymentStatus)) // if that is not null or empty then we will update the payment status  
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

            if (!string.IsNullOrEmpty(sessionId))  // if sessionid is not null we will update order from db 
            {
                orderFromDb.SessionId = sessionId;  //we will update  from db ( when a user try to make a payment)
            }

            if (!string.IsNullOrEmpty(paymentIntentId))  // if paymentIntentId is not null that mean the payment was sussfull 
            {
                orderFromDb.PaymentIntentId = paymentIntentId ;  //we will update paymentIntentId from db 
                orderFromDb.PaymentDate = DateTime.Now;  // update payment date
            }

        }
    }
}
