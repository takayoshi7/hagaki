namespace hagaki
{
    partial class Frm0600_OUT_REPORT
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
            this.BackButton = new System.Windows.Forms.Button();
            this.OutHoukokuButton = new System.Windows.Forms.Button();
            this.OutReportPathLabel = new System.Windows.Forms.Label();
            this.OutputDirButton = new System.Windows.Forms.Button();
            this.AfterUkeDate = new System.Windows.Forms.DateTimePicker();
            this.Label1 = new System.Windows.Forms.Label();
            this.BeforeUkeDate = new System.Windows.Forms.DateTimePicker();
            this.UkeDateLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BackButton
            // 
            this.BackButton.BackColor = System.Drawing.Color.Brown;
            this.BackButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BackButton.FlatAppearance.BorderSize = 0;
            this.BackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BackButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BackButton.ForeColor = System.Drawing.Color.White;
            this.BackButton.Location = new System.Drawing.Point(383, 261);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(98, 45);
            this.BackButton.TabIndex = 38;
            this.BackButton.Text = "戻る";
            this.BackButton.UseVisualStyleBackColor = false;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // OutHoukokuButton
            // 
            this.OutHoukokuButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.OutHoukokuButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.OutHoukokuButton.FlatAppearance.BorderSize = 0;
            this.OutHoukokuButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutHoukokuButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.OutHoukokuButton.Location = new System.Drawing.Point(62, 261);
            this.OutHoukokuButton.Name = "OutHoukokuButton";
            this.OutHoukokuButton.Size = new System.Drawing.Size(98, 45);
            this.OutHoukokuButton.TabIndex = 37;
            this.OutHoukokuButton.Text = "出力";
            this.OutHoukokuButton.UseVisualStyleBackColor = false;
            this.OutHoukokuButton.Click += new System.EventHandler(this.OutHoukokuButton_Click);
            // 
            // OutReportPathLabel
            // 
            this.OutReportPathLabel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.OutReportPathLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OutReportPathLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.OutReportPathLabel.Location = new System.Drawing.Point(62, 191);
            this.OutReportPathLabel.Name = "OutReportPathLabel";
            this.OutReportPathLabel.Size = new System.Drawing.Size(419, 39);
            this.OutReportPathLabel.TabIndex = 36;
            // 
            // OutputDirButton
            // 
            this.OutputDirButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.OutputDirButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.OutputDirButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputDirButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.OutputDirButton.Location = new System.Drawing.Point(62, 169);
            this.OutputDirButton.Name = "OutputDirButton";
            this.OutputDirButton.Size = new System.Drawing.Size(75, 23);
            this.OutputDirButton.TabIndex = 35;
            this.OutputDirButton.Text = "出力先";
            this.OutputDirButton.UseVisualStyleBackColor = false;
            this.OutputDirButton.Click += new System.EventHandler(this.OutputDirButton_Click);
            // 
            // AfterUkeDate
            // 
            this.AfterUkeDate.CalendarFont = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.AfterUkeDate.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.AfterUkeDate.Location = new System.Drawing.Point(242, 101);
            this.AfterUkeDate.Name = "AfterUkeDate";
            this.AfterUkeDate.Size = new System.Drawing.Size(149, 22);
            this.AfterUkeDate.TabIndex = 34;
            this.AfterUkeDate.Value = new System.DateTime(2022, 7, 25, 0, 0, 0, 0);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Label1.Location = new System.Drawing.Point(217, 105);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(22, 15);
            this.Label1.TabIndex = 33;
            this.Label1.Text = "～";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BeforeUkeDate
            // 
            this.BeforeUkeDate.CalendarFont = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BeforeUkeDate.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BeforeUkeDate.Location = new System.Drawing.Point(62, 101);
            this.BeforeUkeDate.Name = "BeforeUkeDate";
            this.BeforeUkeDate.Size = new System.Drawing.Size(149, 22);
            this.BeforeUkeDate.TabIndex = 32;
            this.BeforeUkeDate.Value = new System.DateTime(2022, 6, 1, 0, 0, 0, 0);
            // 
            // UkeDateLabel
            // 
            this.UkeDateLabel.AutoSize = true;
            this.UkeDateLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.UkeDateLabel.Location = new System.Drawing.Point(59, 83);
            this.UkeDateLabel.Name = "UkeDateLabel";
            this.UkeDateLabel.Size = new System.Drawing.Size(52, 15);
            this.UkeDateLabel.TabIndex = 31;
            this.UkeDateLabel.Text = "受付日";
            // 
            // Frm0600_OUT_REPORT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 341);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.OutHoukokuButton);
            this.Controls.Add(this.OutReportPathLabel);
            this.Controls.Add(this.OutputDirButton);
            this.Controls.Add(this.AfterUkeDate);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.BeforeUkeDate);
            this.Controls.Add(this.UkeDateLabel);
            this.Name = "Frm0600_OUT_REPORT";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ｱﾝｹｰﾄ集計 & 謝礼発送事務局/報告書データ出力";
            this.Load += new System.EventHandler(this.Frm0600_OUT_REPORT_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button BackButton;
        internal System.Windows.Forms.Button OutHoukokuButton;
        internal System.Windows.Forms.Label OutReportPathLabel;
        internal System.Windows.Forms.Button OutputDirButton;
        internal System.Windows.Forms.DateTimePicker AfterUkeDate;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.DateTimePicker BeforeUkeDate;
        internal System.Windows.Forms.Label UkeDateLabel;
    }
}