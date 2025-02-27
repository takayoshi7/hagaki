namespace hagaki
{
    partial class Frm0200_OUT_NG_DATA
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
            this.OutputButton = new System.Windows.Forms.Button();
            this.CheckNumCaseButton = new System.Windows.Forms.Button();
            this.NgCountLabel = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.OutNgPathLabel = new System.Windows.Forms.Label();
            this.OutputDirButton = new System.Windows.Forms.Button();
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
            this.BackButton.Location = new System.Drawing.Point(382, 253);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(98, 45);
            this.BackButton.TabIndex = 25;
            this.BackButton.Text = "戻る";
            this.BackButton.UseVisualStyleBackColor = false;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // OutputButton
            // 
            this.OutputButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.OutputButton.Enabled = false;
            this.OutputButton.FlatAppearance.BorderSize = 0;
            this.OutputButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.OutputButton.Location = new System.Drawing.Point(185, 253);
            this.OutputButton.Name = "OutputButton";
            this.OutputButton.Size = new System.Drawing.Size(98, 45);
            this.OutputButton.TabIndex = 24;
            this.OutputButton.Text = "出力";
            this.OutputButton.UseVisualStyleBackColor = false;
            this.OutputButton.Click += new System.EventHandler(this.OutputButton_Click);
            // 
            // CheckNumCaseButton
            // 
            this.CheckNumCaseButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.CheckNumCaseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CheckNumCaseButton.FlatAppearance.BorderSize = 0;
            this.CheckNumCaseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckNumCaseButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.CheckNumCaseButton.Location = new System.Drawing.Point(61, 253);
            this.CheckNumCaseButton.Name = "CheckNumCaseButton";
            this.CheckNumCaseButton.Size = new System.Drawing.Size(100, 45);
            this.CheckNumCaseButton.TabIndex = 23;
            this.CheckNumCaseButton.Text = "件数確認";
            this.CheckNumCaseButton.UseVisualStyleBackColor = false;
            this.CheckNumCaseButton.Click += new System.EventHandler(this.CheckNumCaseButton_Click);
            // 
            // NgCountLabel
            // 
            this.NgCountLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.NgCountLabel.Location = new System.Drawing.Point(345, 119);
            this.NgCountLabel.Name = "NgCountLabel";
            this.NgCountLabel.Size = new System.Drawing.Size(53, 15);
            this.NgCountLabel.TabIndex = 22;
            this.NgCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Label1.Location = new System.Drawing.Point(158, 119);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(263, 15);
            this.Label1.TabIndex = 21;
            this.Label1.Text = "出力対象件数　　　　　　　　　件";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // OutNgPathLabel
            // 
            this.OutNgPathLabel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.OutNgPathLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OutNgPathLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.OutNgPathLabel.Location = new System.Drawing.Point(61, 183);
            this.OutNgPathLabel.Name = "OutNgPathLabel";
            this.OutNgPathLabel.Size = new System.Drawing.Size(419, 39);
            this.OutNgPathLabel.TabIndex = 20;
            // 
            // OutputDirButton
            // 
            this.OutputDirButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.OutputDirButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.OutputDirButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputDirButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.OutputDirButton.Location = new System.Drawing.Point(61, 161);
            this.OutputDirButton.Name = "OutputDirButton";
            this.OutputDirButton.Size = new System.Drawing.Size(75, 23);
            this.OutputDirButton.TabIndex = 19;
            this.OutputDirButton.Text = "出力先";
            this.OutputDirButton.UseVisualStyleBackColor = false;
            this.OutputDirButton.Click += new System.EventHandler(this.OutputDirButton_Click);
            // 
            // Frm0200_OUT_NG_DATA
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 341);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.OutputButton);
            this.Controls.Add(this.CheckNumCaseButton);
            this.Controls.Add(this.NgCountLabel);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.OutNgPathLabel);
            this.Controls.Add(this.OutputDirButton);
            this.Name = "Frm0200_OUT_NG_DATA";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ｱﾝｹｰﾄ集計 & 謝礼発送事務局/NG票出力";
            this.Load += new System.EventHandler(this.Frm0200_OUT_NG_DATA_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button BackButton;
        internal System.Windows.Forms.Button OutputButton;
        internal System.Windows.Forms.Button CheckNumCaseButton;
        internal System.Windows.Forms.Label NgCountLabel;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label OutNgPathLabel;
        internal System.Windows.Forms.Button OutputDirButton;
    }
}