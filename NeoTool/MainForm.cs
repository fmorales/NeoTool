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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoTool {
    public partial class MainForm:Form {
        public API api = new API();

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

            //kryptonNavigator1.StateCommon.RibbonGeneral.TextFont = new Font("Cambria Math", (float)8.25);
        }

        public void AddAccount(object s = null, EventArgs e = null) {
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

        public void UpdateComboBox() {
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
            kryptonHeaderGroup1.Height = kryptonPanel2.Height - 87;
            //kryptonNavigator1.Width = this.Width - 216;
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
            if (node == null) return;

            if (((FileData)node.Tag).info.IsDirectory) return;

            api.username = kryptonComboBox1.Text;
            api.password = Settings.Default.Accounts.Find(x => x.username == kryptonComboBox1.Text).password;

            foreach (KryptonPage p in kryptonNavigator1.Pages)
                if (((FileData)p.Tag).info.FilePath == ((FileData)node.Tag).info.FilePath && ((FileData)p.Tag).site == api.username) {
                    kryptonNavigator1.SelectedPage = p;
                    return;
                }

            foreach (string s in new string[]{".gif", ".jpeg", ".jpg", ".png"})
                if (node.Text.EndsWith(s)) {
                    MessageBox.Show("Images cannot be opened yet.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

            Cursor.Current = Cursors.WaitCursor;
            KryptonPage kp = new KryptonPage(node.Text, imageList1.Images[node.ImageIndex], api.username + "@" + ((FileData)node.Tag).info.FilePath);
            kp.Tag = ((FileData)node.Tag);
            ((FileData)kp.Tag).originalTitle = kp.Text;
            kp.ToolTipTitle = $"Site: {api.username}\nPath: {((FileData)node.Tag).info.FilePath}";
            kryptonNavigator1.Pages.Add(kp);

            FastColoredTextBox fctb = new FastColoredTextBox();

            if (node.Text.EndsWith(".htm")) fctb.Language = Language.HTML;
            else if (node.Text.EndsWith(".html")) fctb.Language = Language.HTML;
            else if (node.Text.EndsWith(".js")) fctb.Language = Language.JS;

            fctb.Name = "FastColoredTextBox";
            fctb.Dock = DockStyle.Fill;
            fctb.TabLength = 2;
            fctb.Text = api.GetFile(((FileData)node.Tag).info.FilePath);
            ((FileData)kp.Tag).originalText = fctb.Text;
            fctb.TextChanged += (s, ea) => {
                if (fctb.Text != ((FileData)kp.Tag).originalText && !((FileData)kp.Tag).modified) {
                    ((FileData)kp.Tag).modified = true;
                    kp.Text = "⁎ " + kp.Text;
                }
            };

            kp.Controls.Add(fctb);
            Cursor.Current = Cursors.Default;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            bool hasUnsavedFiles = false;

            foreach (KryptonPage p in kryptonNavigator1.Pages) if (((FileData)p.Tag).modified) hasUnsavedFiles = true;

            if (hasUnsavedFiles) {
                DialogResult result = MessageBox.Show(this, "Do you want to save the changes you have made?\nIf you don't save, your work will be lost.", "NeoTool", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                switch(result) {
                    case DialogResult.Yes:
                        return;
                    case DialogResult.No:
                        return;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                }
            }
        }

        private void kryptonNavigator1_CloseAction(object sender, CloseActionEventArgs e) {
            if (((FileData)e.Item.Tag).modified) {
                DialogResult result = MessageBox.Show(this, "Do you want to save the changes you have made to "+ ((FileData)e.Item.Tag).originalTitle +"?\nIf you don't save, your work will be lost.", "NeoTool", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                switch (result) {
                    case DialogResult.Yes:
                        return;
                    case DialogResult.No:
                        return;
                    case DialogResult.Cancel:
                        e.Action = CloseButtonAction.None;
                        return;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            switch (keyData) {
                case (Keys.Control | Keys.N):
                    newToolStripMenuItem.PerformClick();
                    break;
                case (Keys.Control | Keys.S):
                    saveToolStripMenuItem.PerformClick();
                    break;
                case (Keys.Control | Keys.Shift | Keys.I):
                    importToolStripMenuItem.PerformClick();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            FastColoredTextBox fctb = (FastColoredTextBox)kryptonNavigator1.SelectedPage.Controls.Find("FastColoredTextBox", true)[0];

            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp"));
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp\\neotemp.html"), fctb.Text);

            api.username = ((FileData)kryptonNavigator1.SelectedPage.Tag).site;
            api.password = Settings.Default.Accounts.Find(x => x.username == ((FileData)kryptonNavigator1.SelectedPage.Tag).site).password;

            api.Upload(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp\\neotemp.html"), ((FileData)kryptonNavigator1.SelectedPage.Tag).info.FilePath);

            ((FileData)kryptonNavigator1.SelectedPage.Tag).originalText = fctb.Text;
            kryptonNavigator1.SelectedPage.Text = ((FileData)kryptonNavigator1.SelectedPage.Tag).originalTitle;
            ((FileData)kryptonNavigator1.SelectedPage.Tag).modified = false;
        }

        private void kryptonButton2_Click(object sender, EventArgs e) {
            if (kryptonTreeView1.SelectedNode == null) return;

            DialogResult result = MessageBox.Show(this, "Are you sure you want to delete " + ((FileData)kryptonTreeView1.SelectedNode.Tag).info.FilePath + "?\nThis cannot be undone!", "NeoTool", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (result == DialogResult.Yes) {
                api.username = kryptonComboBox1.Text;
                api.password = Settings.Default.Accounts.Find(x => x.username == kryptonComboBox1.Text).password;

                foreach (KryptonPage p in kryptonNavigator1.Pages)
                    if (((FileData)p.Tag).info.FilePath == ((FileData)kryptonTreeView1.SelectedNode.Tag).info.FilePath) {
                        kryptonNavigator1.Pages.Remove(p);
                        return;
                    }

                api.Delete(((FileData)kryptonTreeView1.SelectedNode.Tag).info.FilePath);
                PopulateTreeView();
            }
        }

        private void kryptonButton6_Click(object sender, EventArgs e) {
            if (kryptonComboBox1.Text == null) return;

            api.username = kryptonComboBox1.Text;
            api.password = Settings.Default.Accounts.Find(x => x.username == kryptonComboBox1.Text).password;
            PopulateTreeView();
        }

        private void kryptonButton5_Click(object sender, EventArgs e) {
            api.username = kryptonComboBox1.Text;
            api.password = Settings.Default.Accounts.Find(x => x.username == kryptonComboBox1.Text).password;

            if (kryptonTreeView1.SelectedNode != null) {
                TreeDialog td = new TreeDialog(this, true);
                string expectedExtension = "." + kryptonTreeView1.SelectedNode.Text.Split('.').Last();
                td.kryptonTextBox1.Text = kryptonTreeView1.SelectedNode.Text;
                while (true) {
                    DialogResult result = td.ShowDialog();

                    if (result == DialogResult.Cancel) return;

                    if (td.kryptonTextBox1.Text.EndsWith(expectedExtension)) {
                        if (td.kryptonTreeView1.SelectedNode != null) {
                            if (((FileData)td.kryptonTreeView1.SelectedNode.Tag).info.FilePath + "\\" + td.kryptonTextBox1.Text != kryptonTreeView1.SelectedNode.Text) {
                                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp"));
                                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp\\neotemp.html"), api.GetFile(kryptonTreeView1.SelectedNode.Text));

                                api.Delete(((FileData)kryptonTreeView1.SelectedNode.Tag).info.FilePath);

                                api.Upload(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp\\neotemp.html"), ((FileData)td.kryptonTreeView1.SelectedNode.Tag).info.FilePath + "\\" + td.kryptonTextBox1.Text);
                                td.Close();
                                PopulateTreeView();
                                return;
                            } else {
                                MessageBox.Show("Please enter a new filename or select a new destination.");
                                continue;
                            }
                        } else {
                            MessageBox.Show("Please select a destination folder.");
                            continue;
                        }
                    } else {
                        MessageBox.Show($"File needs to end in \"{expectedExtension}\".");
                        continue;
                    }
                }
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e) {
            if (kryptonComboBox1.Text == "") return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.Filter = "All allowed filetypes (*.html;*.htm;*.jpg;*.png;*.gif;*.svg;*.ico;*.md;*.markdown;*.js;*.json;*.geojson;*.css;*.txt;*.text;*.csv;*.tsv;*.xml;*.eot;*.ttf;*.woff;*.woff2;*.mid;*.midi)|*.html;*.htm;*.jpg;*.png;*.gif;*.svg;*.ico;*.md;*.markdown;*.js;*.json;*.geojson;*.css;*.txt;*.text;*.csv;*.tsv;*.xml;*.eot;*.ttf;*.woff;*.woff2;*.mid;*.midi";
            DialogResult result = ofd.ShowDialog();

            api.username = kryptonComboBox1.Text;
            api.password = Settings.Default.Accounts.Find(x => x.username == kryptonComboBox1.Text).password;

            if (result == DialogResult.OK) {
                TreeDialog td = new TreeDialog(this, true);
                string expectedExtension = "." + ofd.SafeFileName.Split('.').Last();
                td.kryptonTextBox1.Text = ofd.SafeFileName;
                while (true) {
                    result = td.ShowDialog();

                    if (result == DialogResult.Cancel) return;

                    if (td.kryptonTextBox1.Text.EndsWith(expectedExtension)) {
                        if (td.kryptonTreeView1.SelectedNode != null) {
                            api.Upload(ofd.FileName, ((FileData)td.kryptonTreeView1.SelectedNode.Tag).info.FilePath + "\\" + td.kryptonTextBox1.Text);
                            td.Close();
                            return;
                        } else {
                            MessageBox.Show("Please select a destination folder.");
                            continue;
                        }
                    } else {
                        MessageBox.Show($"File needs to end in \"{expectedExtension}\".");
                        continue;
                    }
                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e) {
            if (kryptonComboBox1.Text == "") return;

            api.username = kryptonComboBox1.Text;
            api.password = Settings.Default.Accounts.Find(x => x.username == kryptonComboBox1.Text).password;

            TreeDialog td = new TreeDialog(this, true);
            while (true) {
                DialogResult result = td.ShowDialog();

                if (result == DialogResult.Cancel) return;

                if (td.kryptonTextBox1.Text.EndsWith(".htm") | td.kryptonTextBox1.Text.EndsWith(".html") | td.kryptonTextBox1.Text.EndsWith(".css") | td.kryptonTextBox1.Text.EndsWith(".js") | td.kryptonTextBox1.Text.EndsWith(".json") | td.kryptonTextBox1.Text.EndsWith(".md") | td.kryptonTextBox1.Text.EndsWith(".geojson") | td.kryptonTextBox1.Text.EndsWith(".markdown") | td.kryptonTextBox1.Text.EndsWith(".txt") | td.kryptonTextBox1.Text.EndsWith(".xml") | td.kryptonTextBox1.Text.EndsWith(".text") | td.kryptonTextBox1.Text.EndsWith("csv") | td.kryptonTextBox1.Text.EndsWith(".tsv")) {
                    if (td.kryptonTreeView1.SelectedNode != null) {
                        Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp"));
                        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp\\neotemp.html"), "Blank file created by NeoTool.");

                        api.Upload(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp\\neotemp.html"), ((FileData)td.kryptonTreeView1.SelectedNode.Tag).info.FilePath + "\\" + td.kryptonTextBox1.Text);
                        PopulateTreeView();
                        td.Close();
                        return;
                    } else {
                        MessageBox.Show("Please select a destination folder.");
                        continue;
                    }
                } else {
                    MessageBox.Show("File needs to end in an allowed extension.\n(.htm, .html, .md, .markdown, .css, .js, .json, .geojson, .xml, .txt, .text, .csv, .tsv)");
                    continue;
                }
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        private void ToolStripToFCTB(object sender, EventArgs e) {
            if (kryptonNavigator1.Pages.Count != 0) {
                var fctb = ((FastColoredTextBox)kryptonNavigator1.SelectedPage.Controls.Find("FastColoredTextBox", true)[0]);

                if (sender == undoToolStripMenuItem) fctb.Undo();
                else if (sender == redoToolStripMenuItem) fctb.Redo();
                else if (sender == cutToolStripMenuItem) fctb.Cut();
                else if (sender == copyToolStripMenuItem) fctb.Copy();
                else if (sender == pasteToolStripMenuItem) fctb.Paste();
            }
        }

        private void openInBrowserToolStripMenuItem_Click(object sender, EventArgs e) {
            if (kryptonNavigator1.Pages.Count != 0) {
                System.Diagnostics.Process.Start($"https://{((FileData)kryptonNavigator1.SelectedPage.Tag).site}.neocities.org/{((FileData)kryptonNavigator1.SelectedPage.Tag).info.FilePath}");
            }
        }

        private void aboutNeoToolToolStripMenuItem_Click(object sender, EventArgs e) {
            new AboutBox().ShowDialog();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (kryptonNavigator1.Pages.Count != 0) {
                SaveFileDialog sfd = new SaveFileDialog();
                DialogResult r = sfd.ShowDialog();

                if (r == DialogResult.OK) {
                    File.WriteAllText(sfd.FileName, ((FastColoredTextBox)kryptonNavigator1.SelectedPage.Controls.Find("FastColoredTextBox", true)[0]).Text);
                }
            }
        }

        private void kryptonButton3_Click(object sender, EventArgs e) {
            new AccountManager(this).ShowDialog();
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

    public class FileData {
        public APIFileInfo info;
        public string site;
        public string originalText;
        public string originalTitle;
        public bool modified;

        public FileData(APIFileInfo afi, string s) {
            info = afi;
            site = s;
            originalText = "";
            modified = false;
        }
    }
}
