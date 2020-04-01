using System;
using System.Threading;
using System.Threading.Tasks;

namespace RollerCoasterProblem
{
	internal class RollerCoaster
	{

        private static SemaphoreSlim mutex1;
        private static SemaphoreSlim mutex2;
        private static int boarders;
        private static int unboarders;
        private static SemaphoreSlim boardQueue;
        private static SemaphoreSlim unboardQueue;
        private static SemaphoreSlim allAboard;
        private static SemaphoreSlim allAshore;

        private static int numPassengers;
        private static int numSeats;

        public static Random random = new Random();

        public static void Main()
        {
            Console.WriteLine("----------------------- JurassicPark -----------------------");
            Console.WriteLine("(aka RollerCoaster synchronization problem using Semaphores)");
            Console.WriteLine("                                __");
            Console.WriteLine("                               / _)  RAWR!");
            Console.WriteLine("                      _/\\/\\/\\_/ /");
            Console.WriteLine("                    _|         /");
            Console.WriteLine("                  _|  (  | (  |");
            Console.WriteLine("                 /__.-'|_|--|_|");
            Console.WriteLine();



            Console.WriteLine("Enter the expected number of passengers:");
            string input = Console.ReadLine();
            Int32.TryParse(input, out numPassengers);

            Console.WriteLine("Enter the number of seats in car:");
            input = Console.ReadLine();
            Int32.TryParse(input, out numSeats);

            if (numSeats > numPassengers)
            {
                Console.WriteLine("Number of passengers is too small to fill the car, park is closing.");
                Console.WriteLine("Press any key to EXIT.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            if (numSeats <= 0)
            {
                Console.WriteLine("Number of seats is too small, park is closing.");
                Console.WriteLine("Press any key to EXIT.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            if (numPassengers <= 0)
            {
                Console.WriteLine("Number of visitors is too small, park is closing.");
                Console.WriteLine("Press any key to EXIT.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            RollerCoaster.mutex1 = new SemaphoreSlim(1, numPassengers);
            RollerCoaster.mutex2 = new SemaphoreSlim(1, numPassengers);

            RollerCoaster.boarders = 0;
            RollerCoaster.unboarders = 0;

            RollerCoaster.boardQueue = new SemaphoreSlim(0, numSeats);
            RollerCoaster.unboardQueue = new SemaphoreSlim(0, numSeats);

            RollerCoaster.allAboard = new SemaphoreSlim(0, numSeats);
            RollerCoaster.allAshore = new SemaphoreSlim(0, numSeats);

            Task.Run(() => {
                Car();
            });

            for (int i = 0; i < numPassengers; i++)
            {
                int temp = i;
                Task.Run(() => {
                    Passenger(temp + 1);
                });
            }

            Console.ReadKey();
        }

        public static void Car()
        {
            Load();
            boardQueue.Release(numSeats);
            allAboard.Wait();

            AllAboard();

            Unload();
            unboardQueue.Release(numSeats);

            allAshore.Wait();
            numPassengers -= numSeats;

            if (numPassengers == 0)
            {
                Console.WriteLine("All passengers took a ride, park is closing.");
                Console.WriteLine("Press any key to EXIT.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (numPassengers < numSeats)
            {
                Console.WriteLine("The remaining passengers do not fill the car, park is closing.");
                Console.WriteLine("Press any key to EXIT.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else
            {
                Task.Run(() => {
                    Car();
                });
            }
        }

        private static void AllAboard()
        {
            Console.WriteLine("Car is full and its going for ride. (5000ms)");
            Thread.Sleep(5000);
        }

        private static void Unload()
        {
            Console.WriteLine("Car is dispatching passengers.");
        }

        private static void Load()
        {
            Console.WriteLine("Car is loading passengers.");
        }

        private static void Passenger(int i)
        {
            int? id = i;

            ReadyToBoard(id);
            boardQueue.Wait();
            Board(id);

            mutex1.Wait();
            boarders++;
            if (boarders == numSeats)
            {
                allAboard.Release();
                boarders = 0;
            }
            mutex1.Release();

            unboardQueue.Wait();
            Unboard(id);

            mutex2.Wait();
            unboarders++;
            if (unboarders == numSeats)
            {
                allAshore.Release();
                unboarders = 0;
            }
            mutex2.Release();


        }

        private static void ReadyToBoard(int? id)
        {
            // Passengers visit the park and wander around for random time
            Console.WriteLine(String.Format("Passenger {0} wandering around park.", id));
            Thread.Sleep(random.Next(2000, 8000));
            Console.WriteLine(String.Format("Passenger {0} is ready to board the car.", id));
        }

        private static void Unboard(int? id)
        {
            Console.WriteLine(String.Format("Passenger {0} left the car.", id));
        }

        private static void Board(int? id)
        {
            Console.WriteLine(String.Format("Passenger {0} boarded on car.", id));
        }
    } 
}
