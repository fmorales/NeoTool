namespace NeoTool {
    partial class TreeDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.kryptonTreeView1 = new ComponentFactory.Krypton.Toolkit.KryptonTreeView();
            this.kryptonButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.kryptonButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.kryptonTextBox1 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.SuspendLayout();
            // 
            // kryptonTreeView1
            // 
            this.kryptonTreeView1.Location = new System.Drawing.Point(12, 12);
            this.kryptonTreeView1.Name = "kryptonTreeView1";
            this.kryptonTreeView1.Size = new System.Drawing.Size(195, 226);
            this.kryptonTreeView1.TabIndex = 0;
            // 
            // kryptonButton1
            // 
            this.kryptonButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.kryptonButton1.Location = new System.Drawing.Point(13, 273);
            this.kryptonButton1.Name = "kryptonButton1";
            this.kryptonButton1.Size = new System.Drawing.Size(90, 25);
            this.kryptonButton1.TabIndex = 1;
            this.kryptonButton1.Values.Text = "Select";
            // 
            // kryptonButton2
            // 
            this.kryptonButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.kryptonButton2.Location = new System.Drawing.Point(117, 273);
            this.kryptonButton2.Name = "kryptonButton2";
            this.kryptonButton2.Size = new System.Drawing.Size(90, 25);
            this.kryptonButton2.TabIndex = 2;
            this.kryptonButton2.Values.Text = "Cancel";
            // 
            // kryptonTextBox1
            // 
            this.kryptonTextBox1.Location = new System.Drawing.Point(12, 244);
            this.kryptonTextBox1.Name = "kryptonTextBox1";
            this.kryptonTextBox1.Size = new System.Drawing.Size(195, 23);
            this.kryptonTextBox1.TabIndex = 3;
            this.kryptonTextBox1.Text = "kryptonTextBox1";
            this.kryptonTextBox1.Visible = false;
            // 
            // TreeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(206)))), ((int)(((byte)(230)))));
            this.ClientSize = new System.Drawing.Size(219, 310);
            this.Controls.Add(this.kryptonTextBox1);
            this.Controls.Add(this.kryptonButton2);
            this.Controls.Add(this.kryptonButton1);
            this.Controls.Add(this.kryptonTreeView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "TreeDialog";
            this.Text = "Select";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonTreeView kryptonTreeView1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton kryptonButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton kryptonButton2;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox kryptonTextBox1;
    }
}