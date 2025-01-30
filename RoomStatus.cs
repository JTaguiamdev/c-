using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HotelSystem
{
    // Enum for room status
    public enum RoomStatus
    {
        Vacant,
        Occupied,
        UnderMaintenance
    }

    // Enum for room type
    public enum RoomType
    {
        Standard,
        Deluxe,
        Suite
    }

    // Room class
    public class Room
    {
        public int RoomNumber { get; set; }
        public RoomType Type { get; set; }
        public RoomStatus Status { get; set; }
        public decimal PricePerNight { get; set; }
    }

    // Guest class
    public class Guest
    {
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }

    // Booking class
    public class Booking
    {
        public int BookingId { get; set; }
        public int RoomNumber { get; set; }
        public string GuestName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalCost { get; set; }
    }

    // Custom exception for invalid room number
    public class InvalidRoomNumberException : Exception
    {
        public InvalidRoomNumberException(string message) : base(message) { }
    }

    // Hotel class to manage rooms, guests, and bookings
    public class Hotel
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Guest> Guests { get; set; } = new List<Guest>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();

        private const string RoomsFilePath = "rooms.txt";
        private const string GuestsFilePath = "guests.txt";
        private const string BookingsFilePath = "bookings.txt";

        public Hotel()
        {
            LoadData();
        }

        // Add a new room
        public void AddRoom(int roomNumber, RoomType type, decimal pricePerNight)
        {
            Rooms.Add(new Room
            {
                RoomNumber = roomNumber,
                Type = type,
                Status = RoomStatus.Vacant,
                PricePerNight = pricePerNight
            });
            Console.WriteLine($"Room {roomNumber} added successfully!");
            SaveData();
        }

        // Book a room
        public void BookRoom(string guestName, string contactInfo, int roomNumber, DateTime checkInDate, DateTime checkOutDate)
        {
            if (checkInDate >= checkOutDate)
            {
                Console.WriteLine("Check-in date must be before check-out date.");
                return;
            }

            var room = Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber && r.Status == RoomStatus.Vacant);
            if (room == null)
            {
                Console.WriteLine("Room is not available.");
                return;
            }

            var guest = Guests.FirstOrDefault(g => g.Name == guestName);
            if (guest == null)
            {
                guest = new Guest { Name = guestName, ContactInfo = contactInfo };
                Guests.Add(guest);
            }

            int bookingId = Bookings.Count + 1;
            int nights = (checkOutDate - checkInDate).Days;
            decimal totalCost = room.PricePerNight * nights;

            var booking = new Booking
            {
                BookingId = bookingId,
                RoomNumber = roomNumber,
                GuestName = guestName,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                TotalCost = totalCost
            };

            Bookings.Add(booking);
            guest.Bookings.Add(booking);
            room.Status = RoomStatus.Occupied;

            Console.WriteLine($"Booking successful! Booking ID: {bookingId}, Total Cost: {totalCost}");
            SaveData();
        }

        // Check-out a guest
        public void CheckOut(int roomNumber)
        {
            var room = Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber && r.Status == RoomStatus.Occupied);
            if (room == null)
            {
                Console.WriteLine("Room is not occupied.");
                return;
            }

            var booking = Bookings.FirstOrDefault(b => b.RoomNumber == roomNumber && b.CheckOutDate >= DateTime.Now);
            if (booking != null)
            {
                Console.WriteLine($"Guest {booking.GuestName} checked out. Total bill: {booking.TotalCost}");
                room.Status = RoomStatus.Vacant;
                SaveData();
            }
            else
            {
                Console.WriteLine("No active booking found for this room.");
            }
        }

        // Display all rooms
        public void DisplayRooms()
        {
            Console.WriteLine("Room List:");
            foreach (var room in Rooms)
            {
                Console.WriteLine($"Room {room.RoomNumber} - {room.Type} - {room.Status} - ${room.PricePerNight}/night");
            }
        }

        // Display all bookings
        public void DisplayBookings()
        {
            Console.WriteLine("Booking List:");
            foreach (var booking in Bookings)
            {
                Console.WriteLine($"Booking ID: {booking.BookingId}, Room: {booking.RoomNumber}, Guest: {booking.GuestName}, Check-In: {booking.CheckInDate.ToShortDateString()}, Check-Out: {booking.CheckOutDate.ToShortDateString()}, Total Cost: {booking.TotalCost}");
            }
        }

        // Save data to text files
        private void SaveData()
        {
            SaveRooms();
            SaveGuests();
            SaveBookings();
        }

        private void SaveRooms()
        {
            using (StreamWriter writer = new StreamWriter(RoomsFilePath))
            {
                foreach (var room in Rooms)
                {
                    writer.WriteLine($"{room.RoomNumber},{room.Type},{room.Status},{room.PricePerNight}");
                }
            }
        }

        private void SaveGuests()
        {
            using (StreamWriter writer = new StreamWriter(GuestsFilePath))
            {
                foreach (var guest in Guests)
                {
                    writer.WriteLine($"{guest.Name},{guest.ContactInfo}");
                }
            }
        }

        private void SaveBookings()
        {
            using (StreamWriter writer = new StreamWriter(BookingsFilePath))
            {
                foreach (var booking in Bookings)
                {
                    writer.WriteLine($"{booking.BookingId},{booking.RoomNumber},{booking.GuestName},{booking.CheckInDate},{booking.CheckOutDate},{booking.TotalCost}");
                }
            }
        }

        // Load data from text files
        private void LoadData()
        {
            LoadRooms();
            LoadGuests();
            LoadBookings();
        }

        private void LoadRooms()
        {
            if (File.Exists(RoomsFilePath))
            {
                using (StreamReader reader = new StreamReader(RoomsFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        Rooms.Add(new Room
                        {
                            RoomNumber = int.Parse(parts[0]),
                            Type = Enum.Parse<RoomType>(parts[1]),
                            Status = Enum.Parse<RoomStatus>(parts[2]),
                            PricePerNight = decimal.Parse(parts[3])
                        });
                    }
                }
            }
        }

        private void LoadGuests()
        {
            if (File.Exists(GuestsFilePath))
            {
                using (StreamReader reader = new StreamReader(GuestsFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        Guests.Add(new Guest
                        {
                            Name = parts[0],
                            ContactInfo = parts[1]
                        });
                    }
                }
            }
        }

        private void LoadBookings()
        {
            if (File.Exists(BookingsFilePath))
            {
                using (StreamReader reader = new StreamReader(BookingsFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        Bookings.Add(new Booking
                        {
                            BookingId = int.Parse(parts[0]),
                            RoomNumber = int.Parse(parts[1]),
                            GuestName = parts[2],
                            CheckInDate = DateTime.Parse(parts[3]),
                            CheckOutDate = DateTime.Parse(parts[4]),
                            TotalCost = decimal.Parse(parts[5])
                        });
                    }
                }
            }
        }

        // Validate room number
        public void ValidateRoomNumber(int roomNumber)
        {
            if (!Rooms.Any(r => r.RoomNumber == roomNumber))
            {
                throw new InvalidRoomNumberException($"Room number {roomNumber} does not exist.");
            }
        }
    }

    // Main program
    class Program
    {
        static void Main(string[] args)
        {
            Hotel hotel = new Hotel();

            // Add some rooms
            hotel.AddRoom(101, RoomType.Standard, 100);
            hotel.AddRoom(102, RoomType.Deluxe, 150);
            hotel.AddRoom(103, RoomType.Suite, 200);

            while (true)
            {
                Console.WriteLine("\nHotel Room Reservation and Management System");
                Console.WriteLine("1. Display Rooms");
                Console.WriteLine("2. Book a Room");
                Console.WriteLine("3. Check-Out");
                Console.WriteLine("4. Display Bookings");
                Console.WriteLine("5. Exit");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        hotel.DisplayRooms();
                        break;

                    case "2":
                        try
                        {
                            Console.Write("Enter guest name: ");
                            string guestName = Console.ReadLine();
                            Console.Write("Enter contact info: ");
                            string contactInfo = Console.ReadLine();
                            Console.Write("Enter room number: ");
                            int roomNumber = int.Parse(Console.ReadLine());
                            hotel.ValidateRoomNumber(roomNumber);
                            Console.Write("Enter check-in date (yyyy-mm-dd): ");
                            DateTime checkInDate = DateTime.Parse(Console.ReadLine());
                            Console.Write("Enter check-out date (yyyy-mm-dd): ");
                            DateTime checkOutDate = DateTime.Parse(Console.ReadLine());
                            hotel.BookRoom(guestName, contactInfo, roomNumber, checkInDate, checkOutDate);
                        }
                        catch (InvalidRoomNumberException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please try again.");
                        }
                        break;

                    case "3":
                        try
                        {
                            Console.Write("Enter room number to check-out: ");
                            int checkOutRoomNumber = int.Parse(Console.ReadLine());
                            hotel.ValidateRoomNumber(checkOutRoomNumber);
                            hotel.CheckOut(checkOutRoomNumber);
                        }
                        catch (InvalidRoomNumberException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please try again.");
                        }
                        break;

                    case "4":
                        hotel.DisplayBookings();
                        break;

                    case "5":
                        Console.WriteLine("Exiting the system. Goodbye!");
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }
}
