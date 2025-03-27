﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Cinema.Models
{
    public class ShowTime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShowTimeID { get; set; }

        [Required]
        public DateTime ShowDate { get; set; } // SQL `DATE` maps to `DateTime`
        //public string ShowDates { get; set; }




        public int RoomID { get; set; }
        [ForeignKey(nameof(RoomID))]
        [ValidateNever]
        public Room Room { get; set; }


        public int MovieID { get; set; }
        [ForeignKey(nameof(MovieID))]
        [ValidateNever]
        public Movie Movie { get; set; }

    }
}