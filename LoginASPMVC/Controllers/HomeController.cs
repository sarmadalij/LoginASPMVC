using LoginASPMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LoginASPMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CodeFirstDbContext dbContext;

        public HomeController(ILogger<HomeController> logger, CodeFirstDbContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            //matching credentials in database

            var currentUser = dbContext.Users.Where(
                x => x.Email == user.Email && x.Password == user.Password
                ).FirstOrDefault();

            if (currentUser != null)
            {
                //creating session now
                //creating session variable here
                HttpContext.Session.SetString("CurrentUserSession", currentUser.Email);
                int userID = currentUser.Id;
                return RedirectToAction("Dashboard", new { id = userID });
            } 
            else
            {
                ViewBag.LoginFailed = "Invalid Credentials. Login Failed!";
            }


            return View(user);
        }

        public async Task<IActionResult> Dashboard(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await dbContext.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            if (HttpContext.Session.GetString("CurrentUserSession") != null) 
            {
                //getting session data
                ViewBag.UserSession = "Your Session email ID: "+ HttpContext.Session.GetString("CurrentUserSession").ToString();
                ViewBag.sessionStatus = true;
            }
            else
            {
                ViewBag.UserSession = "Session Not Established";
                ViewBag.sessionStatus = false;
            }

            return View(user);
        }

        public IActionResult Logout()
        {

            if (HttpContext.Session.GetString("CurrentUserSession") != null)
            {
                //removing session data
                HttpContext.Session.Remove("CurrentUserSession");
                return RedirectToAction("Login");
            }

            return View();
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            { 
                //adding data in database
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();

                TempData["register_success"] = user.Name +", You Have Registered Successfully! You Can Login Now";
                //not redirecting to login
            }
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
