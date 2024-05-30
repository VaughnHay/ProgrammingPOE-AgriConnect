using AgriConnectLibrary;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AgriConnectApplication_st10044023.Controllers
{
    public class EmployeeController : Controller
    {
        //Display the form to the user if they select the Create Farmer option in the navigation bar
        public IActionResult CreateFarmer()
        {
            return View();
        }

        //Handling the form submission for creating a new farmer
        [HttpPost]
        public IActionResult CreateFarmer(IFormCollection col)
        {
            // Retrieve EmpID from session which is the ID of the current logged in employee
            string empID = EmployeeIDSession.GetEmployeeID;

            // Get data from form collection 
            string farmerID = col["FarmerID"];
            string name = col["Name"];
            string email = col["Email"];
            string password = col["Password"];

            // Create Farmer object and assign EmpID to farmer profile of the current logged in Employee
            Farmer farmer = new Farmer
            {
                FarmerID = farmerID,
                EmpID = empID,
                Name = name,
                Email = email,
                Password = password
            };

            //checking if the data inserted by the employee is valid
            if (ModelState.IsValid)
            {
                //getting the connection from the connections class and creating a query to insert the data posted by the employee into the database
                using (SqlConnection connection = Connections.GetConnection())
                {
                    //the data inserted by the employee will be posted into the database using the INSERT query
                    string cmdInsert = "INSERT INTO AgriFarmers (FarmerID, EmpID, Name, Email, Password) VALUES (@FarmerID, @EmpID, @Name, @Email, @Password)";
                    SqlCommand command = new SqlCommand(cmdInsert, connection);
                    command.Parameters.AddWithValue("@FarmerID", farmer.FarmerID);
                    command.Parameters.AddWithValue("@EmpID", farmer.EmpID);
                    command.Parameters.AddWithValue("@Name", farmer.Name);
                    command.Parameters.AddWithValue("@Email", farmer.Email);
                    command.Parameters.AddWithValue("@Password", farmer.Password);
                    //open the connection, execute the query and then once the query has been executed we close the connection
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                //once the data has been validated and the data has been inserted we will display a success message to the employee so that they can see that they have created a farmer
                ViewBag.SuccessMessage = "Farmer profile created successfully.";
                //clear the form to allow for more data to be entered
                ModelState.Clear();
                return View(); // Return the view after successful creation
            }
            else
            {
                //else we show an error message if something went wrong
                ViewBag.ErrorMessage = "Error occurred while creating the farmer profile.";
                return View(farmer);
            }
        }

        //method to display all the farmer profilesS
        public IActionResult DisplayFarmerProfile()
        {
            List<Farmer> farmers = new List<Farmer>();
            //using the sql connection to select all the data inside the Farmer table
            using (SqlConnection connection = Connections.GetConnection())
            {
                string cmdSelect = "SELECT FarmerID, EmpID, Name, Email FROM AgriFarmers";
                SqlCommand command = new SqlCommand(cmdSelect, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                //read the data and then display it in the table to the employee
                while (reader.Read())
                {
                    Farmer farmer = new Farmer
                    {
                        FarmerID = reader.GetString(0),
                        EmpID = reader.GetString(1),
                        Name = reader.GetString(2),
                        Email = reader.GetString(3)
                    };
                    farmers.Add(farmer);
                }
                connection.Close();
            }
            //show the employee the latest farmer profiles
            return View(farmers);
        }

        //IActionResult method to display a list of products with the filter options that the employee can use to filter through the products 
        public IActionResult DisplayProducts(string farmerID = null, string category = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<Product> products = new List<Product>();
            //get all the products from the AgriProducts table in the database 
            using (SqlConnection connection = Connections.GetConnection())
            {
                string query = "SELECT ProductID, FarmerID, Name, Category, ProductionDate FROM AgriProducts";
                List<string> filterChoice = new List<string>();

                //then adding filters to the select query if the user has selected the filter option
                if (!string.IsNullOrEmpty(farmerID))
                {
                    filterChoice.Add("FarmerID = @FarmerID");
                }
                if (!string.IsNullOrEmpty(category))
                {
                    filterChoice.Add("Category = @Category");
                }
                if (startDate.HasValue)
                {
                    filterChoice.Add("ProductionDate >= @StartDate");
                }
                if (endDate.HasValue)
                {
                    filterChoice.Add("ProductionDate <= @EndDate");
                }
                //adding a where clause to the query if there are any filters selected by the emplouee
                if (filterChoice.Count > 0)
                {
                    query += " WHERE " + string.Join(" AND ", filterChoice);
                }

                SqlCommand command = new SqlCommand(query, connection);
                //adding paramaters to the command if they were selected 
                if (!string.IsNullOrEmpty(farmerID))
                {
                    command.Parameters.AddWithValue("@FarmerID", farmerID);
                }
                if (!string.IsNullOrEmpty(category))
                {
                    command.Parameters.AddWithValue("@Category", category);
                }
                if (startDate.HasValue)
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                }
                if (endDate.HasValue)
                {
                    command.Parameters.AddWithValue("@EndDate", endDate);
                }

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                //reading the data and adding it to a new product list wich will then be displayed to the employee
                while (reader.Read())
                {
                    Product product = new Product
                    {
                        ProductID = reader.GetString(0),
                        FarmerID = reader.GetString(1),
                        Name = reader.GetString(2),
                        Category = reader.GetString(3),
                        ProductDate = reader.GetDateTime(4)
                    };
                    products.Add(product);
                }
                connection.Close();
            }
            //using the ProductFilter class to update the DisplayProducts view with the new list of products after filtering
            var filterViewModel = new ProductFilter
            {
                FarmerID = farmerID,
                Category = category,
                StartDate = startDate,
                EndDate = endDate,
                Products = products
            };
            //passing the view model to the view
            return View(filterViewModel);
        }
    }
}
