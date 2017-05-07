using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Ini;

namespace Kudo_Scraper
{
    public partial class Form1 : Form
    {
        database db = new database();
        DataTable dttb;
        String kartu;
        String kode;
        String nomor;
        String lanjut;
        String count;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Text += Environment.NewLine + "Step 1 : Kudo Mulai jalan";
            webBrowser1.Navigate("https://kudo.co.id/shop/agent");

            var path = Application.StartupPath + "\\setting.ini";
            IniFile ini = new IniFile(path);
            
            txtpin.Text = ini.IniReadValue("Info", "pin");
            count = ini.IniReadValue("Info", "count");
            txtport.Text = ini.IniReadValue("Info", "port");
            Form1 f1 = new Form1();
            f1.Text = "Kudo Scraper | COM : " + ini.IniReadValue("Info", "port");
        }

        private void MulaiLagi()
        {
            //MessageBox.Show("a");
            txthitung.Text = "0";

            timer1.Enabled = true;
            lblstatus.Text = "Menunggu Transaksi";
            lblstatus.ForeColor = Color.Red;
        }

        private void CekOutbox()
        {
            dttb = db.SQLCari("select no_hp,nama,pesan,com from sms_outbox where com = '" + txtport.Text + "' order by id limit 1");
            if (dttb.Rows.Count > 0)
            {
                richTextBox1.Text += Environment.NewLine + "Step 3 : Outbox ditemukan";
                //MessageBox.Show("b");
                lblstatus.Text = "Transaksi Aktif";
                lblstatus.ForeColor = Color.Green;
                try
                {
                    
                    //MessageBox.Show("c");
                    String[] pesan = dttb.Rows[0]["pesan"].ToString().Split('|');
                    kartu = pesan[0].ToLower();
                    kode = "Voucher Rp" + pesan[1];
                    nomor = pesan[2];
                    richTextBox1.Text += Environment.NewLine + "Step 4 : " + kartu + " -- " + kode + " -- " + nomor ;
                    webBrowser1.Navigate("https://kudo.co.id/shop/pulsa/" + kartu + "/?no=" + nomor + "");
                    timer1.Enabled = false;
                    webBrowser1.DocumentCompleted += ProsesPilihPulsa;
                    db.SQLQuery("delete from sms_outbox where com='" + txtport.Text + "' order by id limit 1");
                }
                catch(Exception ex)
                {
                    MessageBox.Show(kartu + "." + kode + "." + nomor + " | " + ex);
                }
                
            }
        }

        private void ProsesPilihPulsa(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //richTextBox1.Text += Environment.NewLine + "Step 5 : Klik Denom";
            ClickDenom();
            //MessageBox.Show("h");
            if (lanjut == "ya")
            {
                //richTextBox1.Text += Environment.NewLine + "Step 7 : Klik Beli Pulsa";
                //MessageBox.Show("i");
                HtmlElement btn = webBrowser1.Document.GetElementsByTagName("button")[0];
                btn.InvokeMember("click");

                webBrowser1.DocumentCompleted += PembelianPulsa;
                
            }
            else
            {
                //MessageBox.Show("j");
            }

            

        }

        private void ClickDenom()
        {
            HtmlDocument doc = webBrowser1.Document;
            HtmlElementCollection col = doc.GetElementsByTagName("li");
            foreach (HtmlElement element in col)
            {
                string cls = element.GetAttribute("data-item");
                if (cls == kode)
                {
                    //MessageBox.Show("d");
                    //richTextBox1.Text += Environment.NewLine + "Step 6 : Denom ditemukan";
                    lanjut = "ya";
                    element.InvokeMember("click");
                    break;
                }else
                {
                    //MessageBox.Show("e");
                }
            }
            if(lanjut == "ya")
            {
                //MessageBox.Show("f");
            }
            else
            {
                richTextBox1.Text += Environment.NewLine + "Step 5 : " + nomor + " Denom tidak ada, Transaksi gagal !!";
                //MessageBox.Show("g");
                lanjut = "tidak";

                db.SQLQuery("insert into sms_inbox(sourceno,nama,tgl,text,type,port,ip,iscentre) select 'kudo',nama,now(),'" + nomor + " gagal',13,'200','192.168.2.3',1 from distributor where replyno='kudo'");
                MulaiLagi();
            }

        }

        private void PembelianPulsa(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            String web = webBrowser1.Url.ToString();
            if (web.Contains("transaksi-aktif"))
            {
                HtmlElement btn = webBrowser1.Document.GetElementsByTagName("button")[5];
                btn.InvokeMember("click");
                webBrowser1.Document.GetElementById("check").Focus();
                webBrowser1.Document.GetElementById("check").InnerText = txtpin.Text;
                HtmlElement btn2 = webBrowser1.Document.GetElementsByTagName("button")[8];
                btn2.InvokeMember("click");
                db.SQLQuery("insert into sms_inbox(sourceno,nama,tgl,text,type,port,ip,iscentre) select 'kudo',nama,now(),'" + nomor + " sukses dgn SN=" + DateTime.Now.ToString("ddMMHHmmss") + ".', 13,'200','192.168.2.3',1 from distributor where replyno='kudo'");
            }

            if(web.Contains("confirm/success"))
            {
                MulaiLagi();
            }

            //timer1.Enabled = true;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int hitung = 0;
            hitung++;
            txthitung.Text = (Convert.ToInt32(txthitung.Text) + hitung).ToString();
            if (txthitung.Text == count)
            {
                CekOutbox();
                txthitung.Text = "0";

            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            var linecount = richTextBox1.Lines.Count();
            if(linecount == 100)
            {
                richTextBox1.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(btnonoff.Text == "ON")
            {
                MulaiLagi();
                btnonoff.Text = "OFF";
            }else
            {
                btnonoff.Text = "ON";
                timer1.Enabled = false;
                lblstatus.Text = "Menunggu Transaksi";
            }
            
        }
    }
}
