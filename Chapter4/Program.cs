using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Chapter4
{
    class Program
    {
        static void Main(string[] args)
        {
            Person p = new Person
            {
                Id = 1,
                Name = "John Doe"
            };

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream("data.bin", FileMode.Create))
            {
                formatter.Serialize(stream, p);
            }

            using (Stream stream = new FileStream("data.bin", FileMode.Open))
            {
                Person dp = (Person)formatter.Deserialize(stream);
            }
            Console.ReadLine();
        }

        private static Order CreateOrder()
        {
            Product p1 = new Product { ID = 1, Description = "p2", Price = 9 };
            Product p2 = new Product { ID = 2, Description = "p3", Price = 6 };

            Order order = new VIPOrder
            {
                ID = 4,
                Description = "Order for John Doe. Use the nice giftwrap",
                OrderLines = new List<OrderLine>
                {
                    new OrderLine { ID = 5, Amount = 1, Product = p1},
                    new OrderLine { ID = 6,Amount = 10, Product = p2},
                }
            };

            return order;

        }

        private static void ListDirectories(DirectoryInfo directoryInfo,
            string searchPattern, int maxLevel, int currentLevel)
        {
            if (currentLevel >= maxLevel)
            {
                return;
            }

            string indent = new string('-', currentLevel);

            try
            {
                DirectoryInfo[] subDirectories = directoryInfo.GetDirectories(searchPattern);

                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    Console.WriteLine(indent + subDirectory.Name);
                    ListDirectories(subDirectory, searchPattern, maxLevel, currentLevel + 1);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // You don't have access to his folder.
                Console.WriteLine(indent + "Can't access: " + directoryInfo.Name);
            }
            catch (DirectoryNotFoundException)
            {
                // The folder is removed while iterating
                Console.WriteLine(indent + "Can't find: " + directoryInfo.Name);
                return;
            }
        }

        private static string ReadAllText()
        {
            string path = @"C:\temp\test.txt";

            try
            {
                return File.ReadAllText(path);
            }
            catch (DirectoryNotFoundException) { }
            catch (FileNotFoundException) { }

            return string.Empty;
        }

        public async Task CreateAndWriteAsyncToFile()
        {
            using (FileStream stream = new FileStream("test.dat", FileMode.Create,
                FileAccess.Write, FileShare.None, 4096, true))
            {
                byte[] data = new byte[100000];
                new Random().NextBytes(data);

                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        public async Task ReadAsyncHttpRequest()
        {
            HttpClient client = new HttpClient();
            string result = await client.GetStringAsync("http://www.microsoft.com");
        }

        public async Task ExecuteMultipleRequests()
        {
            HttpClient client = new HttpClient();

            string microsoft = await client.GetStringAsync("http://www.microsoft.com");
            string msdn = await client.GetStringAsync("http://msdn.microsoft.com");
            string blogs = await client.GetStringAsync("http://blogs.msdn.com/");
        }

        public async Task ExecuteMultipleRequestsInParallel()
        {
            HttpClient client = new HttpClient();

            Task microsoft = client.GetStringAsync("http://www.microsoft.com");
            Task msdn = client.GetStringAsync("http://msdn.microsoft.com");
            Task blogs = client.GetStringAsync("http://blogs.msdn.com/");

            await Task.WhenAll(microsoft, msdn, blogs);
        }

        public async Task SelectDataFromTable()
        {
            string connectionString = ConfigurationManager.
                ConnectionStrings["ProgrammingInCSharpConnection"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM People", connection);
                await connection.OpenAsync();

                SqlDataReader dataReader = await command.ExecuteReaderAsync();

                while (await dataReader.ReadAsync())
                {
                    string formatStringWithMiddleName = "Person ({0}) is named {1} {2} {3}";
                    string formatStringWithoutMiddleName = "Person ({0}) is named {1} {3}";

                    if ((dataReader["middlename"] == null))
                    {
                        Console.WriteLine(formatStringWithoutMiddleName,
                            dataReader["id"],
                            dataReader["firstname"],
                            dataReader["lastname"]);
                    }
                    else
                    {
                        Console.WriteLine(formatStringWithMiddleName,
                            dataReader["id"],
                            dataReader["firstname"],
                            dataReader["middlename"],
                            dataReader["lastname"]);
                    }
                }
                dataReader.Close();
            }
        }

        public async Task SelectMultipleResultSets()
        {
            string connectionString = ConfigurationManager.
                ConnectionStrings["ProgrammingInCSharpConnection"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM PEOPLE;" +
                    "SELECT TOP 1 * FROM People ORDER BY LastName", connection);
                await connection.OpenAsync();
                SqlDataReader dataReader = await command.ExecuteReaderAsync();
                await ReadQueryResults(dataReader);
            }
        }
        private static async Task ReadQueryResults(SqlDataReader dataReader)
        {
            while (await dataReader.ReadAsync())
            {
                string formatStringWithMiddleName = "Person ({0}) is named {1} {2} {3}";
                string formatStringWithoutMiddleName = "Person ({0}) is named {1} {3}";
                if ((dataReader["middlename"] == null))
                {
                    Console.WriteLine(formatStringWithoutMiddleName,
                        dataReader["id"],
                        dataReader["firstname"],
                        dataReader["lastname"]);
                }
                else
                {
                    Console.WriteLine(formatStringWithMiddleName,
                        dataReader["id"],
                        dataReader["firstname"],
                        dataReader["middlename"],
                        dataReader["lastname"]);
                }
            }
        }

        public async Task UpdateRows()
        {
            string connectionString = ConfigurationManager.
                ConnectionStrings["ProgrammingInCSharpConnection"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(
                    "UPDATE People SET FirstName='John'",
                    connection);

                await connection.OpenAsync();
                int numberOfUpdatedRows = await command.ExecuteNonQueryAsync();
                Console.WriteLine("Updated {0} rows", numberOfUpdatedRows);
            }
        }

        public async Task InsertRowWithParametrizedQuery()
        {
            string connectionString = ConfigurationManager.
                ConnectionStrings["ProgrammingInCSharpConnection"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(
                    "INSERT INTO People([FirstName], [LastName], [MiddleName]) VALUES(@" +
                    "firstname, @lastname, @middleName)",
                    connection);
                await connection.OpenAsync();

                command.Parameters.AddWithValue("@firstName", "John");
                command.Parameters.AddWithValue("@lastName", "Doe");
                command.Parameters.AddWithValue("@middleName", "Little");

                int numberOfInsertedRows = await command.ExecuteNonQueryAsync();
                Console.WriteLine("Inserted {0} rows", numberOfInsertedRows);
            }
        }
    }

    public class Product
    {
        [XmlAttribute]
        public int ID { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }

    [Serializable]
    public class OrderLine
    {
        [XmlAttribute]
        public int ID { get; set; }

        [XmlAttribute]
        public int Amount { get; set; }

        [XmlElement("OrderedProduct")]
        public Product Product { get; set; }
    }

    [Serializable]
    public class Order
    {
        [XmlAttribute]
        public int ID { get; set; }

        [XmlIgnore]
        public bool IsDirty { get; set; }

        [XmlArray("Lines")]
        [XmlArrayItem("OrderLine")]
        public List<OrderLine> OrderLines { get; set;  }
    }

    [Serializable]
    public class VIPOrder : Order
    {
        public string Description { get; set; }
    }

    public static class LinqExtensions
    {
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
            }
        }
    }

    [Serializable]
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        private bool isDirty = false;
    }
}
