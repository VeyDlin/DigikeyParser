using System;
using HtmlAgilityPack;
using SeasideResearch.LibCurlNet;
using System.IO;
using System.Collections.Generic;



namespace DigikeyParser {
    class Parser {
        public string pageUrl { get; set; }

        public List<Dictionary<string, string>> table { get; private set; } // table[3]["Digi-Key Part Number"]
        public List<string> columns { get; private set; }

        public string proxy { get; set; } = "";
        public string userAgent { get; set; } = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
        public string cookieFile { get; set; } = "";

        private Easy easy;
        private string SockBuff;


        // Инициализация
        public Parser() {
            Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
        }




        // Удаляем куки (файл хранящий их)
        public void ClearCookies() {
            if (File.Exists(cookieFile)) {
                File.Delete(cookieFile);
            }
        }




        // Сделать GET запрос
        private string Get(string url) {
            SockBuff = "";
            easy = new Easy();

            Easy.WriteFunction wf = new Easy.WriteFunction(delegate (Byte[] buf, Int32 size, Int32 nmemb, Object extraData) {
                SockBuff = SockBuff + System.Text.Encoding.UTF8.GetString(buf);
                return size * nmemb;
            });
            easy.SetOpt(CURLoption.CURLOPT_URL, url);
            easy.SetOpt(CURLoption.CURLOPT_TIMEOUT, "60");
            easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
            easy.SetOpt(CURLoption.CURLOPT_USERAGENT, userAgent);
            easy.SetOpt(CURLoption.CURLOPT_COOKIEFILE, cookieFile);
            easy.SetOpt(CURLoption.CURLOPT_COOKIEJAR, cookieFile);
            easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, true);

            if (url.Contains("https")) {
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 1);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 0);
            }

            if (proxy != "") {
                easy.SetOpt(CURLoption.CURLOPT_PROXY, proxy);
                easy.SetOpt(CURLoption.CURLOPT_PROXYTYPE, CURLproxyType.CURLPROXY_HTTP);
            }

            easy.Perform();
            easy.Dispose();

            return SockBuff;
        }




        // Очистка пробелов больше одного, переносов строк и пробелов по краям
        private string SuperTrim(string str) {
            var reg = new System.Text.RegularExpressions.Regex(@"\s{2,}");
            return reg.Replace(str, " ").Trim();
        }




        // Получение прямой ссылки на PDF без редиректов
        private string RemoveUrlRedirect(string url) {
            return url; // TODO: Мне щас так лень это делать
        }




        // Обновить таблицу результатов с digikey
        public void UpdateInfo() {
            Console.WriteLine("Load " + pageUrl);

            int i = 0;
            columns = new List<string>();
            table = new List<Dictionary<string, string>>();


            // Загружаем страницу
            var doc = new HtmlDocument();
            doc.LoadHtml(Get(pageUrl));


            // Получаем список всех названий столбцов и записываем в tblheadArray
            var tblhead = doc.GetElementbyId("tblhead");
            var tblheadRow = tblhead.SelectNodes("tr")[0];
            i = 0;
            foreach (HtmlNode cell in tblheadRow.SelectNodes("th|td")) {
                switch (i) {
                    case 0: break;  // Compare Parts нам нахер не нужен

                    case 1: // Во втором столбце у нас иконка PDF, надо заменить на текст
                        columns.Add("PDF"); 
                    break;

                    default:
                        columns.Add(SuperTrim(cell.InnerText));
                    break;
                }
                i++;
            }


            // Парсим саму информацию          
            var lnkPart = doc.GetElementbyId("lnkPart");
            foreach (HtmlNode row in lnkPart.SelectNodes("tr")) {

                i = -1; // Ибо мы удалили Compare Parts из columns
                var rowInfo = new Dictionary<string, string>();
                foreach (HtmlNode cell in row.SelectNodes("th|td")) {
                    switch (i) {
                        case -1: break; // Compare Parts нам все ещё нахер не нужен

                        case 0: // PDF надо вытаскивать по другому
                            string pdfUrl = cell.SelectSingleNode("a").Attributes["href"].Value;  
                            rowInfo[columns[i]] = RemoveUrlRedirect(pdfUrl); 
                        break;

                        case 1: // Получаем ссылку на картинку компонента
                            string imgUrl = cell.SelectSingleNode("a/img").Attributes["zoomimg"].Value;
                            rowInfo[columns[i]] = imgUrl;
                        break;

                        default:
                            rowInfo[columns[i]] = SuperTrim(cell.InnerText);
                        break;
                    }
                    i++;
                }

                // Добавляем один row в таблицу
                table.Add(rowInfo);
            }

        }

    }
}
