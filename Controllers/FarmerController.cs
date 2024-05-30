using AgriConnectLibrary;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text;

namespace AgriConnectApplication_st10044023.Controllers
{
    public class FarmerController : Controller
    {
        //display the CreateProduct view to the farmer once they have selected the create product option in the navigation bar
        public IActionResult CreateProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProduct(IFormCollection col)
        {
            // Assign FarmerID to the product from the session
            string farmerID = EmployeeIDSession.GetEmployeeID;

            // Get data from form collection
            string productID = col["ProductID"];
            string name = col["Name"];
            string category = col["Category"];
            DateTime productionDate = Convert.ToDateTime(col["ProductDate"]);

            // Create Product object and assign FarmerID
            Product product = new Product
            {
                ProductID = productID,
                Name = name,
                Category = category,
                ProductDate = productionDate,
                FarmerID = farmerID
            };

            // Check if the form data is valid
            if (ModelState.IsValid)
            {
                //using the connections class to create a Insert query to the database to allow for the farmer to insert their new product into the database
                using (SqlConnection connection = Connections.GetConnection())
                {
                    string cmdInsert = "INSERT INTO AgriProducts (ProductID, FarmerID, Name, Category, ProductionDate) VALUES (@ProductID, @FarmerID, @Name, @Category, @ProductionDate)";
                    SqlCommand command = new SqlCommand(cmdInsert, connection);
                    command.Parameters.AddWithValue("@ProductID", product.ProductID);
                    command.Parameters.AddWithValue("@FarmerID", product.FarmerID);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Category", product.Category);
                    command.Parameters.AddWithValue("@ProductionDate", product.ProductDate);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                //showing a success message to the farmer once they have created a product
                ViewBag.SuccessMessage = "Product created successfully.";
                //clear the form so that the farmer can create more products if needed
                ModelState.Clear();
                return View(); // Return the view after successful creation
            }
            else
            {
                //else display an error message if product was not created
                ViewBag.ErrorMessage = "Error occurred while creating the product.";
                return View(product);
            }
        }

        //simple IActionResult method to dissplay all the employee profiles currently existing in the database
        public IActionResult DisplayEmployeeProfile()
        {
            List<Employee> emp = new List<Employee>();
            //using a select query to select all the employee from the AgriEmployees table inside my database
            using (SqlConnection connection = Connections.GetConnection())
            {
                string cmdSelect = "SELECT EmpID, Name, Surname FROM AgriEmployees";
                SqlCommand command = new SqlCommand(cmdSelect, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                //reading the data from the database and adding it to the employee list so that it can be displayed to the farmer
                while (reader.Read())
                {
                    Employee employee = new Employee
                    {
                        EmpID = reader.GetString(0),
                        Name = reader.GetString(1),
                        Surname = reader.GetString(2),
                    };
                    emp.Add(employee);
                }
                connection.Close();
            }

            return View(emp);
        }
        //displaying all the products created by the current logged in farmer
        public IActionResult DisplayOwnProducts(string category = null, DateTime? startDate = null, DateTime? endDate = null)
        {

            List<Product> products = new List<Product>();

            // Get FarmerID from session
            string farmerID = EmployeeIDSession.GetEmployeeID;
            //using a select query to qurey the products of the AgriProducts table where the farmerID is equal to the farmer that is currently logged in
            using (SqlConnection connection = Connections.GetConnection())
            {
                string cmdSelect = "SELECT ProductID, Name, Category, ProductionDate FROM AgriProducts WHERE FarmerID = @FarmerID";

                SqlCommand command = new SqlCommand(cmdSelect, connection);
                command.Parameters.AddWithValue("@FarmerID", farmerID);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Product product = new Product
                    {
                        ProductID = reader.GetString(0),
                        Name = reader.GetString(1),
                        Category = reader.GetString(2),
                        ProductDate = reader.GetDateTime(3)
                    };
                    products.Add(product);
                }
                connection.Close();
            }

            return View(products);
        }
    }
}
