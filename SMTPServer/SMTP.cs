using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Common.SMTP {
    class SMTP {

        //gaseste toate emailurile dintr un text si retunreaza un array cu ele
        public static string[] extractMails(string text) {
            Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*",
            RegexOptions.IgnoreCase);
            MatchCollection emailMatches = emailRegex.Matches(text);
            List<string> result = new List<string>();
            foreach (Match emailMatch in emailMatches)
                result.Add(emailMatch.Value);
            return result.ToArray();
        }
        public static bool checkRCPTIsInternal(string text) {
            string[] mails = extractMails(text);
            foreach (string mail in mails)
                if (!isInternal(mail))
                    return false;
            return true;
        }
        public static bool isInternal(string email) {
            if (email.EndsWith("@razvi.rest"))
                return true;
            if (email.EndsWith("@localhost"))
                return true;
            if (email.EndsWith("@localhost.com"))
                return true;
            return false;
        }
    }
    class SMTPMessage {
        public string From;
        public List<string> To = new List<string>();
        public List<string> Data = new List<string>();
        internal void SaveAndSend() {
            Save();
            foreach (var to in this.To) {
                var mail = SMTP.extractMails(to);
                if (!SMTP.isInternal(mail[0]))
                    SMTPServer.Sender.Send(mail[0], Data, From);
            }
        }
        internal void Save() {
            DirectoryInfo directory = SMTPServer.SMTPServer.ReceivedPath;
            directory.Create();
            string uniqueFileName = Path.Combine(directory.FullName, System.Guid.NewGuid().ToString() + ".smtp");
            File.WriteAllLines(uniqueFileName, To);
            File.AppendAllLines(uniqueFileName, Data);
        }
    }
}