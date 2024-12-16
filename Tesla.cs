using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace TeslaRental
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.Initialize(); // Создаем таблицы при запуске
            Console.WriteLine("Tesla Rental Backend initialized.");

            // Добавляем машину
            Console.WriteLine("Adding a new car...");
            Car.Add("Tesla Model S", 20.0, 0.5); // Часовая ставка: $20, км ставка: $0.5

            // Добавляем клиента
            Console.WriteLine("Registering a new client...");
            Client.Register("John Doe", "john.doe@example.com");

            // Получаем ID клиента по Email
            var client = Client.GetByEmail("john.doe@example.com");
            Console.WriteLine($"Client ID: {client.ID}, Name: {client.Name}");

            // Получаем первую машину из базы
            var cars = Car.GetAll();
            var firstCar = cars[0];
            Console.WriteLine($"Car ID: {firstCar.ID}, Model: {firstCar.Model}");

            // Начинаем аренду
            Console.WriteLine("Starting rental...");
            Rental.StartRental(client.ID, firstCar.ID);

            Console.WriteLine("Rental started successfully!");
        }
    }

    static class Database
    {
        private const string ConnectionString = "Data Source=tesla_rental.db;Version=3;";

        public static void Initialize()
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            string createCarsTable = @"CREATE TABLE IF NOT EXISTS Cars (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Model TEXT NOT NULL,
                HourlyRate REAL NOT NULL,
                KilometerRate REAL NOT NULL
            );";

            string createClientsTable = @"CREATE TABLE IF NOT EXISTS Clients (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE
            );";

            string createRentalsTable = @"CREATE TABLE IF NOT EXISTS Rentals (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                ClientID INTEGER NOT NULL,
                CarID INTEGER NOT NULL,
                StartTime TEXT NOT NULL,
                EndTime TEXT,
                KilometersDriven REAL,
                TotalCost REAL,
                FOREIGN KEY(ClientID) REFERENCES Clients(ID),
                FOREIGN KEY(CarID) REFERENCES Cars(ID)
            );";

            ExecuteCommand(connection, createCarsTable);
            ExecuteCommand(connection, createClientsTable);
            ExecuteCommand(connection, createRentalsTable);
        }

        private static void ExecuteCommand(SQLiteConnection connection, string commandText)
        {
            using var command = new SQLiteCommand(commandText, connection);
            command.ExecuteNonQuery();
        }

        public static SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }

    class Car
    {
        public int ID { get; set; }
        public string Model { get; set; }
        public double HourlyRate { get; set; }
        public double KilometerRate { get; set; }

        public static List<Car> GetAll()
        {
            using var connection = Database.GetConnection();
            string query = "SELECT * FROM Cars;";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            var cars = new List<Car>();
            while (reader.Read())
            {
                cars.Add(new Car
                {
                    ID = reader.GetInt32(0),
                    Model = reader.GetString(1),
                    HourlyRate = reader.GetDouble(2),
                    KilometerRate = reader.GetDouble(3)
                });
            }
            return cars;
        }

        public static void Add(string model, double hourlyRate, double kilometerRate)
        {
            using var connection = Database.GetConnection();
            string query = "INSERT INTO Cars (Model, HourlyRate, KilometerRate) VALUES (@Model, @HourlyRate, @KilometerRate);";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Model", model);
            command.Parameters.AddWithValue("@HourlyRate", hourlyRate);
            command.Parameters.AddWithValue("@KilometerRate", kilometerRate);
            command.ExecuteNonQuery();
        }
    }

    class Client
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public static void Register(string name, string email)
        {
            using var connection = Database.GetConnection();
            string query = "INSERT INTO Clients (Name, Email) VALUES (@Name, @Email);";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Email", email);
            command.ExecuteNonQuery();
        }

        public static Client GetByEmail(string email)
        {
            using var connection = Database.GetConnection();
            string query = "SELECT * FROM Clients WHERE Email = @Email;";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Client
                {
                    ID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2)
                };
            }
            return null;
        }
    }

    class Rental
    {
        public int ID { get; set; }
        public int ClientID { get; set; }
        public int CarID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? KilometersDriven { get; set; }
        public double? TotalCost { get; set; }

        public static void StartRental(int clientId, int carId)
        {
            using var connection = Database.GetConnection();
            string query = "INSERT INTO Rentals (ClientID, CarID, StartTime) VALUES (@ClientID, @CarID, @StartTime);";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ClientID", clientId);
            command.Parameters.AddWithValue("@CarID", carId);
            command.Parameters.AddWithValue("@StartTime", DateTime.Now.ToString("o"));
            command.ExecuteNonQuery();
        }

        public static void EndRental(int rentalId, double kilometersDriven)
        {
            using var connection = Database.GetConnection();
            string query = "UPDATE Rentals SET EndTime = @EndTime, KilometersDriven = @KilometersDriven, TotalCost = @TotalCost WHERE ID = @RentalID;";

            var rental = GetRentalById(rentalId);
            if (rental == null) throw new Exception("Rental not found.");

            var car = Car.GetAll().First(c => c.ID == rental.CarID);
            var hours = (DateTime.Now - rental.StartTime).TotalHours;
            var totalCost = hours * car.HourlyRate + kilometersDriven * car.KilometerRate;

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@EndTime", DateTime.Now.ToString("o"));
            command.Parameters.AddWithValue("@KilometersDriven", kilometersDriven);
            command.Parameters.AddWithValue("@TotalCost", totalCost);
            command.Parameters.AddWithValue("@RentalID", rentalId);
            command.ExecuteNonQuery();
        }

        public static Rental GetRentalById(int rentalId)
        {
            using var connection = Database.GetConnection();
            string query = "SELECT * FROM Rentals WHERE ID = @RentalID;";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@RentalID", rentalId);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Rental
                {
                    ID = reader.GetInt32(0),
                    ClientID = reader.GetInt32(1),
                    CarID = reader.GetInt32(2),
                    StartTime = DateTime.Parse(reader.GetString(3)),
                    EndTime = reader.IsDBNull(4) ? (DateTime?)null : DateTime.Parse(reader.GetString(4)),
                    KilometersDriven = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                    TotalCost = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6)
                };
            }
            return null;
        }
    }
}


