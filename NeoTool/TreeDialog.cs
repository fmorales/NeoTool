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
    public partial class TreeDialog : Form {
        MainForm mf;

        public TreeDialog(MainForm mf, bool dirsOnly = false) {
            InitializeComponent();

            this.mf = mf;

            Cursor.Current = Cursors.WaitCursor;
            kryptonTreeView1.Nodes.Clear();

            List<APIFileInfo> files = mf.api.GetFileInfos();

            // thanks to https://stackoverflow.com/questions/1155977/
            TreeNode lastNode = null;
            string subPathAgg;
            foreach (APIFileInfo f in files) {
                subPathAgg = string.Empty;
                foreach (string subPath in (mf.api.username + "/" + f.FilePath).Split('/')) {
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
                /*else if (f.FilePath.EndsWith(".htm")) lastNode.ImageIndex = 2;
                else if (f.FilePath.EndsWith(".html")) lastNode.ImageIndex = 2;
                else if (f.FilePath.EndsWith(".css")) lastNode.ImageIndex = 3;
                else if (f.FilePath.EndsWith(".js")) lastNode.ImageIndex = 4;
                else if (f.FilePath.EndsWith(".gif")) lastNode.ImageIndex = 5;
                else if (f.FilePath.EndsWith(".jpeg")) lastNode.ImageIndex = 5;
                else if (f.FilePath.EndsWith(".jpg")) lastNode.ImageIndex = 5;
                else if (f.FilePath.EndsWith(".png")) lastNode.ImageIndex = 5;
                else lastNode.ImageIndex = 1;*/

                lastNode.SelectedImageIndex = lastNode.ImageIndex;
                lastNode.Tag = new FileData(f, mf.api.username);

                if (!f.IsDirectory && dirsOnly) kryptonTreeView1.Nodes.Remove(lastNode);
                lastNode = null;
            }

            APIFileInfo afi = new APIFileInfo {
                FilePath = "",
                IsDirectory = true,
                Size = 0
            };

            kryptonTreeView1.Nodes[0].SelectedImageIndex = 1;
            kryptonTreeView1.Nodes[0].ImageIndex = 1;
            kryptonTreeView1.Nodes[0].Tag = new FileData(afi, mf.api.username);

            Cursor.Current = Cursors.Default;
        }

        private void kryptonButton3_Click(object sender, EventArgs e) {
            TreeNode node = new TreeNode();

            TextDialog text = new TextDialog();
            while (true) {
                DialogResult r = text.ShowDialog();

                if (r == DialogResult.OK) {
                    if (text.kryptonTextBox1.Text != "")
                        if (!text.kryptonTextBox1.Text.Contains(".")) {
                            node.Text = text.kryptonTextBox1.Text;
                            FileData fd = new FileData(new APIFileInfo(), mf.api.username);
                            fd.info.FilePath = ((FileData)kryptonTreeView1.SelectedNode.Tag).info.FilePath + "\\" + node.Text;
                            fd.info.IsDirectory = true;
                            node.Tag = fd;

                            kryptonTreeView1.SelectedNode.Nodes.Add(node);
                            return;
                        } else {
                            MessageBox.Show("Folder names cannot contain periods.");
                            continue;
                        }
                } else {
                    return;
                }
            }
        }
    }
}
