using Microsoft.AspNetCore.Mvc;
using AgriConnectLibrary;

namespace AgriConnectApplication_st10044023.Controllers
{
    public class LoginController : Controller
    {
        //display the login page to the user where they can then enter their credentials to log into the application
        public IActionResult Index()
        {
            return View();
        }
        //IactionResult method to determine which main page to show based off the users logged in credentials and user type
        public IActionResult MainPage()
        {
            //get the user ID and user type from the session and assign it to their ID
            string userID = EmployeeIDSession.GetEmployeeID;
            string userType = EmployeeIDSession.GetUserType;
            //if the user is an employee then display the employee main page
            if (userType == "Employee")
            {
                //retrieve the logged in employees details and ID
                Employee loggedEmployee = Employee.GetEmployeeByID(userID);
                //display the employee main page
                return View("~/Views/Employee/MainPageEmployee.cshtml", loggedEmployee);
            }
            //if the user is a farmer then display the farmer main page
            else if (userType == "Farmer")
            {
                Farmer loggedFarmer = Farmer.GetFarmerByID(userID);
                return View("~/Views/Farmer/MainPageFarmer.cshtml", loggedFarmer);
            }
            //if the user trype is neither employee or farmer then redirect them back to login page with an error message
            return RedirectToAction("Index");
        }
        //handling the login process when the form is submitted
        [HttpPost]
        public IActionResult ProcessLogin(UserLogin userLogin)
        {
            //retrive the employee with the provided empID
            Employee loggedEmployee = Employee.GetEmployeeByID(userLogin.UserID);
            //retrieve the farmer with the provided farmerID
            Farmer loggedFarmer = Farmer.GetFarmerByID(userLogin.UserID);
            //using if statement to check if the employee exists in the database and that their password matches
            if (loggedEmployee != null && userLogin.Password == loggedEmployee.Password)
            {
                //assign the employee ID and their userTyppe to the session
                EmployeeIDSession.AssignID(loggedEmployee.EmpID, "Employee");
                //redirect the employee to the main page
                return RedirectToAction("MainPage");
            }
            //esle if statement to check if the farmer exists and their password matches
            else if (loggedFarmer != null && userLogin.Password == loggedFarmer.Password)
            {
                EmployeeIDSession.AssignID(loggedFarmer.FarmerID, "Farmer");
                return RedirectToAction("MainPage");
            }
            //else if the users credentials do not match then they will be displayed with an error message and redirected back to the login page
            else
            {
                ModelState.Clear();
                ModelState.AddModelError(string.Empty, "Invalid ID or Password. Please try again.");
                return View("Index", userLogin);
            }
        }
    }
}
