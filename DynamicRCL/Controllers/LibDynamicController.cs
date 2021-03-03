using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynamic.Controllers
{
 
    public class LibDynamicController : Controller
    {

        [Route("/dynamic")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
