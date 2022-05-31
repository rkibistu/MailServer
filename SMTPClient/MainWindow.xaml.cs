using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client {
    public static class Globals {

        static public IClient m_client = new CClient();
        static public IAdmin m_admin = null;
    }
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            MyConnection.Instance.ConnectToServer();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

            //MyConnection.Instance.SMTP_read();
            //MyConnection.Instance.SMTP_write("AUTH ");
            MyConnection.Instance.SMTP_read();
            MyConnection.Instance.SMTP_write("HELO client.net");
            MyConnection.Instance.SMTP_read();
            MyConnection.Instance.SMTP_write("AUTH");
            MyConnection.Instance.SMTP_read();
            MyConnection.Instance.SMTP_write("MAIL FROM:<da@razvi.rest>");
            MyConnection.Instance.SMTP_read();
            MyConnection.Instance.SMTP_write("RCPT TO:<ogreanrazvan@gmail.com>");
            MyConnection.Instance.SMTP_read();
            MyConnection.Instance.SMTP_write("DATA");
            MyConnection.Instance.SMTP_read();
            MyConnection.Instance.SMTP_write("DKIM-Signature: v=DKIM1; t=s; b=MIICWwIBAAKBgQC1l3aqtydDa4GhxT3+V9CtrDSnfHA1F/PDsr0sLxBtFLKZHdDe Lp7TvA9Mp/y1wcDHLeDvMsqHOl7XFFpliCvUcrHoy/0jLciR4xPkKsJHUzFKl90W JddvvQ3J2cuED2UbuGn7MS9lsFB5JrKwMNLKZqLq2IGDRWJo+IrhjkCkdQIDAQAB AoGAb+nnApNaKVzqSY7MBCKOw1osIUS5yp72ZpeTqtVepDtrTp5niWbmD8wJCc9G GaZNbvPBeumsk+bPaHJsu8JOb9eK+qO035qahFqWNLJjCI9L8fpzIHcRb5jF/0hr JgkKtzTL6cOYl56iAmhVMh9EAMEmVVPIbcGdgGcvafWmx0kCQQDqQKEjLj95F+mk xltnKuIcJ00lwJvZHgAVVuCqs4/Sy0TnnuMUs8D/hp4rQBppf2aBcv9gnwFl/Wxp dqPestTTAkEAxnNE2oPzcMgj+IQFn3Ph/Jw6yMxU6m6g45M2uQ+ScyFMMIS5Ufmj mvZK0u+yxEHbzocN7ApNLTG8WTsZfOT0lwJAPY+kiUFTLvioz0PNq4wqhemSLbSz gFiQ/wqo2lN8HZKL1i78UGl48+4lzQn4pvbzMlvNX1AKJwp1njWulQz8PwJAC2N4 ExY5dUJ3Ff71l6X91RJeKUHYqa95mjXLkSk8nVDO34XuYK7z3aO+vYY1+x9QbvS/ LsmUjkBpb7F9j/ZPtwJATcj6ZkJ23FUziRg7yPj1Y1yB4RPijzENkQrZ0CrKd1Vb vDtKzPP1d4EG+dindldri+NhqJSa0O474C0WYX25Gg==");
            MyConnection.Instance.SMTP_write("test");
            MyConnection.Instance.SMTP_write("aici nu is sigur");
            MyConnection.Instance.SMTP_write(".");
            MyConnection.Instance.SMTP_read();
            MyConnection.Instance.SMTP_write("QUIT");
            MyConnection.Instance.SMTP_read();



        }


        private void SMTP_read() {


        }
    }
}
