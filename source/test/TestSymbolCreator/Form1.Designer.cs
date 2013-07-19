namespace TestSymbolCreator
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
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBoxExport = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.cbExportToFile = new System.Windows.Forms.CheckBox();
            this.cbSymbolId = new System.Windows.Forms.ComboBox();
            this.labelName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxExport)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(64, 48);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Show Symbol";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBoxExport
            // 
            this.pictureBoxExport.Location = new System.Drawing.Point(55, 89);
            this.pictureBoxExport.Name = "pictureBoxExport";
            this.pictureBoxExport.Size = new System.Drawing.Size(173, 161);
            this.pictureBoxExport.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxExport.TabIndex = 1;
            this.pictureBoxExport.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "SIDC:";
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(42, 299);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(199, 23);
            this.labelStatus.TabIndex = 5;
            this.labelStatus.Text = "Status";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbExportToFile
            // 
            this.cbExportToFile.AutoSize = true;
            this.cbExportToFile.Location = new System.Drawing.Point(185, 52);
            this.cbExportToFile.Name = "cbExportToFile";
            this.cbExportToFile.Size = new System.Drawing.Size(87, 17);
            this.cbExportToFile.TabIndex = 6;
            this.cbExportToFile.Text = "Export to File";
            this.cbExportToFile.UseVisualStyleBackColor = true;
            // 
            // cbSymbolId
            // 
            this.cbSymbolId.FormattingEnabled = true;
            this.cbSymbolId.Items.AddRange(new object[] {
            "SFGPUCI---AAUSG",
            "SFGAUCI---AAUSG",
            "SHGAUCI---DAUSG",
            "SGGPUCI---MO---",
            "SPGPUCI--------",
            "SUGPUCI--------",
            "SAGPUCI--------",
            "SNGPUCI--------",
            "SSGPUCI--------",
            "SGGPUCI--------",
            "SWGPUCI--------",
            "SMGPUCI--------",
            "SDGPUCI--------",
            "SLGPUCI--------",
            "SJGPUCI--------",
            "SKGPUCI--------",
            "SFGAEVAL-------",
            "SFSAXR---------",
            "SFUASN---------",
            "SFFAGP---------",
            "WAS-PLT---P----",
            "EHOPDGC--------"});
            this.cbSymbolId.Location = new System.Drawing.Point(64, 12);
            this.cbSymbolId.Name = "cbSymbolId";
            this.cbSymbolId.Size = new System.Drawing.Size(121, 21);
            this.cbSymbolId.TabIndex = 7;
            this.cbSymbolId.Text = "SFGPUCI---AAUSG";
            this.cbSymbolId.SelectedIndexChanged += new System.EventHandler(this.cbSymbolId_SelectedIndexChanged);
            // 
            // labelName
            // 
            this.labelName.Location = new System.Drawing.Point(12, 253);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(260, 46);
            this.labelName.TabIndex = 8;
            this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 321);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.cbSymbolId);
            this.Controls.Add(this.cbExportToFile);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBoxExport);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Military Style Symbol Checker";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxExport)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBoxExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.CheckBox cbExportToFile;
        private System.Windows.Forms.ComboBox cbSymbolId;
        private System.Windows.Forms.Label labelName;
    }
}

