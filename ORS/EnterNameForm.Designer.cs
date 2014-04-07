namespace ORS
{
    partial class EnterNameForm
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
            this.enterNameTextBox = new System.Windows.Forms.TextBox();
            this.enterNameButton = new System.Windows.Forms.Button();
            this.miscLabel1 = new System.Windows.Forms.Label();
            this.miscLinkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // enterNameTextBox
            // 
            this.enterNameTextBox.Location = new System.Drawing.Point(12, 12);
            this.enterNameTextBox.MaxLength = 64;
            this.enterNameTextBox.Name = "enterNameTextBox";
            this.enterNameTextBox.Size = new System.Drawing.Size(242, 20);
            this.enterNameTextBox.TabIndex = 0;
            this.enterNameTextBox.TextChanged += new System.EventHandler(this.enterNameTextBox_TextChanged);
            // 
            // enterNameButton
            // 
            this.enterNameButton.Enabled = false;
            this.enterNameButton.Location = new System.Drawing.Point(261, 11);
            this.enterNameButton.Name = "enterNameButton";
            this.enterNameButton.Size = new System.Drawing.Size(75, 23);
            this.enterNameButton.TabIndex = 1;
            this.enterNameButton.Text = "OK";
            this.enterNameButton.UseVisualStyleBackColor = true;
            this.enterNameButton.Click += new System.EventHandler(this.enterNameButton_Click);
            // 
            // miscLabel1
            // 
            this.miscLabel1.AutoSize = true;
            this.miscLabel1.Location = new System.Drawing.Point(42, 82);
            this.miscLabel1.Name = "miscLabel1";
            this.miscLabel1.Size = new System.Drawing.Size(253, 13);
            this.miscLabel1.TabIndex = 2;
            this.miscLabel1.Text = "ORS - Office Rageface Sender, by Ryan Ries, 2012";
            // 
            // miscLinkLabel1
            // 
            this.miscLinkLabel1.AutoSize = true;
            this.miscLinkLabel1.Location = new System.Drawing.Point(104, 99);
            this.miscLinkLabel1.Name = "miscLinkLabel1";
            this.miscLinkLabel1.Size = new System.Drawing.Size(118, 13);
            this.miscLinkLabel1.TabIndex = 3;
            this.miscLinkLabel1.TabStop = true;
            this.miscLinkLabel1.Text = "myotherpcisacloud.com";
            this.miscLinkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.miscLinkLabel1_LinkClicked);
            // 
            // EnterNameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 122);
            this.ControlBox = false;
            this.Controls.Add(this.miscLinkLabel1);
            this.Controls.Add(this.miscLabel1);
            this.Controls.Add(this.enterNameButton);
            this.Controls.Add(this.enterNameTextBox);
            this.MaximumSize = new System.Drawing.Size(360, 160);
            this.MinimumSize = new System.Drawing.Size(360, 160);
            this.Name = "EnterNameForm";
            this.Text = "Enter Your Name";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox enterNameTextBox;
        private System.Windows.Forms.Button enterNameButton;
        private System.Windows.Forms.Label miscLabel1;
        private System.Windows.Forms.LinkLabel miscLinkLabel1;
    }
}