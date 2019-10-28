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
using System.Security.Permissions;
using System.Collections;

namespace Chapter4
{
    class Program
    {
        static void Main(string[] args)
        {
            Person p1 = new Person { Id = 1, Name = "Name1" };
            Person p2 = new Person { Id = 2, Name = "Name2" };
            Person p3 = new Person { Id = 3, Name = "Name3" };

            var dict = new Dictionary<int, Person>();
            dict.Add(p1.Id, p1);
            dict.Add(p2.Id, p2);
            dict.Add(p3.Id, p3);

            foreach( KeyValuePair<int, Person> v in dict)
            {
                Console.WriteLine("{0}: {1}", v.Key, v.Value.Name);
            }

            dict[0] = new Person { Id = 4, Name = "Name4" };
            Person result;
            if (!dict.TryGetValue(5, out result))
            {
                Console.WriteLine("No person with a key of 5 can be found");
            }
            Console.ReadLine();
        }

        public interface IList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
        {
            T this[int index] { get; set; }
            int IndexOf(T item);
            void Insert(int index, T item);
            void RemoveAt(int index);
        }

        public interface ICollection<T> : IEnumerable<T>, IEnumerable
        {
            int Count { get; }
            bool IsReadOnly { get; }
            void Add(T item);
            void Clear();
            bool Contains(T item);
            void CopyTo(T[] array, int arrayIndex);
            bool Remove(T item);
        }

        [Serializable]
        public class PersonComplex : ISerializable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            private bool isDirty = false;

            public PersonComplex() { }
            protected PersonComplex(SerializationInfo info, StreamingContext context)
            {
                Id = info.GetInt32("Value1");
                Name = info.GetString("Value2");
                isDirty = info.GetBoolean("Value3");
            }

            [System.Security.Permissions.SecurityPermission(SecurityAction.Demand,
                                                            SerializationFormatter = true)]
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Value1", Id);
                info.AddValue("Value2", Name);
                info.AddValue("Value3", isDirty);
            }

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

        [NonSerialized]
        private bool isDirty = false;

        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {
            Console.WriteLine("OnSerializing");
        }

        [OnSerialized()]
        internal void OnSerializedMethod(StreamingContext context)
        {
            Console.WriteLine("OnSerialized");
        }

        [OnDeserializing()]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            Console.WriteLine("OnDeserializing");
        }

        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Console.WriteLine("OnDeserialized");
        }
    }
}
