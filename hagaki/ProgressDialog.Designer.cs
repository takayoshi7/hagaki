namespace hagaki
{
    partial class ProgressDialog
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
            this.CancelButton = new System.Windows.Forms.Button();
            this.MessageLabel = new System.Windows.Forms.Label();
            this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // CancelButton
            // 
            this.CancelButton.BackColor = System.Drawing.Color.DarkRed;
            this.CancelButton.FlatAppearance.BorderSize = 0;
            this.CancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.CancelButton.ForeColor = System.Drawing.Color.White;
            this.CancelButton.Location = new System.Drawing.Point(206, 53);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(97, 23);
            this.CancelButton.TabIndex = 5;
            this.CancelButton.Text = "キャンセル";
            this.CancelButton.UseVisualStyleBackColor = false;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // MessageLabel
            // 
            this.MessageLabel.AutoSize = true;
            this.MessageLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.MessageLabel.Location = new System.Drawing.Point(13, 60);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new System.Drawing.Size(92, 12);
            this.MessageLabel.TabIndex = 4;
            this.MessageLabel.Text = "messageLabel";
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Location = new System.Drawing.Point(12, 12);
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(291, 32);
            this.ProgressBar1.TabIndex = 3;
            // 
            // ProgressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(315, 89);
            this.ControlBox = false;
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.MessageLabel);
            this.Controls.Add(this.ProgressBar1);
            this.MaximizeBox = false;
            this.Name = "ProgressDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "進捗状況";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button CancelButton;
        internal System.Windows.Forms.Label MessageLabel;
        internal System.Windows.Forms.ProgressBar ProgressBar1;
    }
}