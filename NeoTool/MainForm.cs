using NeoTool.Properties;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoTool {
    public partial class MainForm:Form {
        public RestClient client = new RestClient("http://neocities.org/api/");

        public MainForm() {
            InitializeComponent();

            // Init/load settings
            if (Settings.Default.Accounts == null) {
                Settings.Default.Accounts = new List<AccountData>();
                AddAccount();
            } else if (Settings.Default.Accounts.Count == 0) {
                Settings.Default.Accounts = new List<AccountData>();
                AddAccount();
            }

            List<string> sites = new List<string>();
            foreach (AccountData a in Settings.Default.Accounts) sites.Add(a.username);
            kryptonComboBox1.Items.AddRange(sites.ToArray());
        }

        private void AddAccount(object s = null, EventArgs e = null) {
            LoginDialog login = new LoginDialog();
            while (true) {
                DialogResult result = login.ShowDialog();

                if (result == DialogResult.Cancel)
                    if (s == null) Environment.Exit(0);

                var client = new RestClient("http://neocities.org/api/");
                client.Authenticator = new HttpBasicAuthenticator(login.kryptonTextBox1.Text, login.kryptonTextBox2.Text);

                var request = new RestRequest("key", Method.GET, DataFormat.Json);

                dynamic data = JsonConvert.DeserializeObject(client.Execute(request).Content);
                if (data.result == "error") {
                    if (data.error_type == "invalid_auth") {
                        MessageBox.Show(login, "Invalid username and/or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }
                } else {
                    Settings.Default.Accounts.Add(new AccountData(login.kryptonTextBox1.Text, login.kryptonTextBox2.Text));
                    break;
                }
            }
            Settings.Default.Save();
        }
    }

    public struct AccountData {
        public string username;
        public string password;

        public AccountData(string u, string p) {
            username = u;
            password = p;
        }
    }
}
