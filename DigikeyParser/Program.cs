using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace DigikeyParser {
    class Program {


        static private DigikeyParser.ProductsListResult ClearDigiKeyInfo(DigikeyParser.ProductsListResult list) {
            list = DigikeyParser.DeleteColumn(list, "Digi-Key Part Number");
            list = DigikeyParser.DeleteColumn(list, "Quantity Available");
            list = DigikeyParser.DeleteColumn(list, "Unit Price USD");
            list = DigikeyParser.DeleteColumn(list, "Minimum Quantity");
            list = DigikeyParser.DeleteColumn(list, "Series");
            list = DigikeyParser.DeleteColumn(list, "Part Status");
            list = DigikeyParser.DeleteColumn(list, "Supplier Device Package");
            return list;
        }


        static void Main(string[] args) {


            Console.WriteLine("GetProductsList();");
            var parser = new DigikeyParser();
            var categoryList = parser.GetProductsList("https://www.digikey.com/products/en/integrated-circuits-ics/audio-special-purpose/741");
            categoryList = ClearDigiKeyInfo(categoryList);

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
