using MotherGooseEditor;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MGEGUI {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        bool FilterMode = false;
        XBX Editor;
        KS Filter;
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                textBox1.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
                Text = "id: " + listBox1.SelectedIndex;
            } catch { }
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                try {
                    listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
                } catch {

                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "All Script Files|*.xbx; *.ks";
            if (fd.ShowDialog() != DialogResult.OK)
                return;

            FilterMode = fd.FileName.ToLower().EndsWith(".ks");
            byte[] Script = File.ReadAllBytes(fd.FileName);
            string[] Strings;

            if (FilterMode) {
                Filter = new KS(Script);
                Strings = Filter.Import();
            } else {
                Editor = new XBX(Script);
                Strings = Editor.Import();
            }

            listBox1.Items.Clear();
            foreach (string str in Strings)
                listBox1.Items.Add(str);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "All Script Files|*.xbx;*.ks";
            if (fd.ShowDialog() != DialogResult.OK)
                return;

            string[] Strings = listBox1.Items.Cast<string>().ToArray();
            byte[] Script = FilterMode ? Filter.Export(Strings) : Editor.Export(Strings);
            File.WriteAllBytes(fd.FileName, Script);

            MessageBox.Show("Script Saved");
        }

    }
}
