﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
//using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cinema.Models.ViewModels
{
    public class MovieDetailVM
    {
        public Movie Movie { get; set; } // hien thi movie
        public OrderTable OrderTable { get; set; } // de tien toi trang thanh toan 
        // List to store multiple seats
      
        public List<ShowtimeSeat> ShowtimeSeats { get; set; } // hien thi ghe

        
        //[ValidateNever]
        //public IEnumerable<SelectListItem> ShowTimes { get; set; }
        //public List<ShowtimeSeat> ShowtimeSeats { get; set; }
        public List<Seat> Seats { get; set; }
        public List<Product> Products { get; set; }
        public List<Theater> Cinemas { get; set; }
        public IEnumerable<OrderDetail> orderDetails { get; set; } // this is shopping cart 
        //public ShoppingCart ShoppingCart { get; set; }
        public List<Product> products { get; set; } // hien thi food 
    }
}
