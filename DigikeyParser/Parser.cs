using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net;
using System.Text;


namespace DigikeyParser {

    class DigikeyParser {

        // Поддержка GZip у WebClient
        private class GZipWebClient : WebClient {
            protected override WebRequest GetWebRequest(Uri address) {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                return request;
            }
        }




        // Страница со списком 
        // Например: Product Index > Integrated Circuits (ICs) > Data Acquisition - Digital to Analog Converters (DAC)
        public struct CategoryResult {
            public List<Dictionary<string, string>> table; // table[3]["Digi-Key Part Number"]
            public List<string> columns;
        };




        // Инициализация
        public DigikeyParser() {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

        }




        // Сделать GET запрос
        private string GetHtmlCode(string url) {
            using(var webClient = new GZipWebClient()) {
                webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                webClient.Encoding = Encoding.UTF8;
 
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12");
                webClient.Headers.Add("Accept", "*/*");
                webClient.Headers.Add("Accept-Language", "en-gb,en;q=0.5");
                webClient.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");

                var htmlData = webClient.DownloadData(url);
                var htmlCode = Encoding.UTF8.GetString(htmlData);

                return htmlCode;
            }
        }





        // Получить HtmlDocument по ссылке
        private HtmlDocument GetHtmlDocument(string url) {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(GetHtmlCode(url));
            return htmlDocument;
        }





        // Очистка пробелов больше одного, переносов строк и пробелов по краям
        private string SuperTrim(string str) {
            var reg = new System.Text.RegularExpressions.Regex(@"\s{2,}");
            return reg.Replace(str, " ").Trim();
        }





        // Получение прямой ссылки на PDF без редиректов
        private string RemoveUrlRedirect(string url) {
            var uri = new Uri(url);

            if(uri.Host == "www.digikey.com" || uri.Host == "digikey.com") {
                var myWebRequest = WebRequest.Create(url);
                WebResponse myWebResponse = myWebRequest.GetResponse();

                string outUrl = uri.Equals(myWebResponse.ResponseUri) ? url : myWebResponse.ResponseUri.ToString();

                myWebResponse.Close();

                return outUrl;
            } else {
                return url;
            }
        }




        // Обновить таблицу результатов с digikey
        public CategoryResult GetCategory(string url) {
            var outList = new CategoryResult();
            outList.columns = new List<string>();
            outList.table = new List<Dictionary<string, string>>();


            // Загружаем страницу
            var htmlDocument = GetHtmlDocument(url);

            // Получаем список всех названий столбцов
            var tblheadRow = htmlDocument.GetElementbyId("tblhead").SelectNodes("tr")[0];
            int i = 0;
            foreach (HtmlNode cell in tblheadRow.SelectNodes("th|td")) {
                outList.columns.Add(i == 1 ? "PDF" : SuperTrim(cell.InnerText));
                i++;
            }


            // Парсим саму информацию       
           
            var lnkPart = htmlDocument.GetElementbyId("lnkPart");
            foreach (HtmlNode row in lnkPart.SelectNodes("tr")) {
                int index = 0;
                var rowInfo = new Dictionary<string, string>();
                foreach (HtmlNode cell in row.SelectNodes("th|td")) {
                    switch (index) {
                        case 1: // PDF надо вытаскивать по другому
                            string pdfUrl = cell.SelectSingleNode("a").Attributes["href"].Value;  
                            rowInfo[outList.columns[index]] = RemoveUrlRedirect(pdfUrl); 
                        break;

                        case 2: // Получаем ссылку на картинку компонента
                            string imgUrl = cell.SelectSingleNode("a/img").Attributes["zoomimg"].Value;
                            rowInfo[outList.columns[index]] = imgUrl;
                        break;

                        default:
                            rowInfo[outList.columns[index]] = SuperTrim(cell.InnerText);
                        break;
                    }
                    index++;
                }

                // Добавляем один row в таблицу
                outList.table.Add(rowInfo);
            }

            return outList;
        }






    }
}
