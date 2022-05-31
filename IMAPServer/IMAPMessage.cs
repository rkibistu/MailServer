using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using MimeKit;

namespace IMAPServer {


    class IMAPMessage {
        public string From;
        public string Subject;
        public List<string> To = new List<string>();
        public List<string> Data = new List<string>();

        private string tempFile = "tempFileMails.txt";

        public void Save() {
            string currentPath;         //aici o sa fie numele fisierului pentru mailu asta
            DirectoryInfo directory;    //directorul clientului actual (ca sa verificam ca exista sau sa il creem)          
            MimeMessage message;        //mailul primit sub forma de MimeMessage
            foreach (var to in To) {

                string username = extracUserFromMail(extractMails(to)[0]);  // kail din RCPT TO:kail@razvi.rest
                currentPath = IMAPServer.ReceivedPath + "\\" + username;    
                directory = new DirectoryInfo(currentPath);
                if (!directory.Exists) {
                    directory.Create();
                }

                message = CreateMimeMessage();
                currentPath += "\\" + message.MessageId + ".txt"; //ca sa fie unic sigur
                
                WriteMessageToFile(currentPath); //scrie mailul in fisier; Scriu tot mailul, nu doar partile ce are nevoie sa le vada clientul
            }
        }

        private static string[] extractMails(string text) {
            Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*",
                RegexOptions.IgnoreCase);
            MatchCollection emailMatches = emailRegex.Matches(text);
            List<string> result = new List<string>();
            foreach (Match emailMatch in emailMatches)
                result.Add(emailMatch.Value);
            return result.ToArray();
        }

        private static string extracUserFromMail(string mailAddres) {

            string result = "";
            foreach(var character in mailAddres) {

                if (character == '@')
                    break;
                result += character;
            }
            return result;
        }

        private MimeMessage CreateMimeMessage() {

            FileStream a = File.Create(tempFile);
            a.Close();

            foreach (var line in Data) {
                if (line != "DATA")
                    File.AppendAllText(tempFile, line + "\n");
            }

            MimeMessage message = MimeMessage.Load(tempFile);
            return message;
        }
        private void WriteMessageToFile(string filename) {

            FileStream a = File.Create(filename);
            a.Close();
            foreach (var line in Data) {

                if (line != "DATA")
                    File.AppendAllText(filename, line + "\n");
            }
        }
    }
}
