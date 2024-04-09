using System.Net.Http.Headers;
using System.Text;

//Спасибо, что прислали задание, благодаря таким компаниям как вы я имею возможность практиковаться выполняя тестовые и повышать навыки.

namespace NET_TEST{
    internal class Program{
        static string journalPath = "Source\\Journal\\fileJournal.txt";
        static string journalDir = "Source\\Journal";
        static string outputPath = "Source\\Output\\fileOutput.txt";
        static string outputDir = "Source\\Output";
        static string logPath = "Source\\Log\\fileLog.txt";
        static string logDir = "Source\\Log";
        static int ipStart = 0;
        static int ipEnd= 255;
        static long timeStart = 0;
        static long timeEnd = ((DateTimeOffset)(DateTime.UtcNow)).ToUnixTimeSeconds();
        static public void Main(string[] args){
            //args = new string[]{"--file-log","--file-output", "--time-start-07.04.2024","--time-end-08.04.2024","--address-start-110","--address-mask-120"}; // пример как я вводил
            if(!Directory.Exists(logDir))Directory.CreateDirectory(logDir);
            if(!File.Exists(logPath))File.Create(logPath).Close();
            AppFile(logPath,$"{DateTime.Now}: file fileLog.txt is not found, created new file fileLog.txt");
            if(!Directory.Exists(outputDir))Directory.CreateDirectory(outputDir);
            if(!File.Exists(outputPath))File.Create(outputPath).Close();
            AppFile(logPath,$"{DateTime.Now}: file fileOutput.txt is not found, created new file fileOutput.txt");
            if(!Directory.Exists(journalDir))Directory.CreateDirectory(journalDir);
            if(!File.Exists(journalPath)){
                AppFile(logPath,$"{DateTime.Now}: file -fileJournal- at path -{journalPath}- is not found");
            }else{
                AppFile(logPath,$"{DateTime.Now}: program start");
                if(args.Length>0){
                    TakeArg(args);
                    string str = "";
                    foreach(var a in args)str+=a + " ";
                    AppFile(logPath,$"{DateTime.Now}: args taken {str}");
                }
                HashSet<string>ipList = new HashSet<string>();
                if(CheckJournal(ReadJournal(journalPath))){
                    ipList = ReadJournal(journalPath).ToHashSet();
                } else {
                    Console.WriteLine("Error, data journal!");
                }
                ShowIP(ipList,ipStart,ipEnd,timeStart,timeEnd);
                AppFile(logPath,$"{DateTime.Now}: program end");
            }
        }
        /// <summary>
        /// Вывод адресов в файл
        /// </summary>
        /// <param name="ipList">весь список</param>
        /// <param name="ipStart">нижняя граница</param>
        /// <param name="ipEnd">верхняя граница</param>
        /// <param name="timeStart">начальная временная точка</param>
        /// <param name="timeEnd">конечная временная точка</param>
        static void ShowIP(HashSet<string>ipList, int ipStart, int ipEnd, long timeStart, long timeEnd){
            HashSet<string> outputList = new HashSet<string>();
            foreach(var ip_date in ipList){
                if(checkIpBorder(ip_date.Split(':',2)[0],ipStart,ipEnd) && checkTimeBorder((DateTimeOffset.Parse(ip_date.Split(':',2)[1])).ToUnixTimeSeconds(),timeStart,timeEnd))outputList.Add(ip_date);
            }
            string str = "";
            if(outputList.Count>0)foreach(var ip in outputList)str+=ip+"\n";
            WriteFile(outputPath,str);
            AppFile(logPath,$"{DateTime.Now}: file fileOutput.txt created");
        }
        /// <summary>
        /// создание файла с найдеными адресами
        /// </summary>
        /// <param name="path">путь</param>
        /// <param name="str">текст в файл</param>
        static void WriteFile(string path, string str) {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
                string userText = str;
                byte[] writeBytes = Encoding.Default.GetBytes(userText);
                fs.Write(writeBytes, 0, writeBytes.Length);
            }
        }
        /// <summary>
        /// проверка по границам адресов
        /// </summary>
        /// <param name="ip">адрес</param>
        /// <param name="ipStart">нижняя граница</param>
        /// <param name="ipEnd">верхняя граница</param>
        /// <returns></returns>
        static bool checkIpBorder(string ip, int ipStart, int ipEnd){
            foreach(var n in ip.Split('.'))if(Int32.Parse(n)<ipStart || Int32.Parse(n)>ipEnd) return false;
            return true;
        }
        /// <summary>
        /// проверка по времени
        /// </summary>
        /// <param name="ip">адрес</param>
        /// <param name="timeStart">начальная временная точка</param>
        /// <param name="timeEnd">конечная временная точка</param>
        /// <returns></returns>
        static bool checkTimeBorder(long ip, long timeStart, long timeEnd){
            if(ip<timeStart || ip>timeEnd)return false;
            return true;
        }
        /// <summary>
        /// проверка аргументов
        /// </summary>
        /// <param name="Args">аргументы</param>
        static void TakeArg(string[] Args){
            foreach(var a in Args){
                int checkIP = 0;
                long checkTime = 0;
                if(a.Contains("--file-log"))Console.WriteLine(logPath);
                if(a.Contains("--file-output"))Console.WriteLine(outputPath);
                if(a.Contains("--time-start")){
                    string str = a.Remove(0,"--time-start".Length);
                    int yyyy = 0, MM = 0, dd = 0;
                    if(str.Split('.')[2].Length>4){
                        yyyy = Int32.Parse(trimStr((str.Split('.')[2]).Split()[0]));
                    } else {
                        yyyy = Int32.Parse(trimStr(str.Split('.')[2]));
                    }
                    MM = Int32.Parse(trimStr(str.Split('.')[1]));
                    dd = Int32.Parse(trimStr(str.Split('.')[0]));
                    timeStart = ((DateTimeOffset)DateTime.Parse(new string(dd+"."+MM+"."+yyyy+" "+"00"+":"+"00"+":"+"00"))).ToUnixTimeSeconds();
                }
                if(a.Contains("--time-end")){
                    string str = a.Remove(0,"--time-end".Length);
                    int yyyy = 0, MM = 0, dd = 0;
                    if(str.Split('.')[2].Length>4){
                        yyyy = Int32.Parse(trimStr((str.Split('.')[2]).Split()[0]));
                    } else {
                        yyyy = Int32.Parse(trimStr(str.Split('.')[2]));
                    }
                    MM = Int32.Parse(trimStr(str.Split('.')[1]));
                    dd = Int32.Parse(trimStr(str.Split('.')[0]));
                    timeEnd = ((DateTimeOffset)DateTime.Parse(new string(dd+"."+MM+"."+yyyy+" "+"23"+":"+"59"+":"+"59"))).ToUnixTimeSeconds();
                    if(timeEnd<timeStart){
                        checkTime = timeEnd;
                        timeEnd = timeStart;
                        timeStart = checkTime;
                    }
                }
                if(a.Contains("--address-start")){
                    string str = a.Remove(0,"--address-start".Length);
                    ipStart = Int32.Parse(trimStr(str));
                }
                if(a.Contains("--address-mask")){
                    string str = a.Remove(0,"--address-mask".Length);
                    ipEnd = Int32.Parse(trimStr(str));
                    if(ipStart>ipEnd){
                        checkIP = ipEnd;
                        ipEnd = ipStart;
                        ipStart = ipEnd;
                    }
                }
            }
        }
        /// <summary>
        /// запись переданной методу строки в файл
        /// </summary>
        /// <param name="path">полный путь к фалу</param>
        /// <param name="str"></param>
        static void AppFile(string path, string str) {
            using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None)) {
                string userText = str + "\n";
                byte[] writeBytes = Encoding.Default.GetBytes(userText);
                fs.Write(writeBytes, 0, writeBytes.Length);
            }
        }
        /// <summary>
        /// удаление лишних (кроме цифр) символов из строки
        /// </summary>
        /// <param name="str">строка для проверки</param>
        /// <returns></returns>
        static string trimStr(string str){
            List<char> check = str.ToList();
            foreach(char c in str)if((int)c>(int)'9' || (int)c<(int)'0')check.Remove(c);
            return new string(check.ToArray());
        }
        /// <summary>
        /// прочитать журнал
        /// </summary>
        /// <param name="path">полный путь к журналу</param>
        /// <returns></returns>
        static string[] ReadJournal(string path) {
            using(FileStream fs = new FileStream(path,FileMode.Open, FileAccess.Read, FileShare.Read)) {
                byte[] inData = new byte[fs.Length];
                fs.Read(inData, 0, inData.Length);
                string str = Encoding.Default.GetString(inData);
                string[] ipData = CleanData(str.Split('\n'));
                fs.Close();
                return ipData;
            }
        }
        /// <summary>
        /// удаление лишних строк считанных с файла
        /// </summary>
        /// <param name="data">массив данных</param>
        /// <returns></returns>
        static string[] CleanData(string[] data){
            HashSet<string> newData = new HashSet<string>();
            foreach(var str in data)if(str.Length>13)newData.Add(str);
            return newData.ToArray();
        }
        /// <summary>
        /// проверка журнала на корректность данных
        /// </summary>
        /// <param name="ipList">массив данных</param>
        /// <returns></returns>
        static bool CheckJournal(string[] ipList){
            if(!CheckIP(ipList[0].Split(':',2)[0])){
                AppFile(logPath,$"{DateTime.Now}: file -fileJournal- data error, no IPv4 address formats was found");
                return false;
            }
            foreach(var ip_date in ipList){
                if(!CheckIP(new string(ip_date.Split(':',2)[0]))){
                    AppFile(logPath,$"{DateTime.Now}: file -fileJournal- data error, abnormal address -{ip_date.Split(':',2)[0]}- was found");
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// проверка адресса
        /// </summary>
        /// <param name="str">строка на проверку</param>
        /// <returns></returns>
        static bool CheckIP(String str){
            try{
                string[] check = str.Split('.');
                if(check.Length!=4) return false;
                foreach(var num in check)if(Int32.Parse(num) > 255 || Int32.Parse(num) < 0) return false;
                return true;
            } catch (Exception ex){
                AppFile(logPath,$"{DateTime.Now}: file -fileJournal- data error, {ex.Message}");
                return false;
            }
        }
    }
}