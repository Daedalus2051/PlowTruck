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
            Console.WriteLine("Initializing...");

            // Check for command line args first
            if (args.Length > 0)
            {
                // Process args if they were passed
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                            // TODO: Define switch arguments
                        case "":

                            break;

                        default:
                            Console.WriteLine("Switch '{0}' not recognized.", arg);
                            return;
                    }
                }
                return; // Make sure we don't go into the menu system if args are passed.
            }
            // Run the menu system if no command line arguments were passed
            RunMenu();

            Console.WriteLine("Thank you for using the PlowTruck!");
        }

        public static void RunMenu()
        {
            bool isRunning = true;
            int choice = -1;

            // Create the root menu for users
            MenuSystem rootMenu = new MenuSystem();
            rootMenu.Greeting = "Welcome to the PlowTruck console!";
            string[] rootMenuDef = new string[] { "Option 1", "Option 2", "Option 3", "Exit" };
            rootMenu.MenuItems = rootMenuDef;
            rootMenu.Prompt = "Choice->";

            while (isRunning)
            {
                rootMenu.DrawMenu();
                choice = rootMenu.ReadInput();

                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Choice made: {0} yay!", choice);
                        Console.ReadLine();
                        break;

                    case 2:
                        Console.WriteLine("Choice made: {0} @choicemade", choice);
                        Console.ReadLine();
                        break;

                    case 3:
                        Console.WriteLine("Choice made: {0} #menusystemsareawesome", choice);
                        Console.ReadLine();
                        break;

                    case 4:
                        Console.WriteLine("Choice made: {0} quitting...", choice);
                        isRunning = false;
                        break;

                    default:
                        Console.WriteLine("Command '{0}' not recognized.", choice);
                        Console.ReadLine();
                        break;
                }
            }

        }
    }
}
