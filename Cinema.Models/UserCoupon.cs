﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Models
{
    public class UserCoupon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserCouponID { get; set; }

        [Required]
        public string UserID { get; set; } // IdentityUser uses string as primary key

        [Required]
        public int CouponID { get; set; }

        public DateTime UsedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("CouponID")]
        public virtual Coupon Coupon { get; set; }
    }
}
