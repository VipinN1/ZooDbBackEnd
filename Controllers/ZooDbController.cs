using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using BackEnd.Models;
using System;
using System.Dynamic;
using System.Text;



namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZooDbController : ControllerBase
    {
        private IConfiguration _configuration;
        public ZooDbController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetTestTable")]
        public JsonResult GetTestTable()
        {

            string query = "select * from test";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);

        }

        [HttpPost]
        [Route("NewUser")]
        public JsonResult NewUser([FromBody] UserDetails userDetails)
        {
            // Prepare the SQL query for inserting a new user
            string query = "INSERT INTO user_info (email, password, user_type) VALUES (@Email, @Password, @UserType)";

            // Create a new DataTable to store the result (although in this case, there's no result to store)
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@Email", userDetails.Email);
                    myCommand.Parameters.AddWithValue("@Password", userDetails.Password);
                    myCommand.Parameters.AddWithValue("@UserType", userDetails.UserType);

                    // Execute the query (which in this case is an INSERT operation)
                    myCommand.ExecuteNonQuery();
                }
            }

            // Return a response indicating success
            return new JsonResult("New user added successfully");
        }

        [HttpPost]
        [Route("NewUserProfile")]
        public JsonResult NewUserProfile([FromBody] UserProfile userProfile)
        {
            // Prepare the SQL query for inserting a new user must be same as database
            string query = "INSERT INTO customer (first_name, last_name, phone_number, email) VALUES (@firstName, @lastName, @phoneNumber, @email)";

            // Create a new DataTable to store the result (although in this case, there's no result to store)
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@firstName", userProfile.firstName);
                    myCommand.Parameters.AddWithValue("@lastName", userProfile.lastName);
                    myCommand.Parameters.AddWithValue("@phoneNumber", userProfile.phoneNumber);
                    myCommand.Parameters.AddWithValue("@email", userProfile.email);


                    // Execute the query (which in this case is an INSERT operation)
                    myCommand.ExecuteNonQuery();
                }
            }

            // Return a response indicating success
            return new JsonResult("New user profile added successfully");
        }



            [HttpPost]
            [Route("NewDiet")]
            public JsonResult NewDiet([FromBody] Diet newDiet)
            {
                // Prepare SQL queries
                string checkAnimalExistsQuery = "SELECT animal_id FROM animal WHERE animal_name = @animalName AND animal_species = @animalSpecies AND animal_DoB = @animalDoB";
                string insertDietQuery = "INSERT INTO diet (animal_id, diet_name, diet_type, diet_schedule) VALUES (@animalID, @dietName, @dietType, @dietSchedule)";
                string updateDietQuery = "UPDATE diet SET diet_name = @dietName, diet_type = @dietType, diet_schedule = @dietSchedule WHERE animal_id = @animalID";

                // Get the connection string from appsettings.json
                string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

                // Define dietExists outside of the using block so it's accessible later
                bool dietExists = false;

                // Open a connection to the database
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();

                    // Check if animal exists and get animal_id
                    int animalId = 0;
                    using (SqlCommand checkAnimalCmd = new SqlCommand(checkAnimalExistsQuery, myCon))
                    {
                        checkAnimalCmd.Parameters.AddWithValue("@animalName", newDiet.animalName);
                        checkAnimalCmd.Parameters.AddWithValue("@animalSpecies", newDiet.animalSpecies);
                        checkAnimalCmd.Parameters.AddWithValue("@animalDoB", newDiet.animalDoB);

                        object result = checkAnimalCmd.ExecuteScalar();  // Use ExecuteScalar to get the first column of the first row
                        if (result != null)
                            animalId = Convert.ToInt32(result);
                        else
                            return new JsonResult("No such animal found");
                    }

                    // Check if diet entry exists for this animal_id
                    using (SqlCommand checkDietCmd = new SqlCommand("SELECT COUNT(1) FROM diet WHERE animal_id = @animalID", myCon))
                    {
                        checkDietCmd.Parameters.AddWithValue("@animalID", animalId);
                        dietExists = (int)checkDietCmd.ExecuteScalar() > 0;
                    }

                    // Insert or update diet information
                    using (SqlCommand dietCmd = new SqlCommand(dietExists ? updateDietQuery : insertDietQuery, myCon))
                    {
                        dietCmd.Parameters.AddWithValue("@animalID", animalId);
                        dietCmd.Parameters.AddWithValue("@dietName", newDiet.dietName);
                        dietCmd.Parameters.AddWithValue("@dietType", newDiet.dietType);
                        dietCmd.Parameters.AddWithValue("@dietSchedule", newDiet.dietSchedule);

                        dietCmd.ExecuteNonQuery();  // Execute either update or insert
                    }
                }

                // Return a response indicating success
                return new JsonResult(dietExists ? "Diet updated successfully" : "New diet added successfully");
            }






        public class ModifyVetRequest
        {
            public VetRecords OriginalVetRecord { get; set; }
            public VetRecords UpdatedVetRecord { get; set; }
        }

        [HttpPut]
        [Route("Vet/Modify")]
        public JsonResult ModifyVetRecords([FromBody] ModifyVetRequest request)
        {
            VetRecords originalVetRecord = request.OriginalVetRecord;
            VetRecords updatedVetRecord = request.UpdatedVetRecord;

            // Prepare the SQL query to update vet records
            string query = @"
    UPDATE vet_records
    SET
        weight = @newWeight,
        height = @newHeight,
        diagnosis = @newDiagnosis,
        medications = @newMedications
    WHERE animal_id = (
        SELECT animal_id 
        FROM animal 
        WHERE animal_name = @originalAnimalName 
            AND animal_species = @originalAnimalSpecies 
            AND animal_DoB = @originalAnimalDoB
    )";

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            try
            {
                // Open a connection to the database and execute the query
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();

                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        // Parameters for the new (updated) data
                        myCommand.Parameters.AddWithValue("@newWeight", updatedVetRecord.weight);
                        myCommand.Parameters.AddWithValue("@newHeight", updatedVetRecord.height);
                        myCommand.Parameters.AddWithValue("@newDiagnosis", updatedVetRecord.diagnosis);
                        myCommand.Parameters.AddWithValue("@newMedications", updatedVetRecord.medications);

                        // Parameters for matching the original data (to find the correct animal_id)
                        myCommand.Parameters.AddWithValue("@originalAnimalName", originalVetRecord.animalName);
                        myCommand.Parameters.AddWithValue("@originalAnimalSpecies", originalVetRecord.animalSpecies);
                        myCommand.Parameters.AddWithValue("@originalAnimalDoB", originalVetRecord.animalDoB);

                        // Execute the update query
                        int rowsAffected = myCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { message = "Vet records updated successfully." });
                        }
                        else
                        {
                            // If no rows were affected, the vet records were not found
                            return new JsonResult(new { message = "Vet records not found.", status = 404 });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.Error.WriteLine(ex);

                // Return an error response
                return new JsonResult(new { message = "An error occurred while updating vet records.", status = 500 });
            }
        }



        [HttpPost]
        [Route("NewEnclosure")]
        public JsonResult NewEnclosure([FromBody] Enclosure newEnclosure)
        {
            // Prepare the SQL query for inserting a new user
            string query = "INSERT INTO enclosure (enclosure_name, enclosure_type, built_date, cleaning_schedule_start, cleaning_schedule_end) VALUES (@enclosureName, @enclosureType, @builtDate, @cleaningScheduleStart, @cleaningScheduleEnd)";

            // Create a new DataTable to store the result (although in this case, there's no result to store)
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@enclosureName", newEnclosure.enclosureName);
                    myCommand.Parameters.AddWithValue("@enclosureType", newEnclosure.enclosureType);
                    myCommand.Parameters.AddWithValue("@builtDate", newEnclosure.builtDate);
                    myCommand.Parameters.AddWithValue("@cleaningScheduleStart", newEnclosure.cleaningScheduleStart);
                    myCommand.Parameters.AddWithValue("@cleaningScheduleEnd", newEnclosure.cleaningScheduleEnd);

                    // Execute the query (which in this case is an INSERT operation)
                    myCommand.ExecuteNonQuery();
                }
            }

            // Return a response indicating success
            return new JsonResult("New Enclosure added successfully");
        }




        [HttpPost]
        [Route("NewAnimal")]
        public JsonResult NewAnimal([FromBody] Animal newAnimal)
        {
            // Prepare the SQL query for inserting a new user must be same as database
            string query = "INSERT INTO animal (animal_name, animal_species, animal_gender, animal_DoB, animal_endangered, animal_DoA, animal_origin) VALUES (@animalName, @animalSpecies, @animalGender, @animalDoB, @animalEndangered, @animalDoA, @animalOrigin)";

            // Create a new DataTable to store the result (although in this case, there's no result to store)
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    //Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@animalName", newAnimal.animalName);
                    myCommand.Parameters.AddWithValue("@animalSpecies", newAnimal.animalSpecies);
                    myCommand.Parameters.AddWithValue("@animalGender", newAnimal.animalGender);
                    myCommand.Parameters.AddWithValue("@animalDoB", newAnimal.animalDoB);
                    myCommand.Parameters.AddWithValue("@animalEndangered", newAnimal.animalEndangered);
                    myCommand.Parameters.AddWithValue("@animalDoA", newAnimal.animalDoA);
                    myCommand.Parameters.AddWithValue("@animalOrigin", newAnimal.animalOrigin);




                    // Execute the query (which in this case is an INSERT operation)
                    myCommand.ExecuteNonQuery();
                }
            }

            // Return a response indicating success
            return new JsonResult("New animal added");
        }




        [HttpPost]
        [Route("NewSecurityReport")]
        public JsonResult NewSecurityReport([FromBody] SecurityReport NewSecurityReport)
        {
            // Prepare the SQL query for inserting a new user must be same as database
            string query = "INSERT INTO security_logs (emp_id, log_date, log_time, event_description, event_location, severity_level) VALUES (@empId, @date, @time, @eventDescription, @location, @severityLevel)";

            // Create a new DataTable to store the result (although in this case, there's no result to store)
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@empId", NewSecurityReport.empId);
                    myCommand.Parameters.AddWithValue("@date", NewSecurityReport.date);
                    myCommand.Parameters.AddWithValue("@time", NewSecurityReport.time);
                    myCommand.Parameters.AddWithValue("@eventDescription", NewSecurityReport.eventDescription);
                    myCommand.Parameters.AddWithValue("@location", NewSecurityReport.location);
                    myCommand.Parameters.AddWithValue("@severityLevel", NewSecurityReport.severityLevel);



                    // Execute the query (which in this case is an INSERT operation)
                    myCommand.ExecuteNonQuery();
                }
            }

            // Return a response indicating success
            return new JsonResult("New security log added successfully");
        }




        [HttpPost]
        [Route("ValidateUser")]
        public JsonResult ValidateUser([FromBody] UserCredentials credentials)
        {
            // Prepare the SQL query to fetch the user type and email based on provided email and password
            string query = "SELECT user_type, email FROM user_info WHERE email = @Email AND password = @Password";

            // Create a new DataTable to store the result
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@Email", credentials.email);
                    myCommand.Parameters.AddWithValue("@Password", credentials.password);

                    // Execute the query and load the results into the DataTable
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        table.Load(myReader);
                        myReader.Close();
                    }
                }
                

                // Check if the DataTable has any rows (i.e., if the credentials are valid)
                if (table.Rows.Count > 0)
                {
                    // Extract user type and email from the DataTable
                    string userType = table.Rows[0]["user_type"].ToString();
                    string userEmail = table.Rows[0]["email"].ToString();

                    // Log user type
                    // Console.WriteLine("User Type: " + userType);
                    // Console.WriteLine("email " + userEmail);

                    // Check user type and perform additional queries accordingly
                    if (userType == "employee" || userType == "manager")
                    {
                        //Console.WriteLine("It went inside if statement");
                        // Perform query to fetch employee_id from employee table using the email
                        string employeeQuery = "SELECT emp_id FROM employee WHERE email = @Email";
                        using (SqlCommand employeeCommand = new SqlCommand(employeeQuery, myCon))
                        {
                            employeeCommand.Parameters.AddWithValue("@Email", userEmail);
                            int employeeId = (int)employeeCommand.ExecuteScalar();

                            // Return the user_type and employee_id if found
                            return new JsonResult(new { success = true, userType, employeeId });
                        }
                    }
                    else if (userType == "customer")
                    {
                        // Perform query to fetch customer_id from customer table using the email
                        string customerQuery = "SELECT customer_id FROM customer WHERE email = @Email";
                        using (SqlCommand customerCommand = new SqlCommand(customerQuery, myCon))
                        {
                            customerCommand.Parameters.AddWithValue("@Email", userEmail);
                            int customerId = (int)customerCommand.ExecuteScalar();

                            // Return the user_type and customer_id if found
                            return new JsonResult(new { success = true, userType, customerId });
                        }
                    }
                    else
                    {
                        // Return an error message if user type is neither employee nor customer
                        return new JsonResult(new { success = false, message = "Invalid user type" });
                        //Console.WriteLine("It went inside else statement");
                    }
                }
                else
                {
                    // Return an error message if no valid credentials found
                    return new JsonResult(new { success = false, message = "Invalid credentials" });
                }
            }
        }


                [HttpPost]
                [Route("NewDonation")]
                public JsonResult NewDonation([FromBody] Donation userDonation)
                {
                    // Prepare the SQL query for inserting a new user must be same as database
                    string query = "INSERT INTO donations (donation_amount, customer_id, donation_date) VALUES (@donationAmount, @customerId, @donationDate)";

                    // Create a new DataTable to store the result (although in this case, there's no result to store)
                    DataTable table = new DataTable();

                    // Get the connection string from appsettings.json
                    string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

                    // Open a connection to the database and execute the query
                    using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                    {
                        myCon.Open();
                        using (SqlCommand myCommand = new SqlCommand(query, myCon))
                        {
                            // Add parameters to the command to prevent SQL injection
                            myCommand.Parameters.AddWithValue("@donationAmount", userDonation.donationAmount);
                            myCommand.Parameters.AddWithValue("@customerId", userDonation.customerId);
                            myCommand.Parameters.AddWithValue("@donationDate", userDonation.donationDate);


                            // Execute the query (which in this case is an INSERT operation)
                            myCommand.ExecuteNonQuery();
                        }
                    }

                    // Return a response indicating success
                    return new JsonResult("New donation added successfully");
                }


                [HttpPost]
                [Route("NewClockIn")]
                public JsonResult NewClockIn([FromBody] ClockIn userClockIn)
                {

                    string query = "INSERT INTO emp_schedule (emp_id, clock_in, clock_out, total_hours) VALUES (@empId, @clockIn, @clockOut, @totalHours)";

                    DataTable table = new DataTable();

                    string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

                    using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                    {
                        myCon.Open();
                        using (SqlCommand myCommand = new SqlCommand(query, myCon))
                        {
                            // Add parameters to the command to prevent SQL injection
                            myCommand.Parameters.AddWithValue("@empId", userClockIn.empId);
                            myCommand.Parameters.AddWithValue("@clockIn", userClockIn.clockIn);
                            myCommand.Parameters.AddWithValue("@clockOut", userClockIn.clockOut);
                            myCommand.Parameters.AddWithValue("@totalHours", userClockIn.totalHours);


                            // Execute the query (which in this case is an INSERT operation)
                            myCommand.ExecuteNonQuery();
                        }
                    }

                    // Return a response indicating success
                    return new JsonResult("New clockIn added successfully");

                }



                [HttpPost]
                [Route("NewTickets")]
                public JsonResult NewTickets([FromBody] TicketPurchase newTicketPurchase)
                {
                    // Prepare the SQL query for inserting a new ticket purchase
                    string query = "INSERT INTO ticket_purchase (adult_tickets, child_tickets, senior_tickets, infant_tickets, visit_date, total_cost, customer_id) VALUES (@adultTickets, @childTickets, @seniorTickets, @infantTickets, @formattedDate, @totalCost, @customerId)";

                    // Create a new DataTable to store the result (although in this case, there's no result to store)
                    DataTable table = new DataTable();

                    // Get the connection string from appsettings.json
                    string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

                    // Open a connection to the database and execute the query
                    using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                    {
                        myCon.Open();
                        using (SqlCommand myCommand = new SqlCommand(query, myCon))
                        {
                            // Add parameters to the command to prevent SQL injection
                            myCommand.Parameters.AddWithValue("@adultTickets", newTicketPurchase.adultTickets);
                            myCommand.Parameters.AddWithValue("@childTickets", newTicketPurchase.childTickets);
                            myCommand.Parameters.AddWithValue("@seniorTickets", newTicketPurchase.seniorTickets);
                            myCommand.Parameters.AddWithValue("@infantTickets", newTicketPurchase.infantTickets);
                            myCommand.Parameters.AddWithValue("@formattedDate", newTicketPurchase.formattedDate); // Make sure visitDate is of type DateTime
                            myCommand.Parameters.AddWithValue("@totalCost", newTicketPurchase.totalCost);
                            myCommand.Parameters.AddWithValue("@customerId", newTicketPurchase.customerId);

                            // Execute the query (which in this case is an INSERT operation)
                            myCommand.ExecuteNonQuery();
                        }
                    }

                    // Return a response indicating success
                    return new JsonResult("New ticket purchase added successfully");
                }


            [HttpPost]
            [Route("GenerateSecurityReportByDates")]
            public JsonResult GenerateSecurityReportByDates([FromBody] dynamic data)
            {
                // Extracting the dates from the request body
                DateTime startDate = Convert.ToDateTime(data.startDate);
                DateTime endDate = Convert.ToDateTime(data.endDate);

                // SQL query to fetch security reports within the specified date range
                string query = "SELECT emp_id, log_date, log_time, event_description, event_location, severity_level, log_id " +
                            "FROM security_logs " +
                            "WHERE log_date BETWEEN @startDate AND @endDate";

                        // List to hold the results
                        List<SecurityReport> securityReports = new List<SecurityReport>();

                // Get the connection string from appsettings.json
                string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

                // Execute the query and load the results
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        // Adding parameters to the command
                        myCommand.Parameters.AddWithValue("@startDate", startDate);
                        myCommand.Parameters.AddWithValue("@endDate", endDate);

                        using (SqlDataReader myReader = myCommand.ExecuteReader())
                        {
                            while (myReader.Read())
                            {
                                        // Create a SecurityReport object and populate its properties
                                        SecurityReport securityReport = new SecurityReport
                                        {
                                            empID = Convert.ToInt32(myReader["emp_id"]),
                                            date = Convert.ToDateTime(myReader["log_date"]),
                                            time = myReader.GetTimeSpan(myReader.GetOrdinal("log_time")),
                                            eventDescription = myReader["event_description"].ToString(),
                                            location = myReader["event_location"].ToString(),
                                            severityLevel = myReader["severity_level"].ToString(),
                                            logID = Convert.ToInt32(myReader["log_id"])
                                };

                                // Add the object to the list
                                securityReports.Add(securityReport);
                            }
                        }
                    }
                    myCon.Close();
                }

                // Return the list of SecurityReport objects as a JSON response
                return new JsonResult(securityReports);
            }




                [HttpPost]
                [Route("GenerateSecurityReportByDatesAndLocation")]
                public JsonResult GenerateSecurityReportByDatesAndLocation([FromBody] dynamic data)
                {
                    // Extracting the dates and location from the request body
                    DateTime startDate = Convert.ToDateTime(data.startDate);
                    DateTime endDate = Convert.ToDateTime(data.endDate);
                    string location = data.location;

                    // SQL query to fetch security reports within the specified date range and location
                    string query = "SELECT emp_id, log_date, log_time, event_description, event_location, severity_level, log_id " +
                                "FROM security_logs " +
                                "WHERE log_date BETWEEN @startDate AND @endDate AND event_location = @location";

                    // List to hold the results
                    List<SecurityReport> securityReports = new List<SecurityReport>();

                    // Get the connection string from appsettings.json
                    string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

                    // Execute the query and load the results
                    using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                    {
                        myCon.Open();
                        using (SqlCommand myCommand = new SqlCommand(query, myCon))
                        {
                            // Adding parameters to the command
                            myCommand.Parameters.AddWithValue("@startDate", startDate);
                            myCommand.Parameters.AddWithValue("@endDate", endDate);
                            myCommand.Parameters.AddWithValue("@location", location);

                            using (SqlDataReader myReader = myCommand.ExecuteReader())
                            {
                                while (myReader.Read())
                                {
                                    // Create a SecurityReport object and populate its properties
                                    SecurityReport securityReport = new SecurityReport
                                    {
                                        empID = Convert.ToInt32(myReader["emp_id"]),
                                        date = Convert.ToDateTime(myReader["log_date"]),
                                        time = myReader.GetTimeSpan(myReader.GetOrdinal("log_time")),
                                        eventDescription = myReader["event_description"].ToString(),
                                        location = myReader["event_location"].ToString(),
                                        severityLevel = myReader["severity_level"].ToString(),
                                        logID = Convert.ToInt32(myReader["log_id"])
                                    };

                                    // Add the object to the list
                                    securityReports.Add(securityReport);
                                }
                            }
                        }
                        myCon.Close();
                    }

                    // Return the list of SecurityReport objects as a JSON response
                    return new JsonResult(securityReports);
                }



        [HttpGet]
        [Route("viewTickets")]
        public JsonResult ViewTickets(int customerId)
        {
            // Prepare the SQL query for selecting tickets based on customer id
            string query = "SELECT ticket_id, visit_date, adult_tickets, child_tickets, senior_tickets, infant_tickets, total_cost FROM ticket_purchase WHERE customer_id = @customerId";

            // Create a new DataTable to store the result
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameter to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@customerId", customerId);

                    // Create a DataAdapter to fill the DataTable
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(myCommand);
                    dataAdapter.Fill(table);
                }
            }

            // Return the JSON representation of the DataTable
            return new JsonResult(table);
        }

        [HttpDelete]
        [Route("Animal/Delete")]
        public JsonResult DeleteAnimal(string animalName, string animalSpecies, string animalDoB)
        {
            // Prepare the SQL query for deleting an animal based on its name, species, and date of birth
            string query = "DELETE FROM animal WHERE animal_name = @animalName AND animal_species = @animalSpecies AND animal_dob = @animalDoB";

            // Create a new DataTable to store the result (though there won't be any result to store)
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@animalName", animalName);
                    myCommand.Parameters.AddWithValue("@animalSpecies", animalSpecies);
                    myCommand.Parameters.AddWithValue("@animalDoB", animalDoB);

                    // Execute the query (which is a DELETE operation)
                    int rowsAffected = myCommand.ExecuteNonQuery();

                    // If no rows were affected, the animal with the given name, species, and DoB was not found
                    if (rowsAffected == 0)
                    {
                        return new JsonResult("Animal not found");
                    }
                }
            }

            // Return a response indicating success
            return new JsonResult("Animal deleted successfully");
        }



        [HttpGet]
        [Route("Animal/Get")]
        public IActionResult GetAnimal(string animalName, string animalSpecies, string animalDoB)
        {
            // Prepare the SQL query to retrieve animal data based on its ID
            string query = "SELECT animal_id, animal_name, animal_species, animal_gender, animal_dob, animal_endangered, animal_origin, animal_doa FROM animal WHERE animal_name = @animalName AND animal_species = @animalSpecies AND animal_dob = @animalDoB";

            // Create a dictionary to store the retrieved animal data
            var animalData = new Dictionary<string, object>();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            try
            {
                // Open a connection to the database and execute the query
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@animalName", animalName);
                    myCommand.Parameters.AddWithValue("@animalSpecies", animalSpecies);
                    myCommand.Parameters.AddWithValue("@animalDoB", animalDoB);

                        using (SqlDataReader reader = myCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                animalData["animalID"] = reader["animal_id"];
                                animalData["animalName"] = reader["animal_name"];
                                animalData["animalSpecies"] = reader["animal_species"];
                                animalData["animalGender"] = reader["animal_gender"];
                                animalData["animalDoB"] = reader["animal_dob"];
                                animalData["animalEndangered"] = reader["animal_endangered"];
                                animalData["animalOrigin"] = reader["animal_origin"];
                                animalData["animalDoA"] = reader["animal_doa"];
                            }
                            else
                            {
                                return NotFound("Animal not found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error and return a meaningful error message
                // You may use your logging library here
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching animal data.");
            }

            // Return the data as JSON
            return Ok(animalData);
        }

        public class ModifyAnimalRequest
        {
            public Animal OriginalAnimal { get; set; }
            public Animal UpdatedAnimal { get; set; }
        }

        [HttpPut]
        [Route("Animal/Modify")]
        public JsonResult ModifyAnimal([FromBody] ModifyAnimalRequest request)
        {
            Animal originalAnimalData = request.OriginalAnimal;
            Animal updatedAnimalData = request.UpdatedAnimal;

            // Prepare the SQL query to update animal data
            string query = @"
            UPDATE animal
            SET
                animal_name = @newAnimalName,
                animal_species = @newAnimalSpecies,
                animal_gender = @newAnimalGender,
                animal_dob = @newAnimalDoB,
                animal_endangered = @newAnimalEndangered,
                animal_origin = @newAnimalOrigin
            WHERE animal_name = @originalAnimalName
                AND animal_species = @originalAnimalSpecies
                AND animal_dob = @originalAnimalDoB";

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            try
            {
                // Open a connection to the database and execute the query
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();

                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        // Parameters for the new (updated) data
                        myCommand.Parameters.AddWithValue("@newAnimalName", updatedAnimalData.animalName);
                        myCommand.Parameters.AddWithValue("@newAnimalSpecies", updatedAnimalData.animalSpecies);
                        myCommand.Parameters.AddWithValue("@newAnimalGender", updatedAnimalData.animalGender);
                        myCommand.Parameters.AddWithValue("@newAnimalDoB", updatedAnimalData.animalDoB);
                        myCommand.Parameters.AddWithValue("@newAnimalEndangered", updatedAnimalData.animalEndangered);
                        myCommand.Parameters.AddWithValue("@newAnimalOrigin", updatedAnimalData.animalOrigin);

                        // Parameters for matching the original data
                        myCommand.Parameters.AddWithValue("@originalAnimalName", originalAnimalData.animalName);
                        myCommand.Parameters.AddWithValue("@originalAnimalSpecies", originalAnimalData.animalSpecies);
                        myCommand.Parameters.AddWithValue("@originalAnimalDoB", originalAnimalData.animalDoB);

                        // Execute the update query
                        int rowsAffected = myCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { message = "Animal details updated successfully." });
                        }
                        else
                        {
                            // If no rows were affected, the animal was not found
                            return new JsonResult(new { message = "Animal not found.", status = 404 });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Handle SQL errors
                Console.WriteLine("SQL Error updating animal data: " + ex.Message);
                return new JsonResult(new { error = "SQL error occurred while updating animal data.", details = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other errors
                Console.WriteLine("Error updating animal data: " + ex.Message);
                return new JsonResult(new { error = "Failed to update animal data.", details = ex.Message });
            }
        }






        [HttpPost]
        [Route("SearchAnimal")]
        public JsonResult SearchAnimal([FromBody] dynamic data)
        {
            // Retrieve input fields from the request
            string animalName = data.animalName;
            string animalSpecies = data.animalSpecies;
            string animalGender = data.animalGender;
            DateTime? animalDoB = data.animalDoB;
            bool? animalEndangered = data.animalEndangered;
            DateTime? animalDoA = data.animalDoA;
            string animalOrigin = data.animalOrigin;

            // Initialize the base query
            string query = "SELECT * FROM animal WHERE 1=1"; // The `1=1` ensures the WHERE clause always evaluates to true

            // Create a dictionary to hold parameters and their values
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // Add conditions based on the input fields, if provided
            if (!string.IsNullOrEmpty(animalName))
            {
                query += " AND animal_name = @animalName";
                parameters.Add("@animalName", animalName);
            }

            if (!string.IsNullOrEmpty(animalSpecies))
            {
                query += " AND animal_species = @animalSpecies";
                parameters.Add("@animalSpecies", animalSpecies);
            }

            if (!string.IsNullOrEmpty(animalGender))
            {
                query += " AND animal_gender = @animalGender";
                parameters.Add("@animalGender", animalGender);
            }

            if (animalDoB.HasValue)
            {
                query += " AND animal_dob = @animalDoB";
                parameters.Add("@animalDoB", animalDoB.Value);
            }

            if (animalEndangered.HasValue)
            {
                query += " AND animal_endangered = @animalEndangered";
                parameters.Add("@animalEndangered", animalEndangered.Value);
            }

            if (animalDoA.HasValue)
            {
                query += " AND animal_doa = @animalDoA";
                parameters.Add("@animalDoA", animalDoA.Value);
            }

            if (!string.IsNullOrEmpty(animalOrigin))
            {
                query += " AND animal_origin = @animalOrigin";
                parameters.Add("@animalOrigin", animalOrigin);
            }

            // Create a list to hold the results
            List<Animal> animals = new List<Animal>();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                // Use a SqlCommand to execute the query with parameterization
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the SqlCommand object
                    foreach (var param in parameters)
                    {
                        myCommand.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    // Execute the query and load the results into a SqlDataReader
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            // Create a new Animal object and populate its properties
                            Animal animal = new Animal
                            {
                                animalID = Convert.ToInt32(myReader["animal_id"]),
                                animalName = myReader["animal_name"].ToString(),
                                animalSpecies = myReader["animal_species"].ToString(),
                                animalGender = Convert.ToChar(myReader["animal_gender"]),
                                animalDoB = Convert.ToDateTime(myReader["animal_dob"]),
                                animalEndangered = Convert.ToBoolean(myReader["animal_endangered"]),
                                animalDoA = Convert.ToDateTime(myReader["animal_doa"]),
                                animalOrigin = myReader["animal_origin"].ToString(),
                            };

                            // Add the Animal object to the list
                            animals.Add(animal);
                        }
                    }
                }

                // Close the connection
                myCon.Close();
            }

            // Return the list of Animal objects as a JSON response
            return new JsonResult(animals);
        }


        [HttpGet]
        [Route("Enclosure/Get")]
        public IActionResult GetEnclosure(string enclosureName, string enclosureType)
        {
            // Prepare the SQL query to retrieve animal data based on its ID
            string query = "SELECT enclosure_name, enclosure_type, built_date, cleaning_schedule_start, cleaning_schedule_end FROM enclosure WHERE enclosure_name = @enclosureName AND enclosure_type = @enclosureType";

            // Create a dictionary to store the retrieved animal data
            var enclosureData = new Dictionary<string, object>();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            try
            {
                // Open a connection to the database and execute the query
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        // Add parameters to the command to prevent SQL injection
                        myCommand.Parameters.AddWithValue("@enclosureName", enclosureName);
                        myCommand.Parameters.AddWithValue("@enclosureType", enclosureType);

                        using (SqlDataReader reader = myCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                enclosureData["enclosureName"] = reader["enclosure_name"];
                                enclosureData["enclosureType"] = reader["enclosure_type"];
                                enclosureData["builtDate"] = reader["built_date"];
                                enclosureData["cleaningScheduleStart"] = reader["cleaning_schedule_start"];
                                enclosureData["cleaningScheduleEnd"] = reader["cleaning_schedule_end"];
                            }
                            else
                            {
                                return NotFound("Enclosure not found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error and return a meaningful error message
                // You may use your logging library here
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching animal data.");
            }

            // Return the data as JSON
            return Ok(enclosureData);
        }


        public class ModifyEnclosureRequest
        {
            public Enclosure OriginalEnclosure { get; set; }
            public Enclosure UpdatedEnclosure { get; set; }
        }

        [HttpPut]
        [Route("Enclosure/Modify")]
        public JsonResult ModifyEnclosure([FromBody] ModifyEnclosureRequest request)
        {
            Enclosure originalEnclosureData = request.OriginalEnclosure;
            Enclosure updatedEnclosureData = request.UpdatedEnclosure;

            // Prepare the SQL query to update enclosure data
            string query = @"
        UPDATE enclosure
        SET
            enclosure_name = @newEnclosureName,
            enclosure_type = @newEnclosureType,
            built_date = @newBuiltDate,
            cleaning_schedule_start = @newCleaningScheduleStart,
            cleaning_schedule_end = @newCleaningScheduleEnd
        WHERE enclosure_name = @originalEnclosureName
            AND enclosure_type = @originalEnclosureType";

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            try
            {
                // Open a connection to the database and execute the query
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();

                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        // Parameters for the new (updated) data
                        myCommand.Parameters.AddWithValue("@newEnclosureName", updatedEnclosureData.enclosureName);
                        myCommand.Parameters.AddWithValue("@newEnclosureType", updatedEnclosureData.enclosureType);
                        myCommand.Parameters.AddWithValue("@newBuiltDate", updatedEnclosureData.builtDate);
                        myCommand.Parameters.AddWithValue("@newCleaningScheduleStart", updatedEnclosureData.cleaningScheduleStart);
                        myCommand.Parameters.AddWithValue("@newCleaningScheduleEnd", updatedEnclosureData.cleaningScheduleEnd);

                        // Parameters for matching the original data
                        myCommand.Parameters.AddWithValue("@originalEnclosureName", originalEnclosureData.enclosureName);
                        myCommand.Parameters.AddWithValue("@originalEnclosureType", originalEnclosureData.enclosureType);

                        // Execute the update query
                        int rowsAffected = myCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { message = "Enclosure details updated successfully." });
                        }
                        else
                        {
                            // If no rows were affected, the enclosure was not found
                            return new JsonResult(new { message = "Enclosure not found.", status = 404 });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Handle SQL errors
                Console.WriteLine("SQL Error updating enclosure data: " + ex.Message);
                return new JsonResult(new { error = "SQL error occurred while updating enclosure data.", details = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other errors
                Console.WriteLine("Error updating enclosure data: " + ex.Message);
                return new JsonResult(new { error = "Failed to update enclosure data.", details = ex.Message });
            }
        }


        [HttpPost]
        [Route("SearchEnclosure")]
        public JsonResult SearchEnclosure([FromBody] dynamic data)
        {
            // Retrieve input fields from the request
            string enclosureName = data.enclosureName;
            string enclosureType = data.enclosureType;
            DateTime? builtDate = data.builtDate;
            TimeSpan? cleaningScheduleStart = data.cleaningScheduleStart;
            TimeSpan? cleaningScheduleEnd = data.cleaningScheduleEnd;

            // Initialize the base query
            string query = "SELECT * FROM enclosure WHERE 1=1"; // The `1=1` ensures the WHERE clause always evaluates to true

            // Create a dictionary to hold parameters and their values
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // Add conditions based on the input fields, if provided
            if (!string.IsNullOrEmpty(enclosureName))
            {
                query += " AND enclosure_name = @enclosureName";
                parameters.Add("@enclosureName", enclosureName);
            }

            if (!string.IsNullOrEmpty(enclosureType))
            {
                query += " AND enclosure_type = @enclosureType";
                parameters.Add("@enclosureType", enclosureType);
            }

            if (builtDate.HasValue)
            {
                query += " AND built_date = @builtDate";
                parameters.Add("@builtDate", builtDate.Value);
            }

            if (cleaningScheduleStart.HasValue)
            {
                query += " AND cleaning_schedule_start = @cleaningScheduleStart";
                parameters.Add("@cleaningScheduleStart", cleaningScheduleStart.Value);
            }

            if (cleaningScheduleEnd.HasValue)
            {
                query += " AND cleaning_schedule_end = @cleaningScheduleEnd";
                parameters.Add("@cleaningScheduleEnd", cleaningScheduleEnd.Value);
            }

            // Create a list to hold the results
            List<Enclosure> enclosures = new List<Enclosure>();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                // Use a SqlCommand to execute the query with parameterization
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the SqlCommand object
                    foreach (var param in parameters)
                    {
                        myCommand.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    // Execute the query and load the results into a SqlDataReader
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            // Create a new Enclosure object and populate its properties
                            Enclosure enclosure = new Enclosure
                            {
                                enclosureName = myReader["enclosure_name"].ToString(),
                                enclosureType = myReader["enclosure_type"].ToString(),
                                builtDate = Convert.ToDateTime(myReader["built_date"]),
                                cleaningScheduleStart = TimeSpan.Parse(myReader["cleaning_schedule_start"].ToString()),
                                cleaningScheduleEnd = TimeSpan.Parse(myReader["cleaning_schedule_end"].ToString())
                            };

                            // Add the Enclosure object to the list
                            enclosures.Add(enclosure);
                        }
                    }
                }

                // Close the connection
                myCon.Close();
            }

            // Return the list of Enclosure objects as a JSON response
            return new JsonResult(enclosures);
        }


        [HttpDelete]
        [Route("Enclosure/Delete")]
        public JsonResult DeleteEnclosure(string enclosureName, string enclosureType)
        {
            // Prepare the SQL query for deleting an enclosure based on its name and type
            string query = "DELETE FROM enclosure WHERE enclosure_name = @enclosureName AND enclosure_type = @enclosureType";

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@enclosureName", enclosureName);
                    myCommand.Parameters.AddWithValue("@enclosureType", enclosureType);

                    // Execute the query (which is a DELETE operation)
                    int rowsAffected = myCommand.ExecuteNonQuery();

                    // If no rows were affected, the enclosure with the given name and type was not found
                    if (rowsAffected == 0)
                    {
                        return new JsonResult("Enclosure not found");
                    }
                }
            }

            // Return a response indicating success
            return new JsonResult("Enclosure deleted successfully");
        }


        [HttpGet]
        [Route("GetDiet")]
        public IActionResult GetDiet(string animalName, string animalSpecies, DateTime animalDoB)
        {
            // Prepare the SQL query to check for the existence of the animal
            string checkAnimalExistsQuery = "SELECT animal_id FROM animal WHERE animal_name = @animalName AND animal_species = @animalSpecies AND animal_DoB = @animalDoB";

            // Prepare the SQL query to retrieve diet information
            string retrieveDietQuery = "SELECT diet_name, diet_type, diet_schedule FROM diet WHERE animal_id = @animalID";

            // Get the database connection string
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            try
            {
                // Open a connection to the database
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();

                    // Check if the animal exists and retrieve animal_id
                    int animalId = 0;
                    using (SqlCommand checkAnimalCmd = new SqlCommand(checkAnimalExistsQuery, myCon))
                    {
                        // Add parameters to the command
                        checkAnimalCmd.Parameters.AddWithValue("@animalName", animalName);
                        checkAnimalCmd.Parameters.AddWithValue("@animalSpecies", animalSpecies);
                        checkAnimalCmd.Parameters.AddWithValue("@animalDoB", animalDoB);

                        // Execute the command and retrieve the animal ID
                        object result = checkAnimalCmd.ExecuteScalar();
                        if (result != null)
                        {
                            animalId = Convert.ToInt32(result);
                        }
                        else
                        {
                            // Return a not found response if the animal does not exist
                            return NotFound("No such animal found");
                        }
                    }

                    // Retrieve diet information for the animal
                    var dietData = new List<Dictionary<string, object>>();
                    using (SqlCommand retrieveDietCmd = new SqlCommand(retrieveDietQuery, myCon))
                    {
                        // Add parameters to the command
                        retrieveDietCmd.Parameters.AddWithValue("@animalID", animalId);

                        // Execute the command and read the data
                        using (SqlDataReader dietReader = retrieveDietCmd.ExecuteReader())
                        {
                            while (dietReader.Read())
                            {
                                var dietRecord = new Dictionary<string, object>
                        {
                            { "dietName", dietReader["diet_name"] },
                            { "dietType", dietReader["diet_type"] },
                            { "dietSchedule", dietReader["diet_schedule"] }
                        };

                                dietData.Add(dietRecord);
                            }
                        }
                    }

                    // Return the diet data as a JSON response
                    return Ok(dietData);
                }
            }
            catch (Exception ex)
            {
                // Log the error and return a meaningful error message
                // You can use your preferred logging library here
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving diet information.");
            }
        }


        [HttpGet]
        [Route("GetVetRecords")]
        public IActionResult GetVetRecords(string animalName, string animalSpecies, DateTime animalDoB)
        {
            // Prepare the SQL query to check for the existence of the animal
            string checkAnimalExistsQuery = "SELECT animal_id FROM animal WHERE animal_name = @animalName AND animal_species = @animalSpecies AND animal_DoB = @animalDoB";

            // Prepare the SQL query to retrieve vet records
            string retrieveVetQuery = "SELECT weight, height, diagnosis, medications FROM vet_records WHERE animal_id = @animalID";

            // Get the database connection string
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            try
            {
                // Open a connection to the database
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();

                    // Check if the animal exists and retrieve animal_id
                    int animalId = 0;
                    using (SqlCommand checkAnimalCmd = new SqlCommand(checkAnimalExistsQuery, myCon))
                    {
                        // Add parameters to the command
                        checkAnimalCmd.Parameters.AddWithValue("@animalName", animalName);
                        checkAnimalCmd.Parameters.AddWithValue("@animalSpecies", animalSpecies);
                        checkAnimalCmd.Parameters.AddWithValue("@animalDoB", animalDoB);

                        // Execute the command and retrieve the animal ID
                        object result = checkAnimalCmd.ExecuteScalar();
                        if (result != null)
                        {
                            animalId = Convert.ToInt32(result);
                        }
                        else
                        {
                            // Return a not found response if the animal does not exist
                            return NotFound("No such animal found");
                        }
                    }

                    // Retrieve vet records for the animal
                    var vetData = new List<Dictionary<string, object>>();
                    using (SqlCommand retrieveVetCmd = new SqlCommand(retrieveVetQuery, myCon))
                    {
                        // Add parameters to the command
                        retrieveVetCmd.Parameters.AddWithValue("@animalID", animalId);

                        // Execute the command and read the data
                        using (SqlDataReader vetReader = retrieveVetCmd.ExecuteReader())
                        {
                            while (vetReader.Read())
                            {
                                var vetRecord = new Dictionary<string, object>
                        {
                            { "weight", vetReader["weight"] },
                            { "height", vetReader["height"] },
                            { "diagnosis", vetReader["diagnosis"] },
                            { "medications", vetReader["medications"] }
                        };

                                vetData.Add(vetRecord);
                            }
                        }
                    }

                    // Return the vet data as a JSON response
                    return Ok(vetData);
                }
            }
            catch (Exception ex)
            {
                // Log the error and return a meaningful error message
                // You can use your preferred logging library here
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving vet information.");
            }
        }


        [HttpGet]
        [Route("GetAllEnclosures")]
        public JsonResult GetAllEnclosures()
        {
            // SQL query to retrieve all enclosures
            string query = "SELECT * FROM enclosure";

            // Create a DataTable to store the results
            DataTable table = new DataTable();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Execute the query and load the results into the DataTable
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    // Close the reader and connection
                    myReader.Close();
                    myCon.Close();
                }
            }

            // Return the DataTable as a JSON response
            return new JsonResult(table);
        }

            [HttpPut]
            [Route("Animal/Transfer")]
            public IActionResult TransferAnimalToEnclosure([FromBody] TransferRequest request)
            {
                // Prepare the SQL query for updating the animal's enclosure_id
                string query = "UPDATE animal SET enclosure_id = @NewEnclosureID WHERE animal_id = @AnimalID";

                // Get the connection string from appsettings.json
                string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

                try
                {
                    // Open a connection to the database and execute the query
                    using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                    {
                        myCon.Open();
                        using (SqlCommand myCommand = new SqlCommand(query, myCon))
                        {
                            // Add parameters to the command to prevent SQL injection
                            myCommand.Parameters.AddWithValue("@NewEnclosureID", request.EnclosureID);
                            myCommand.Parameters.AddWithValue("@AnimalID", request.AnimalID);

                            // Execute the query (UPDATE operation)
                            int rowsAffected = myCommand.ExecuteNonQuery();

                            // Check if the update was successful (at least one row affected)
                            if (rowsAffected > 0)
                            {
                                // Return a success response
                                return Ok("Animal transferred successfully.");
                            }
                            else
                            {
                                // Return a not found response if no rows were affected
                                return NotFound("Animal not found.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error and return a meaningful error message
                    // You may use your logging library here
                    Console.WriteLine($"Error: {ex.Message}");
                    return StatusCode(500, "An error occurred while transferring the animal.");
                }
            }

            // Define a model to receive the transfer request
            public class TransferRequest
            {
                public int AnimalID { get; set; }
                public int EnclosureID { get; set; }
            }

        [HttpPost]
        [Route("GenerateEnclosureReport")]
        public JsonResult GenerateEnclosureReport([FromBody] EnclosureReportRequest request)
        {
            // Access filter parameters from the request
            string enclosureName = request.EnclosureName;
            string enclosureType = request.EnclosureType;
            DateTime? dateRangeStart = request.DateRangeStart;
            DateTime? dateRangeEnd = request.DateRangeEnd;
            TimeSpan? timeRangeStart = request.TimeRangeStart;
            TimeSpan? timeRangeEnd = request.TimeRangeEnd;

            // Prepare the query with filtering conditions
            StringBuilder queryBuilder = new StringBuilder("SELECT enclosure_id, enclosure_name, enclosure_type, built_date, cleaning_schedule_start, cleaning_schedule_end FROM enclosure WHERE 1=1");

            // Add filtering conditions based on the provided parameters
            if (!string.IsNullOrEmpty(enclosureName))
            {
                queryBuilder.Append(" AND enclosure_name LIKE @enclosureName");
            }

            if (!string.IsNullOrEmpty(enclosureType))
            {
                queryBuilder.Append(" AND enclosure_type = @enclosureType");
            }

            if (dateRangeStart.HasValue)
            {
                queryBuilder.Append(" AND built_date >= @dateRangeStart");
            }

            if (dateRangeEnd.HasValue)
            {
                queryBuilder.Append(" AND built_date <= @dateRangeEnd");
            }

            if (timeRangeStart.HasValue)
            {
                queryBuilder.Append(" AND cleaning_schedule_start >= @timeRangeStart");
            }

            if (timeRangeEnd.HasValue)
            {
                queryBuilder.Append(" AND cleaning_schedule_end <= @timeRangeEnd");
            }

            string query = queryBuilder.ToString();

            // Create a list to hold the results
            List<Enclosure> enclosures = new List<Enclosure>();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                // Use a SqlCommand to execute the query with parameterization
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    if (!string.IsNullOrEmpty(enclosureName))
                    {
                        myCommand.Parameters.AddWithValue("@enclosureName", $"%{enclosureName}%");
                    }

                    if (!string.IsNullOrEmpty(enclosureType))
                    {
                        myCommand.Parameters.AddWithValue("@enclosureType", enclosureType);
                    }

                    if (dateRangeStart.HasValue)
                    {
                        myCommand.Parameters.AddWithValue("@dateRangeStart", dateRangeStart.Value);
                    }

                    if (dateRangeEnd.HasValue)
                    {
                        myCommand.Parameters.AddWithValue("@dateRangeEnd", dateRangeEnd.Value);
                    }

                    if (timeRangeStart.HasValue)
                    {
                        myCommand.Parameters.AddWithValue("@timeRangeStart", timeRangeStart.Value);
                    }

                    if (timeRangeEnd.HasValue)
                    {
                        myCommand.Parameters.AddWithValue("@timeRangeEnd", timeRangeEnd.Value);
                    }

                    // Execute the query and load the results into a SqlDataReader
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            // Create a new Enclosure object and populate its properties
                            Enclosure enclosure = new Enclosure
                            {
                                enclosureID = Convert.ToInt32(myReader["enclosure_id"]),
                                enclosureName = myReader["enclosure_name"].ToString(),
                                enclosureType = myReader["enclosure_type"].ToString(),
                                builtDate = Convert.ToDateTime(myReader["built_date"]),
                                cleaningScheduleStart = (TimeSpan)myReader["cleaning_schedule_start"],
                                cleaningScheduleEnd = (TimeSpan)myReader["cleaning_schedule_end"]
                            };

                            // Add the Enclosure object to the list
                            enclosures.Add(enclosure);
                        }
                    }
                }
            }

            // Return the list of Enclosure objects as a JSON response
            return new JsonResult(enclosures);
        }


        [HttpGet]
        [Route("FetchAnimalsForEnclosure/{enclosureID}")]
        public JsonResult FetchAnimalsForEnclosure(int enclosureID)
        {
            // Create a list to hold the results
            List<Animal> animals = new List<Animal>();

            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

            // Define the query to fetch animals from the specified enclosure
            string query = "SELECT * FROM animal WHERE enclosure_id = @enclosureID";

            // Open a connection to the database
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                // Use a SqlCommand to execute the query with parameterization
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    myCommand.Parameters.AddWithValue("@enclosureID", enclosureID);

                    // Execute the query and load the results into a SqlDataReader
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            // Create a new Animal object and populate its properties
                            Animal animal = new Animal
                            {
                                animalID = Convert.ToInt32(myReader["animal_id"]),
                                enclosureID = Convert.ToInt32(myReader["enclosure_id"]),
                                animalName = myReader["animal_name"].ToString(),
                                animalSpecies = myReader["animal_species"].ToString(),
                                animalGender = Convert.ToChar(myReader["animal_gender"]),
                                animalDoB = Convert.ToDateTime(myReader["animal_dob"]),
                                animalEndangered = Convert.ToBoolean(myReader["animal_endangered"]),
                                animalDoA = Convert.ToDateTime(myReader["animal_doa"]),
                                animalOrigin = myReader["animal_origin"].ToString()
                            };

                            // Add the Animal object to the list
                            animals.Add(animal);
                        }
                    }
                }
            }

            // Return the list of Animal objects
            return new JsonResult(animals);
        }

        [HttpGet]
        [Route("GetUniqueEnclosureTypes")]
        public IActionResult GetUniqueEnclosureTypes()
        {
            // Define a list to store the unique enclosure types
            List<string> uniqueEnclosureTypes = new List<string>();

            // Retrieve the connection string from configuration
            string connectionString = _configuration.GetConnectionString("ZooDBConnection");

            // Define the SQL query to fetch unique enclosure types from the enclosure table
            string query = "SELECT DISTINCT enclosure_type FROM enclosure";

            // Create a SqlConnection using the connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create a SqlCommand to execute the query
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    // Open the connection
                    connection.Open();

                    // Execute the query and retrieve the results using a SqlDataReader
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Iterate through the results and add each unique enclosure type to the list
                        while (reader.Read())
                        {
                            string enclosureType = reader["enclosure_type"].ToString();
                            uniqueEnclosureTypes.Add(enclosureType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the detailed exception
                    Console.Error.WriteLine($"Error while retrieving unique enclosure types: {ex.Message}");
                    return StatusCode(500, "An error occurred while retrieving unique enclosure types.");
                }
            }

            // Return the list of unique enclosure types as a JSON response
            return Ok(uniqueEnclosureTypes);
        }






    }

}