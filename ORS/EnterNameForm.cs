using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ORS
{
    public partial class EnterNameForm : Form
    {
        public EnterNameForm()
        {
            InitializeComponent();
        }

        private void enterNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (enterNameTextBox.Text.Length > 1)
                enterNameButton.Enabled = true;
            else
                enterNameButton.Enabled = false;
        }

        private void enterNameButton_Click(object sender, EventArgs e)
        {
            enterNameTextBox.Text = enterNameTextBox.Text.Replace("|","");
            ORSForm1.yourName = enterNameTextBox.Text;
            try
            {
                RegistryKey orsNameRegKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
                orsNameRegKey.CreateSubKey("Office Rageface Sender");
                orsNameRegKey.Close();
                orsNameRegKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Office Rageface Sender", true);
                orsNameRegKey.SetValue("Name", enterNameTextBox.Text, RegistryValueKind.String);
                orsNameRegKey.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save your name to the registry. This means you're going to be asked for your name again every time you launch this application.\n\n" + ex.Message, "Y U NO SAVE TO REGISTRY??", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Dispose();
        }

        private void miscLinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://myotherpcisacloud.com");
        }
    }
}
