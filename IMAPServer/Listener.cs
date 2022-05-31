using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;


namespace IMAPServer {
    public class Listener {
        System.IO.StreamReader reader;
        System.IO.StreamWriter writer;
        TcpClient client;

        private string fileCredentials = "users.txt";   //Fisierul d eunde citim cedentialele utilizatorilor


        //The combination of mailbox name, UIDVALIDITY, and UID
        //  must refer to a single immutable message on that server forever
        private int UID = 1;                    // identificatorul mesajului (ar trebui sa il pastrez intr-un fisier, ca sa nu se repete la fiecare pornire)

        private int UIDVALIDITY = 1000000000;   // asta e ceva specific unui folder (mailbox)
                                                // ajuta la sincronizrea dintre client si server
                                                // scenariu: clientul descarca mesajele din acest mailbox ca sa le paote accesa offline -> isi creaza in cache de UID pentru sincronziarea cus erverul
                                                //      la urmatoarea conectare la server,
                                                //              daca UIDVALIDITY NU E modificat -> sicronziarea e okay
                                                //              daca UIDVALIDITY e modificat -> sicronziarea nu e okay, se sterg toate UID de la cleint si se resicronizeaza cu serverul
                                                //      


        #region Main and Constructor 
        public Listener(TcpClient client) {
            this.client = client;
            NetworkStream stream = client.GetStream();
            reader = new System.IO.StreamReader(stream);
            writer = new System.IO.StreamWriter(stream);
            writer.NewLine = "\r\n";
            writer.AutoFlush = true;
        }
        public static void Start() {
            //TcpListener listener = new TcpListener(IPAddress.Parse(SMTPServer.ListenerIP), SMTPServer.ListenerPort);
            TcpListener listener = new TcpListener(IPAddress.Any, IMAPServer.ListenerPort);

            listener.Start();
            while (true) {
                Listener handler = new Listener(listener.AcceptTcpClient());
                Thread thread = new System.Threading.Thread(new ThreadStart(handler.Run));
                thread.Start();
            }
        }
        #endregion
        public void Run() {
            try {
                //wr(220, "razvi.rest --  SMTP Server");
                wr("*", "OK details");
                IMAPMessage message = new IMAPMessage();
                bool isUserAuthenticated = false;
                for (; ; ) {
                    string line = rd();
                    if (line == null)
                        break;
                    string[] tokens = line.Split(' ');
                    bool requiresAuthorization = false;
                    switch (tokens[0].ToUpper()) {
                        case "EHLO":
                            wr("250", "AUTH LOGIN PLAIN");
                            break;
                        case "HELP":
                            wr("250", "OK Success - please contact *insert site* ");
                            break;
                        case "HELO":
                            wr("250", "OK Success");
                            break;
                        case "MAIL":
                            message.From = line;
                            wr("250", "OK Success");
                            break;
                        case "RCPT":
                            message.To.Add(line);
                            wr("250", "OK ");
                            break;
                        case "AUTH":
                            //wr(334, "VXNlcm5hbWU6");
                            //string user = rd64();
                            //if (user == null)
                            //    return;
                            //if (user.Length == 0) {
                            //    wr(535, "invalid user");
                            //    break;
                            //}
                            //wr(334, "UGFzc3dvcmQ6");
                            //string pass = rd64();
                            //if (pass == null)
                            //    return;
                            //if (pass.Length == 0) {
                            //    wr(535, "invalid password");
                            //}
                            //if (SMTPServer.UserPass.Equals(user + "-" + pass)) {
                            //    wr(535, "Authentication failed");
                            //    break;
                            //}
                            isUserAuthenticated = true;
                            wr("235", "Authentication succesful");
                            break;
                        case "DATA":
                            if (requiresAuthorization && !isUserAuthenticated) {
                                wr("530", "Authorization Required");
                                break;
                            }
                            wr("354", "End data with <CR><LF>.<CR><LF>");
                            //message.Data.Add(line);
                            for (; ; ) {
                                line = rd();
                                message.Data.Add(line);
                                if ((line == null) || (line == "."))
                                    break;
                            }
                            wr("250", "Ok: queued");
                            message.Save();
                            message = new IMAPMessage();
                            break;
                        case "RSET":
                            wr("250", "Ok: Reset");
                            message = new IMAPMessage();
                            break;
                        case "QUIT":
                            wr("221", "BYE");
                            message.Save();          // AICI,  vezi sa adaugi verificari de rigioare in save. Ca poate mesajul nu e complet !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            writer.Close();
                            client.Close();
                            return;
                        default:
                            //wr(550, "Command not understood");
                            //inseamna ca e mesaj de la IMAPCLIENT
                            ProcessImapMessage(tokens);
                            break;
                    }
                }
            }
            catch (Exception) { }
        }

        // IMAP METHODS
        #region
        private void ProcessImapMessage(string[] tokens) {

            //tokens[0] - tag
            //tokens[1] - command
            //tokens[2]++  - restul informatiei
            switch (tokens[1]) {
                case "CAPABILITY":
                    wr("*", "CAPABILITY IMAP4rev1 NAMESPACE");
                    wr(tokens[0], "OK CAPABILITY completed");
                    break;
                case "LOGIN":
                    LoginResponse(tokens[0], tokens[2], tokens[3]);
                    break;
                case "NAMESPACE":
                    NamespaceResponse(tokens[0]);
                    break;
                case "LIST":
                    ListResponse(tokens[0], tokens[2], tokens[3]);
                    break;
                case "EXAMINE":
                    ExamineResponse(tokens[0]);
                    break;
                case "FETCH":
                    FetchResponse(tokens[0]);
                    break;
                default:
                    wr("*", "BAD");
                    break;
            }
        }
        private void LoginResponse(string tag, string username, string password) {

            if(CheckCredentials(username,password) == true) {

                wr(tag, "OK LOGIN completed");
            }
            else {
                wr(tag, "NO Logon failure: unknown user name or bad password");
            } 
        }
        private void NamespaceResponse(string tag) {

            // personal namespace: INBOX cu delimitatorul "." (se acceseaza foldere: INBOX.sent)
            //NIL NIL -> pentru other's namespace si shared namespec (care nu le configuram)
            wr("*", "NAMESPACE ((\"\" \"/\")) NIL  NIL");
            //wr("*", "NAMESPACE ((\"\" \"/\")) NIL  NIL");
            wr(tag, "OK NAMESPACE completed");
        }
        private void ListResponse(string tag, string reference, string mailbox) {

            //daca ajugnem aici, inseamna ca e un client logat
            //ne intereseaza sa ii listam folderele la care are acces acel client
            //reference + mailbox -> calea in care cautam folderele (relativ la ce stie clientul, nu la PC-ul serverului)

            wr("*", "LIST () \"/\" \"INBOX\"");
            wr(tag, "OK LIST completed");
        }
        private void ExamineResponse(string tag) {

            wr("*", "FLAGS (\\Answered \\Flagged \\Draft \\Deleted \\Seen)");
            wr("*", "OK [PERMANENTFLAGS ()] No permanent flags permitted.");
            wr("*", "OK [UIDVALIDITY 7] UIDs valid.");      //un numar folosit la sincronizarea UID-urilor dintre client si server (daca nu se schimba de la un EXAMINE la altul -> sicnronizarea e okay)
            wr("*", "1 EXISTS");            //cate mailuri sunt in acest mailbox
            wr("*", "0 RECENT");            //cate au aparut de la ultima interogare EXAMINE
            //wr("*", "OK[UNSEEN 1] Message 1 is first unseen");  // asta daca exista mesaje RECENTE!!! -> UID-ul la primul in acest caz
            wr("*", "OK [UIDNEXT 1480] Predicted next UID.");       // UID pt ultimul mail + 1
            wr("*", "OK [HIGHESTMODSEQ 245306]");
            wr(tag, "OK [READ-ONLY] INBOX selected. (Success)");
        }
        private void FetchResponse(string tag) {

            wr("*", "1 FETCH (UID 1315 RFC822.SIZE 5239 BODY (\"TEXT\" \"PLAIN\" NIL NIL NIL \"7BIT\" 549 11))");
            wr(tag, "OK Succes");
        }
        private bool CheckCredentials(string username, string password) {
            //verifica daca exista in fisier acest user

            string[] credentials;
            foreach(var line in File.ReadAllLines(fileCredentials)) {

                credentials = line.Split(" ");
                if (credentials[0] == username && credentials[1] == password)
                    return true;
            }
            return false;
        }
        #endregion
        private void wr(string tag, string c) {
            //writer.WriteLine(code + " " + c);
            writer.WriteLine(tag + " " +c);
            writer.Flush();
            Console.WriteLine("S: " + tag + " " + c);
        }
        private string rd() {
            string result = null;
            try {
                //Console.WriteLine(reader.EndOfStream);
                result = reader.ReadLine();
            }
            catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
            }
            if (result == null)
                Console.WriteLine("C: NULL");
            else {

                Console.WriteLine("C: " + result);
                File.AppendAllText("mailHeader.txt", result);
                File.AppendAllText("mailHeader.txt", "\n");
            }
            return result;
        }
        private string rd64() {
            try {
                string record = rd();
                if (record == null)
                    return null;
                return System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(record));
            }
            catch (Exception) {
                return "";
            }
        }
    }
}