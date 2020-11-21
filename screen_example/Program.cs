using Screen_Drop_In;
using System;

namespace screen_example
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var item in Screen.AllScreens) {
                Console.WriteLine(item.ToString());
            }
            Console.ReadLine();
        }
    }
}
