using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace DigikeyParser {
    class Program {

        static void Main(string[] args) {


            Console.WriteLine("GetCategory();");
            var parser = new DigikeyParser();
            var categoryList = parser.GetCategory("https://www.digikey.com/products/en/integrated-circuits-ics/audio-special-purpose/741");

            // Выводим весь список
            foreach (var row in categoryList.table) {
                foreach (var cell in row) {
                    Console.WriteLine(cell.Key + ": " + cell.Value);
                }

                Console.WriteLine("----------------------------------");
            }


            Console.ReadKey();
        }
    }
}
