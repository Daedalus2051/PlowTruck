using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlowTruckConsole
{
    class MenuSystem
    {
        #region Variables
        private string _greeting;
        private string _prompt;
        private string[] _menuItems;

        /// <summary>
        /// String that will be displayed above the menu.
        /// </summary>
        public string Greeting
        {
            get { return _greeting; }
            set { _greeting = value; }
        }
        /// <summary>
        /// String that will be displayed as the prompt for user input.
        /// </summary>
        public string Prompt
        {
            get { return _prompt; }
            set { _prompt = value; }
        }
        /// <summary>
        /// String array of menu items that will be displayed.
        /// </summary>
        public string[] MenuItems
        {
            get { return _menuItems; }
            set { _menuItems = value; }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Basic constructor for the MenuSystem class, initializes with no properties; they must be set later.
        /// </summary>
        public MenuSystem()
        {
        }
        /// <summary>
        /// Basic constructor for the MenuSystem class
        /// </summary>
        /// <param name="greeting">String that will be displayed above the menu.</param>
        /// <param name="menuItems">String array of menu items that will be displayed.</param>
        public MenuSystem(string greeting, string[] menuItems)
        {
            _greeting = greeting;
            _menuItems = menuItems;
        }
        /// <summary>
        /// Basic constructor for the MenuSystem class.
        /// </summary>
        /// <param name="greeting">String that will be displayed above the menu.</param>
        /// <param name="prompt">String that will be displayed as the prompt for user input.</param>
        /// <param name="menuItems">String array of menu items that will be displayed.</param>
        public MenuSystem(string greeting, string prompt, string[] menuItems)
        {
            _greeting = greeting;
            _menuItems = menuItems;
            _prompt = prompt;
        }
        #endregion
        #region Methods
        // Load methods; for pulling in menu items from a file

        // Command processor? Probably not generic enough; this class should return the input to be processed by another class for running the methods

        // Draw menu
        /// <summary>
        /// Draws the menu from the string array that was provided in the constructor or set as a property.
        /// </summary>
        public void DrawMenu()
        {
            // Check that the array contains items 
            if (_menuItems == null)
            {
                Console.WriteLine("Error! Menu array contains no items!");
                return;
            }

            // Array is valid, run through printing the menu
            Console.Clear();
            Console.WriteLine(_greeting);
            for (int i = 0; i < _menuItems.Length; i++)
            {
                Console.WriteLine("{0}. {1}", (i + 1), _menuItems[i]);
            }
            Console.WriteLine();
            Console.Write(_prompt);
        }

        // Receive input (menu item number), returns to caller
        /// <summary>
        /// Reads the users input of what menu choice they want. [Currently does not implement validation or error handling]
        /// </summary>
        /// <returns>Returns an int of what menu item was chosen</returns>
        public int ReadInput()
        {
            // The most basic of input... this is mostly just so I can start testing; should change this to be more robust and validate input
            int input = -1;
            input = Convert.ToInt32(Console.ReadLine());
            return input;
        }
        #endregion
    }
}
