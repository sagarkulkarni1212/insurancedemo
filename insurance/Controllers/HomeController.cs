using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using insurance.Models;
using insurance.Models.ViewModels;
using System.IO;
using insurance.Helpers;
using Microsoft.Extensions.Configuration;

namespace insurance.Controllers
{
    public class HomeController : Controller
    {

        public IConfiguration _cnf;

        public HomeController(IConfiguration conf)
        {
            this._cnf = conf;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {

                //save customer img to azure blob
                var custId = Guid.NewGuid();

                StorageHelper stghelp = new StorageHelper();
                stghelp.ConnectionString = _cnf.GetConnectionString("StorageConnection");
                var tempFile = Path.GetTempFileName();
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    await model.ImageUrl.CopyToAsync(fs);
                }
                var fileName = Path.GetFileName(model.ImageUrl.FileName);
                var tempPath = Path.GetDirectoryName(tempFile);
                var imgPath = Path.Combine(tempPath, string.Concat(custId, "_", fileName));

                System.IO.File.Move(tempFile, imgPath);

                await stghelp.uploadCustImgAsync("images", imgPath);



                //save data to azure tbl
                Customer cust = new Customer(custId.ToString(), model.InsuranceType);
                cust.FullName = model.FullName;
                cust.Email = model.Email;
                cust.Amount = model.Amount;
                cust.AppDate = model.AppDate;
                cust.EndDate = model.EndDate;
                cust.Preminum = model.Preminum;
                cust.ImageUrl = imgPath;

                await stghelp.InsertCustAsync("table", cust);



                //add confirmation msg to azure queue
                stghelp.AddMsgAsync("isurance-req", cust);

            }
            else
            {

            }
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
