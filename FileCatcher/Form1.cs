using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FileCatcher.Core
{
    public partial class Form1 : Form
    {
        public Form1(IEnumerable<FileInfo> files)
        {
            InitializeComponent();

            var parentList = new Dictionary<string, FileTreeNode>();
            var rootNodes = new List<FileTreeNode>();

            foreach (var file in files)
            {
                var node = new FileTreeNode(file, true);
                var rootNode = AddParentsOfNode(parentList, file.Directory, node);

                if (rootNode != null)
                {
                    rootNodes.Add(rootNode);
                }
            }

            foreach (var node in rootNodes)
            {
                MergeNodes(node);
            }

            fileTree.Nodes.AddRange(rootNodes.ToArray());
        }

        private void MergeNodes(TreeNode node)
        {
            if (node.Nodes.Count == 1 && node.Parent != null)
            {
                var mergeNode = node.FirstNode;
                mergeNode.Text = node.Text + mergeNode.Text;

                node.Parent.Nodes.Add(mergeNode);
                node.Parent.Nodes.Remove(node);
            }

            foreach (TreeNode child in node.Nodes)
            {
                MergeNodes(child);
            }
        }

        private FileTreeNode AddParentsOfNode(IDictionary<string, FileTreeNode> parentList, DirectoryInfo nodeDir, FileTreeNode node)
        {
            if (nodeDir == null)
            {
                return node;
            }

            var parentDidNotExist = false;

            if (!parentList.TryGetValue(nodeDir.FullName, out FileTreeNode parentNode))
            {
                parentNode = new FileTreeNode(nodeDir);
                parentList[nodeDir.FullName] = parentNode;

                parentDidNotExist = true;
            }

            parentNode.Nodes.Add(node);

            if (parentDidNotExist)
            {
                return AddParentsOfNode(parentList, nodeDir.Parent, parentNode);
            }

            return null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void nextButton_Click(object sender, EventArgs evt)
        {
            var files = new List<FileTreeNode>();
            GetCheckedLeafNodes(fileTree.Nodes, files);

            if (!Directory.Exists(prefixText.Text))
            {
                MessageBox.Show(
                    "The prefix path does not exist, please enter or select a proper prefix path.",
                    "Invalid prefix",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(outputDirectory.Text))
            {
                var res = MessageBox.Show(
                    "The output directory does not exist, would you like it to be created?",
                    "Output directory does not exist",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Information);

                if (res == DialogResult.Yes)
                {
                    Directory.CreateDirectory(outputDirectory.Text);
                }
                else
                {
                    return;
                }
            }

            nextButton.Enabled = false;
            closeButton.Enabled = false;
            progressBar1.Visible = true;

            void ProgressDone()
            {
                nextButton.Enabled = true;
                closeButton.Enabled = true;

                progressBar1.Visible = false;
                progressBar1.Value = 0;
            }

            for (var i = 0; i < files.Count; i++)
            {
                progressBar1.Value = (int)((double)i / files.Count * 100.0);

                if (!CopyFile(files[i].File))
                {
                    ProgressDone();

                    return;
                }
            }

            ProgressDone();

            var result = MessageBox.Show(
                "Finished copying files! Click OK to close out or Cancel to stay.",
                "Finished",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private bool CopyFile(FileSystemInfo file)
        {
            var relativePath = Path.GetRelativePath(prefixText.Text, file.FullName);
            var destFile = outputDirectory.Text + Path.DirectorySeparatorChar + relativePath;

            try
            {
                var fileDir = new FileInfo(destFile).Directory;
                if (!fileDir.Exists)
                {
                    fileDir.Create();
                }

                File.Copy(file.FullName, destFile, true);
            }
            catch (Exception e)
            {
                var res = MessageBox.Show(
                    $"Unable to copy {file.Name} to the output directory. Would you like to retry?\n\n{e.Message}",
                    "Error copying file",
                    MessageBoxButtons.AbortRetryIgnore,
                    MessageBoxIcon.Error);

                if (res == DialogResult.Abort)
                {
                    return false;
                }

                if (res == DialogResult.Retry)
                {
                    return CopyFile(file);
                }
            }

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void fileTree_NodeChecked(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode item in e.Node.Nodes)
            {
                item.Checked = e.Node.Checked;
            }
        }

        private void fileTree_NodeSelected(object sender, TreeViewEventArgs e)
        {
            prefixText.Text = (e.Node as FileTreeNode).File.FullName;
        }

        private void selectOutputDir_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    outputDirectory.Text = fbd.SelectedPath;
                }
            }
        }

        private void expandAll_Click(object sender, EventArgs e)
        {
            fileTree.ExpandAll();
        }

        private void collapseAll_Click(object sender, EventArgs e)
        {
            fileTree.CollapseAll();
        }

        private void GetCheckedLeafNodes(TreeNodeCollection nodes, List<FileTreeNode> list)
        {
            foreach (FileTreeNode node in nodes)
            {
                if (node.IsLeafNode && node.Checked)
                {
                    list.Add(node);
                }

                GetCheckedLeafNodes(node.Nodes, list);
            }
        }
    }
}
