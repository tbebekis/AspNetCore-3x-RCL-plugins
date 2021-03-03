using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaticRCL.Controllers
{
 
    public class LibController : Controller
    {
        [Route("/static")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
