using System.ComponentModel;
using System.Windows.Forms;

namespace GSF.Security.CertificateGenerator
{
    partial class FormCertificateGenerator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.buttonCreateCertificate = new System.Windows.Forms.Button();
            this.textBoxCommonName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dateTimePickerStartDate = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerEndDate = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.CmbKeyStrengths = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxSignatureBits = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCreateCertificate
            // 
            this.buttonCreateCertificate.Location = new System.Drawing.Point(179, 170);
            this.buttonCreateCertificate.Name = "buttonCreateCertificate";
            this.buttonCreateCertificate.Size = new System.Drawing.Size(118, 23);
            this.buttonCreateCertificate.TabIndex = 0;
            this.buttonCreateCertificate.Text = "Create Certificate";
            this.buttonCreateCertificate.UseVisualStyleBackColor = true;
            this.buttonCreateCertificate.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBoxCommonName
            // 
            this.textBoxCommonName.Location = new System.Drawing.Point(97, 12);
            this.textBoxCommonName.Name = "textBoxCommonName";
            this.textBoxCommonName.Size = new System.Drawing.Size(200, 20);
            this.textBoxCommonName.TabIndex = 6;
            this.toolTip.SetToolTip(this.textBoxCommonName, "the common name (CN) of the cert. Do not include CN=.");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Common Name";
            // 
            // dateTimePickerStartDate
            // 
            this.dateTimePickerStartDate.Location = new System.Drawing.Point(97, 38);
            this.dateTimePickerStartDate.Name = "dateTimePickerStartDate";
            this.dateTimePickerStartDate.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerStartDate.TabIndex = 7;
            this.toolTip.SetToolTip(this.dateTimePickerStartDate, "Certificate Start");
            // 
            // dateTimePickerEndDate
            // 
            this.dateTimePickerEndDate.Location = new System.Drawing.Point(97, 64);
            this.dateTimePickerEndDate.Name = "dateTimePickerEndDate";
            this.dateTimePickerEndDate.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerEndDate.TabIndex = 8;
            this.toolTip.SetToolTip(this.dateTimePickerEndDate, "Certificate End");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Start Date";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "End Date";
            // 
            // CmbKeyStrengths
            // 
            this.CmbKeyStrengths.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbKeyStrengths.FormattingEnabled = true;
            this.CmbKeyStrengths.Location = new System.Drawing.Point(97, 117);
            this.CmbKeyStrengths.Name = "CmbKeyStrengths";
            this.CmbKeyStrengths.Size = new System.Drawing.Size(200, 21);
            this.CmbKeyStrengths.TabIndex = 20;
            this.toolTip.SetToolTip(this.CmbKeyStrengths, "Recommended: 2048. \r\n1024 only if in a trusted environment where encryption is op" +
        "tional and high speed key exchange is desired. \r\n4096 will take minutes to gener" +
        "ate.");
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 93);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Signature Bits";
            // 
            // comboBoxSignatureBits
            // 
            this.comboBoxSignatureBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSignatureBits.FormattingEnabled = true;
            this.comboBoxSignatureBits.Location = new System.Drawing.Point(97, 90);
            this.comboBoxSignatureBits.Name = "comboBoxSignatureBits";
            this.comboBoxSignatureBits.Size = new System.Drawing.Size(200, 21);
            this.comboBoxSignatureBits.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 120);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Key Strength";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(113, 170);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(60, 23);
            this.buttonCancel.TabIndex = 19;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(97, 144);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(200, 20);
            this.txtPassword.TabIndex = 21;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "PFX Password";
            // 
            // FormCertificateGenerator
            // 
            this.AcceptButton = this.buttonCreateCertificate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(312, 206);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.CmbKeyStrengths);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.comboBoxSignatureBits);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dateTimePickerEndDate);
            this.Controls.Add(this.dateTimePickerStartDate);
            this.Controls.Add(this.textBoxCommonName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonCreateCertificate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormCertificateGenerator";
            this.Text = "Certificate Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button buttonCreateCertificate;
        private TextBox textBoxCommonName;
        private Label label3;
        private DateTimePicker dateTimePickerStartDate;
        private DateTimePicker dateTimePickerEndDate;
        private Label label4;
        private Label label5;
        private ToolTip toolTip;
        private Label label7;
        private ComboBox comboBoxSignatureBits;
        private Label label8;
        private Button buttonCancel;
        private ComboBox CmbKeyStrengths;
        private TextBox txtPassword;
        private Label label1;
    }
}

