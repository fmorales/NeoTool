using ComponentFactory.Krypton.Navigator;
using NeoTool.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoTool {
    public partial class AccountManager : Form {
        private MainForm MF;

        public AccountManager(MainForm mf) {
            InitializeComponent();

            MF = mf;

            foreach (AccountData ad in Settings.Default.Accounts) kryptonListBox1.Items.Add(ad.username);
        }

        private void kryptonButton1_Click(object sender, EventArgs e) {
            if (kryptonListBox1.SelectedItem == null) return;

            LoginDialog login = new LoginDialog();
            login.kryptonTextBox1.Text = (string)kryptonListBox1.SelectedItem;
            login.kryptonTextBox1.Enabled = false;
            while (true) {
                DialogResult result = login.ShowDialog();

                if (result == DialogResult.Cancel) return;

                MF.api.username = login.kryptonTextBox1.Text;
                MF.api.password = login.kryptonTextBox2.Text;

                if (MF.api.GetKey() == null) continue;
                else {
                    int i = Settings.Default.Accounts.IndexOf(Settings.Default.Accounts.Find(x => x.username == login.kryptonTextBox1.Text));
                    Settings.Default.Accounts[i] = new AccountData(login.kryptonTextBox1.Text, login.kryptonTextBox2.Text);
                    break;
                }
            }
            Settings.Default.Save();
        }

        private void kryptonButton2_Click(object sender, EventArgs e) {
            if (kryptonListBox1.SelectedItem == null) return;

            if (MessageBox.Show("Are you sure you wish to remove this account from NeoTool?", "NeoTool", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                if (MF.kryptonNavigator1.Pages.Count > 0) {
                    foreach (KryptonPage kp in MF.kryptonNavigator1.Pages)
                        if (((FileData)kp.Tag).site == (string)kryptonListBox1.SelectedItem)
                            MF.kryptonNavigator1.Pages.Remove(kp);
                }
                AccountData a = Settings.Default.Accounts.Find(x => x.username == (string)kryptonListBox1.SelectedItem);
                Settings.Default.Accounts.Remove(a);
                Settings.Default.Save();
                MF.UpdateComboBox();
                MF.kryptonTreeView1.Nodes.Clear();
                MF.api.username = "";
                MF.api.password = "";

                kryptonListBox1.Items.Clear();
                foreach (AccountData ad in Settings.Default.Accounts) kryptonListBox1.Items.Add(ad.username);
            }
        }
    }
}
