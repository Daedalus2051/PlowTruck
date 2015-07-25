using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlowTruckConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int choice=-1;
            do {
                DrawMenu();

                try { choice = Convert.ToInt16(Console.ReadLine()); }
                catch (FormatException)
                {
                    Console.WriteLine("Input was not a valid choice!");
                }

                switch (choice)
                {
                    case 1:
                        Console.WriteLine("User chose option {0}", choice);
                        break;
                    case 2:
                        Console.WriteLine("User chose option {0}", choice);
                        break;
                    case 3:
                        Console.WriteLine("User chose option {0}", choice);
                        break;
                    case 4:
                        Console.WriteLine("User chose option {0}", choice);
                        break;
                    case 5:
                        Console.WriteLine("User chose option {0}", choice);
                        break;
                    case 0:
                        Console.WriteLine("User chose option {0}, exiting...", choice);
                        break;

                    default:
                        Console.WriteLine("Choice {0} does not exist!", choice);
                        choice = -1;
                        break;
                }

                Console.ReadLine();
            }
            while (choice != 0);
            Console.WriteLine("Thank you for using the PlowTruck!");
            Console.ReadLine();
        }

        public static void DrawMenu()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the PlowTruck!");
            Console.WriteLine("\t1. Option 1");
            Console.WriteLine("\t2. Option 2");
            Console.WriteLine("\t3. Option 3");
            Console.WriteLine("\t4. Option 4");
            Console.WriteLine("\t5. Option 5");
            Console.WriteLine("\t0. Exit");
        }
    }
}
