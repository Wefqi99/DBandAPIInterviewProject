using DBandAPIInterviewProject.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace DBandAPIInterviewProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;

        string url = "https://holidays.abstractapi.com/v1/";
        string starWars = "https://swapi.dev/api/people";

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {

            DataTable dataTable = await GetStarWarsData(starWars);

            PrintDataTable(dataTable);

            return View(dataTable);
        }

        public IActionResult Privacy()
        {
            //Replace DB Connectin string here
            string connectionString = "server=DESKTOP-CHS98C4;uid=secondUser;pwd=Silwad-14113;database=students;";
            //Replace name of table
            string tableName = "studentInformation";

            DataTable dataTable = GetTableData(connectionString, tableName);

            Console.WriteLine("Column Names:");
            foreach (DataColumn column in dataTable.Columns)
            {
                Console.Write(column.ColumnName);
            }

            Console.WriteLine();

            Console.WriteLine("Data:");
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    Console.Write(row[column]);
                }
                Console.WriteLine();
            }

            return View(dataTable);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        static async Task<string> GetApiResponse(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {

                    HttpResponseMessage response = await client.GetAsync(apiUrl);


                    if (response.IsSuccessStatusCode)
                    {

                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    return null;
                }
            }
        }

        static DataTable GetTableData(string connectionString, string tableName)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Fetch data
                using (MySqlCommand dataCmd = new MySqlCommand($"SELECT * FROM {tableName}", connection))
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(dataCmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        static async Task<DataTable> GetStarWarsData(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject json = JsonConvert.DeserializeObject<JObject>(data);
                    JArray results = json.Value<JArray>("results");

                    return ConvertToDataTable(results);
                }
                else
                {
                    throw new Exception($"Failed to retrieve data. Status code: {response.StatusCode}");
                }
            }
        }

        static DataTable ConvertToDataTable(JArray jsonArray)
        {
            DataTable dataTable = new DataTable();

            foreach (JProperty property in jsonArray.First().Children<JProperty>())
            {
                dataTable.Columns.Add(property.Name, typeof(string));
            }

            foreach (JObject item in jsonArray)
            {
                DataRow row = dataTable.NewRow();
                foreach (JProperty property in item.Children<JProperty>())
                {
                    row[property.Name] = property.Value.ToString();
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        static void PrintDataTable(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    Console.Write($"{col.ColumnName}: {row[col]}   ");
                }
                Console.WriteLine();
            }
        }
    }
}

