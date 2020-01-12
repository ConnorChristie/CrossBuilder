namespace CrossCapture.GUI.Core
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fileTree = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.nextButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.expandAll = new System.Windows.Forms.Button();
            this.collapseAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.outputDirectory = new System.Windows.Forms.TextBox();
            this.selectOutputDir = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.prefixText = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileTree
            // 
            this.fileTree.CheckBoxes = true;
            this.fileTree.Location = new System.Drawing.Point(21, 50);
            this.fileTree.Margin = new System.Windows.Forms.Padding(12);
            this.fileTree.Name = "fileTree";
            this.fileTree.PathSeparator = "";
            this.fileTree.ShowLines = false;
            this.fileTree.Size = new System.Drawing.Size(586, 640);
            this.fileTree.TabIndex = 0;
            this.fileTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.fileTree_NodeChecked);
            this.fileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fileTree_NodeSelected);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Controls.Add(this.nextButton);
            this.panel1.Controls.Add(this.closeButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 805);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(628, 48);
            this.panel1.TabIndex = 1;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(21, 13);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(256, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(297, 3);
            this.nextButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(152, 33);
            this.nextButton.TabIndex = 3;
            this.nextButton.Text = "Copy Files";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(455, 3);
            this.closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(152, 33);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // expandAll
            // 
            this.expandAll.Location = new System.Drawing.Point(21, 12);
            this.expandAll.Name = "expandAll";
            this.expandAll.Size = new System.Drawing.Size(86, 23);
            this.expandAll.TabIndex = 5;
            this.expandAll.Text = "Expand All";
            this.expandAll.UseVisualStyleBackColor = true;
            this.expandAll.Click += new System.EventHandler(this.expandAll_Click);
            // 
            // collapseAll
            // 
            this.collapseAll.Location = new System.Drawing.Point(113, 12);
            this.collapseAll.Name = "collapseAll";
            this.collapseAll.Size = new System.Drawing.Size(86, 23);
            this.collapseAll.TabIndex = 6;
            this.collapseAll.Text = "Collapse All";
            this.collapseAll.UseVisualStyleBackColor = true;
            this.collapseAll.Click += new System.EventHandler(this.collapseAll_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 702);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Output Directory";
            // 
            // outputDirectory
            // 
            this.outputDirectory.Location = new System.Drawing.Point(21, 720);
            this.outputDirectory.Name = "outputDirectory";
            this.outputDirectory.Size = new System.Drawing.Size(542, 23);
            this.outputDirectory.TabIndex = 1;
            this.outputDirectory.Text = "D:\\Toolchains\\sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf-2";
            // 
            // selectOutputDir
            // 
            this.selectOutputDir.Location = new System.Drawing.Point(569, 719);
            this.selectOutputDir.Name = "selectOutputDir";
            this.selectOutputDir.Size = new System.Drawing.Size(38, 24);
            this.selectOutputDir.TabIndex = 2;
            this.selectOutputDir.Text = "...";
            this.selectOutputDir.UseVisualStyleBackColor = true;
            this.selectOutputDir.Click += new System.EventHandler(this.selectOutputDir_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 749);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Prefix";
            // 
            // prefixText
            // 
            this.prefixText.Location = new System.Drawing.Point(21, 767);
            this.prefixText.Name = "prefixText";
            this.prefixText.Size = new System.Drawing.Size(586, 23);
            this.prefixText.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(628, 853);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.prefixText);
            this.Controls.Add(this.collapseAll);
            this.Controls.Add(this.selectOutputDir);
            this.Controls.Add(this.outputDirectory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.expandAll);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.fileTree);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "CrossCapture";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView fileTree;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button expandAll;
        private System.Windows.Forms.Button collapseAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox outputDirectory;
        private System.Windows.Forms.Button selectOutputDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox prefixText;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

