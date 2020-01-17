using System.IO;
using System.Windows.Forms;

namespace FileCatcher.Core
{
    public class FileTreeNode : TreeNode
    {
        public FileSystemInfo File { get; }

        public bool IsLeafNode { get; }

        public FileTreeNode(FileSystemInfo file, bool isLeafNode = false)
        {
            File = file;
            Text = file.Name;
            IsLeafNode = isLeafNode;

            if (!isLeafNode)
            {
                Text = file.Name + (file.Name.EndsWith(Path.DirectorySeparatorChar) ? "" : Path.DirectorySeparatorChar.ToString());
            }
        }
    }
}
