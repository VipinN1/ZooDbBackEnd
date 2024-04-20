using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using BackEnd.Models;
using System;



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
      // Prepare the SQL query for inserting a new user
      string query = @"INSERT INTO customer (first_name, last_name, phone_number, email, address, zip_code, date_of_birth) 
                   VALUES (@firstName, @lastName, @phoneNumber, @email, @address, @zipCode, @formattedDate)";

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
              myCommand.Parameters.AddWithValue("@address", userProfile.address);
              myCommand.Parameters.AddWithValue("@zipCode", userProfile.zipCode);
              myCommand.Parameters.AddWithValue("@formattedDate", userProfile.formattedDate);

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
                string checkAnimalExistsQuery = "SELECT animal_id FROM animal WHERE animal_species = @animalSpecies AND animal_DoB = @animalDoB";
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






        [HttpPost]
        [Route("NewVetRecords")]
        public JsonResult NewVetRecords([FromBody] VetRecords newVetRecords)
        {
            // Prepare the SQL query for inserting a new user
            string checkAnimalExistsQuery1 = "SELECT animal_id from animal WHERE animal_species = @animalSpecies AND animal_DoB = @animalDoB";
            string insertVetQuery = "INSERT INTO vet_records (animal_id,weight,height,diagnosis,medications) VALUES (@animalID, @weight, @height, @diagnosis, @medications)";
            string updateVetQuery = "UPDATE vet_records SET weight = @weight, height = @height, diagnosis = @diagnosis, medications = @medications WHERE animal_id = @animalID";


            // Get the connection string from appsettings.json
            string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");
            

            bool vetExists = false;

            
            // Open a connection to the database and execute the query
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {

                myCon.Open();
                int animalId = 0;
                using (SqlCommand checkAnimalCmd = new SqlCommand(checkAnimalExistsQuery1, myCon))
                {
                    // Add parameters to the command to prevent SQL injection
                    checkAnimalCmd.Parameters.AddWithValue("@animalSpecies", newVetRecords.animalSpecies);
                    checkAnimalCmd.Parameters.AddWithValue("@animalDoB", newVetRecords.animalDoB);

                        object result = checkAnimalCmd.ExecuteScalar();  // Use ExecuteScalar to get the first column of the first row
                        if (result != null)
                            animalId = Convert.ToInt32(result);
                        else
                            return new JsonResult("No such animal found");
                }

                
                    // Check if vet entry exists for this animal_id
                    using (SqlCommand checkVetCmd = new SqlCommand("SELECT COUNT(1) FROM vet_records WHERE animal_id = @animalID", myCon))
                    {
                        checkVetCmd.Parameters.AddWithValue("@animalID", animalId);
                        vetExists = (int)checkVetCmd.ExecuteScalar() > 0;
                    }

                    // Insert or update vet information
                    using (SqlCommand vetCmd = new SqlCommand(vetExists ? updateVetQuery : insertVetQuery, myCon))
                    {
                        vetCmd.Parameters.AddWithValue("@animalID", animalId);
                        vetCmd.Parameters.AddWithValue("@weight", newVetRecords.weight);
                        vetCmd.Parameters.AddWithValue("@height", newVetRecords.height);
                        vetCmd.Parameters.AddWithValue("@medications", newVetRecords.medications);
                        vetCmd.Parameters.AddWithValue("@diagnosis",newVetRecords.diagnosis);

                        vetCmd.ExecuteNonQuery();  // Execute either update or insert
                    }
            }

            // Return a response indicating success
            return new JsonResult(vetExists ? "Vet updated successfully" : "New vet added successfully");
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
            string query = "INSERT INTO animal (animal_name, animal_species, animal_gender, animal_DoB, animal_endangered, animal_DoA, animal_origin, life_stage) VALUES (@animalName, @animalSpecies, @animalGender, @animalDoB, @animalEndangered, @animalDoA, @animalOrigin, @lifeStage)";

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
                    myCommand.Parameters.AddWithValue("@lifeStage", newAnimal.animalLifeStage);




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
            [Route("GenerateAnimalReport")]
            public JsonResult GenerateAnimalReport([FromBody] dynamic data)
            {
                string animalSpecies = data.animalSpecies;
                // Define the query to fetch animal data based on the given animal species
                string query = "SELECT * FROM animal WHERE animal_species = @animalSpecies";

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
                        // Add the animal species parameter to the command
                        myCommand.Parameters.AddWithValue("@animalSpecies", animalSpecies);

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
                                    animalLifeStage = myReader["life_stage"].ToString(),
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


            public class EnclosureTypeRequest
            {
                public string EnclosureType { get; set; }
            }

            [HttpPost]
            [Route("GenerateEnclosureReport")]
            public JsonResult GenerateEnclosureReport([FromBody] EnclosureTypeRequest request)
            {
                // Access request.EnclosureType instead of data.enclosureType
                string enclosureType = request.EnclosureType;

                // Define the query to fetch enclosure data based on the given enclosure type
                string query = "SELECT * FROM enclosure WHERE enclosure_type = @enclosureType";

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
                        // Add the enclosure type parameter to the command
                        myCommand.Parameters.AddWithValue("@enclosureType", enclosureType);

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
                                    cleaningScheduleStart = myReader.GetTimeSpan(myReader.GetOrdinal("cleaning_schedule_start")),
                                    cleaningScheduleEnd = myReader.GetTimeSpan(myReader.GetOrdinal("cleaning_schedule_end"))
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





    [HttpGet]
   [Route("GetEmployeeProfile")]
   public JsonResult GetEmployeeProfile([FromQuery] int employeeId)
   {
       // Prepare the SQL query for retrieving employee data
       string query = @"SELECT last_name, first_name, hire_date, emp_DoB, salary, email 
                FROM employee WHERE emp_Id = @empId";

       // Get the connection string from appsettings.json
       string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

       EmployeeProfile employeeProfile = null;

       // Open a connection to the database
       using (SqlConnection myCon = new SqlConnection(sqlDataSource))
       {
           myCon.Open();
           using (SqlCommand myCommand = new SqlCommand(query, myCon))
           {
               // Add parameter to the command to prevent SQL injection
               myCommand.Parameters.AddWithValue("@empId", employeeId);

               using (SqlDataReader reader = myCommand.ExecuteReader())
               {
                   // Check if an employee was found
                   if (reader.Read())
                   {
                       employeeProfile = new EmployeeProfile
                       {
                           lastName = reader["last_name"] as string ?? "",
                           firstName = reader["first_name"] as string ?? "",
                           hireDate = reader["hire_date"] != DBNull.Value ? Convert.ToDateTime(reader["hire_date"]) : default(DateTime),
                           dob = reader["emp_DoB"] != DBNull.Value ? Convert.ToDateTime(reader["emp_DoB"]) : default(DateTime),
                           salary = reader["salary"] != DBNull.Value ? Convert.ToDecimal(reader["salary"]) : 0,
                           email = reader["email"] as string ?? ""
                       };
                   }
               }
           }
       }

       // Check if employee data was found and return appropriate result
       if (employeeProfile != null)
       {
           return new JsonResult(employeeProfile);
       }
       else
       {
           return new JsonResult("Employee not found");
       }
   }


    [HttpGet]
   [Route("GetUserProfile")]
   public JsonResult GetUserProfile([FromQuery] int customerId)
   {
       // Prepare the SQL query for retrieving user data
       string query = @"SELECT first_name, last_name, phone_number, email, address, zip_code, date_of_birth 
            FROM customer WHERE customer_Id = @customerId";


       // Get the connection string from appsettings.json
       string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");

       UserProfile userProfile = null;

       // Open a connection to the database
       using (SqlConnection myCon = new SqlConnection(sqlDataSource))
       {
           myCon.Open();
           using (SqlCommand myCommand = new SqlCommand(query, myCon))
           {
               // Add parameter to the command to prevent SQL injection
               // Correct the parameter name from @userId to @customerId
               myCommand.Parameters.AddWithValue("@customerId", customerId);

               using (SqlDataReader reader = myCommand.ExecuteReader())
               {
                   // Check if a user was found
                   if (reader.Read())
                   {
                       userProfile = new UserProfile
                       {
                           firstName = reader["first_name"].ToString(),
                           lastName = reader["last_name"].ToString(),
                           phoneNumber = reader["phone_number"].ToString(),
                           email = reader["email"].ToString(),
                           address = reader["address"].ToString(),
                           zipCode = reader["zip_code"].ToString(),
                           formattedDate = Convert.ToDateTime(reader["date_of_birth"])
                       };
                   }
               }
           }

       }

       // Check if user data was found and return appropriate result
       if (userProfile != null)
       {
           return new JsonResult(userProfile);
       }
       else
       {
           return new JsonResult("User not found");
       }
   }


    [HttpPut]
  [Route("UpdateUserProfile/{customerId}")]
  public JsonResult UpdateUserProfile(int customerId, [FromBody] UserProfile userProfile)
  {
      string sqlDataSource = _configuration.GetConnectionString("ZooDBConnection");
      string updateQuery = @"
      UPDATE customer 
      SET 
          first_name = COALESCE(@firstName, first_name), 
          last_name = COALESCE(@lastName, last_name),
          phone_number = COALESCE(@phoneNumber, phone_number),
          address = COALESCE(@address, address),
          zip_code = COALESCE(@zipCode, zip_code),
          date_of_birth = COALESCE(@formattedDate, date_of_birth)
      WHERE customer_id = @customerId";

      using (SqlConnection con = new SqlConnection(sqlDataSource))
      {
          con.Open();
          using (SqlCommand command = new SqlCommand(updateQuery, con))
          {
              command.Parameters.AddWithValue("@customerId", customerId);
              command.Parameters.AddWithValue("@firstName", (object)userProfile.firstName ?? DBNull.Value);
              command.Parameters.AddWithValue("@lastName", (object)userProfile.lastName ?? DBNull.Value);
              command.Parameters.AddWithValue("@phoneNumber", (object)userProfile.phoneNumber ?? DBNull.Value);
              command.Parameters.AddWithValue("@address", (object)userProfile.address ?? DBNull.Value);
              command.Parameters.AddWithValue("@zipCode", (object)userProfile.zipCode ?? DBNull.Value);
              command.Parameters.AddWithValue("@formattedDate", (object)userProfile.formattedDate ?? DBNull.Value);

              int effectedRows = command.ExecuteNonQuery();
              if (effectedRows > 0)
              {
                  return new JsonResult("Profile updated successfully");
              }
              else
              {
                  return new JsonResult("No profile found or no changes detected");
              }
          }
      }
  }




   








    }

}