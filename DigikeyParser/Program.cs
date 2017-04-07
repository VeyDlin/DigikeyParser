using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace DigikeyParser {
    class Program {
        static void Main(string[] args) {

            Parser parser = new Parser() {
                pageUrl = "https://www.digikey.com/products/en/integrated-circuits-ics/data-acquisition-digital-to-analog-converters-dac/701"
            };

            parser.UpdateInfo();

            // Столбцы
            Console.WriteLine("==================================");
            foreach (var column in parser.columns) {
                Console.WriteLine(column);
            }
            Console.WriteLine("==================================");


            // Выводим весь список
            foreach (var row in parser.table) {
                foreach (var cell in row) {
                    Console.WriteLine(cell.Key + ": " + cell.Value);
                }

                Console.WriteLine("----------------------------------");
            }


            Console.ReadKey();
        }
    }
}
