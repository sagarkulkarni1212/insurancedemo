﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace insurance.Models.ViewModels
{
    public class CustomerViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public double Amount { get; set; }
        public double Preminum { get; set; }
        public string InsuranceType { get; set; }

        [DataType(DataType.Date)]
        public DateTime AppDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public IFormFile ImageUrl { get; set; }
    }
}
