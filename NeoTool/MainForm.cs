using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Toolkit;
using FastColoredTextBoxNS;
using NeoTool.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoTool {
    public partial class MainForm:Form {
        API api = new API();

        string oldComboText;

        public MainForm() {
            InitializeComponent();

            // Init/load settings
            if (Settings.Default.Accounts == null) {
                Settings.Default.Accounts = new List<AccountData>();
                AddAccount();
            } else if (Settings.Default.Accounts.Count == 0) {
                Settings.Default.Accounts = new List<AccountData>();
                AddAccount();
            } else {
                UpdateComboBox();
            }

            oldComboText = kryptonComboBox1.Text;
        }

        private void AddAccount(object s = null, EventArgs e = null) {
            LoginDialog login = new LoginDialog();
            while (true) {
                DialogResult result = login.ShowDialog();

                if (result == DialogResult.Cancel) {
                    if (s == null) Environment.Exit(0);
                    else {
                        login.Close();
                        return;
                    }
                }

                api.username = login.kryptonTextBox1.Text;
                api.password = login.kryptonTextBox2.Text;
                
                if (api.GetKey() == null) continue;
                else {
                    Settings.Default.Accounts.Add(new AccountData(login.kryptonTextBox1.Text, login.kryptonTextBox2.Text));
                    break;
                }
            }
            Settings.Default.Save();
            UpdateComboBox();
            kryptonTreeView1.Nodes.Clear();
            api.username = "";
            api.password = "";
        }

        private void UpdateComboBox() {
            kryptonComboBox1.Items.Clear();
            List<string> sites = new List<string>();
            foreach (AccountData a in Settings.Default.Accounts) sites.Add(a.username);
            kryptonComboBox1.Items.AddRange(sites.ToArray());
        }

        private void PopulateTreeView() {
            Cursor.Current = Cursors.WaitCursor;
            kryptonTreeView1.Nodes.Clear();

            List<APIFileInfo> files = api.GetFileInfos();

            // thanks to https://stackoverflow.com/questions/1155977/
            TreeNode lastNode = null;
            string subPathAgg;
            foreach (APIFileInfo f in files) {
                subPathAgg = string.Empty;
                foreach (string subPath in f.FilePath.Split('/')) {
                    subPathAgg += subPath + '/';
                    TreeNode[] nodes = kryptonTreeView1.Nodes.Find(subPathAgg, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                            lastNode = kryptonTreeView1.Nodes.Add(subPathAgg, subPath);
                        else
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
                    else
                        lastNode = nodes[0];
                }
                if (f.IsDirectory) lastNode.ImageIndex = 0;
                else if (f.FilePath.EndsWith(".htm")) lastNode.ImageIndex = 2;
                else if (f.FilePath.EndsWith(".html")) lastNode.ImageIndex = 2;
                else if (f.FilePath.EndsWith(".css")) lastNode.ImageIndex = 3;
                else if (f.FilePath.EndsWith(".js")) lastNode.ImageIndex = 4;
                else if (f.FilePath.EndsWith(".gif")) lastNode.ImageIndex = 5;
                else if (f.FilePath.EndsWith(".jpeg")) lastNode.ImageIndex = 5;
                else if (f.FilePath.EndsWith(".jpg")) lastNode.ImageIndex = 5;
                else if (f.FilePath.EndsWith(".png")) lastNode.ImageIndex = 5;
                else lastNode.ImageIndex = 1;

                lastNode.SelectedImageIndex = lastNode.ImageIndex;
                lastNode.Tag = new FileData(f, api.username);

                lastNode = null;
            }

            Cursor.Current = Cursors.Default;
        }

        private void MainForm_Resize(object sender, EventArgs e) {
            kryptonHeaderGroup1.Height = kryptonPanel2.Height - 114;
            kryptonNavigator1.Width = this.Width - 216;
        }

        private void kryptonComboBox1_SelectedValueChanged(object sender, EventArgs e) {
            foreach (AccountData a in Settings.Default.Accounts) {
                if (kryptonComboBox1.Text == a.username) {
                    api.username = a.username;
                    api.password = a.password;
                    PopulateTreeView();
                    return;
                }
            }
            MessageBox.Show(this, "NeoTool tried to access an account it shouldn't have tried to access. This shouldn't happen at all, so please let lempamo know.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void kryptonTreeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
            TreeNode node = ((KryptonTreeView)sender).SelectedNode;

            foreach (string s in new string[]{".gif", ".jpeg", ".jpg", ".png"})
                if (node.Text.EndsWith(s)) {
                    MessageBox.Show("Images cannot be opened yet.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

            KryptonPage kp = new KryptonPage(node.Text, imageList1.Images[node.ImageIndex], api.username + "@" + ((FileData)node.Tag).info.FilePath);
            kryptonNavigator1.Pages.Add(kp);

            FastColoredTextBox fctb = new FastColoredTextBox();

            if (node.Text.EndsWith(".htm")) fctb.Language = Language.HTML;
            else if (node.Text.EndsWith(".html")) fctb.Language = Language.HTML;
            else if (node.Text.EndsWith(".js")) fctb.Language = Language.JS;

            fctb.Dock = DockStyle.Fill;
            fctb.Text = api.GetFile(((FileData)node.Tag).info.FilePath);

            kp.Controls.Add(fctb);
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

    public struct FileData {
        public APIFileInfo info;
        public string site;

        public FileData(APIFileInfo afi, string s) {
            info = afi;
            site = s;
        }
    }
}
