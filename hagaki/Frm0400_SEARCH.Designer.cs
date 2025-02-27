namespace hagaki
{
    partial class Frm0400_SEARCH
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.BackButton = new System.Windows.Forms.Button();
            this.SearchButton = new System.Windows.Forms.Button();
            this.DataGridView = new System.Windows.Forms.DataGridView();
            this.KANRI_NO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UKE_DATE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ZIP_CD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ADD_ALL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NAME_SEI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NAME_MEI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TEL_NO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ANK_1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ANK_2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ANK_3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.JYOTAI_KB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NG_OUT_KB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NG_OUT_DATETIME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NG_OUT_LOGINID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HISO_OUT_KB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HISO_OUT_DATETIME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HISO_OUT_LOGINID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.REGIST_DATETIME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.REGIST_LOGINID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UPDATE_DATETIME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UPDATE_LOGINID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SortPanel = new System.Windows.Forms.Panel();
            this.ResetButton = new System.Windows.Forms.Button();
            this.HisoOutKb = new System.Windows.Forms.ComboBox();
            this.NgOutKb = new System.Windows.Forms.ComboBox();
            this.JyotaiKb = new System.Windows.Forms.ComboBox();
            this.NgOutLabel = new System.Windows.Forms.Label();
            this.JyotaiLabel = new System.Windows.Forms.Label();
            this.TelNo = new System.Windows.Forms.TextBox();
            this.TelNoLabel = new System.Windows.Forms.Label();
            this.Mei = new System.Windows.Forms.TextBox();
            this.MeiLabel = new System.Windows.Forms.Label();
            this.Sei = new System.Windows.Forms.TextBox();
            this.SeiLabel = new System.Windows.Forms.Label();
            this.Address = new System.Windows.Forms.TextBox();
            this.AddressLabel = new System.Windows.Forms.Label();
            this.ZipCd = new System.Windows.Forms.TextBox();
            this.ZipCdLabel = new System.Windows.Forms.Label();
            this.HisoOutLabel = new System.Windows.Forms.Label();
            this.AfterUkeDate = new System.Windows.Forms.DateTimePicker();
            this.BeforeUkeDate = new System.Windows.Forms.DateTimePicker();
            this.Label2 = new System.Windows.Forms.Label();
            this.UkedateLabel = new System.Windows.Forms.Label();
            this.AfterKanriNo = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.KanriNoLabel = new System.Windows.Forms.Label();
            this.BeforeKanriNo = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView)).BeginInit();
            this.SortPanel.SuspendLayout();
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
            this.BackButton.Location = new System.Drawing.Point(834, 572);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(111, 47);
            this.BackButton.TabIndex = 7;
            this.BackButton.Text = "戻る";
            this.BackButton.UseVisualStyleBackColor = false;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // SearchButton
            // 
            this.SearchButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.SearchButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SearchButton.FlatAppearance.BorderSize = 0;
            this.SearchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SearchButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.SearchButton.Location = new System.Drawing.Point(680, 572);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(111, 47);
            this.SearchButton.TabIndex = 6;
            this.SearchButton.Text = "検索";
            this.SearchButton.UseVisualStyleBackColor = false;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // DataGridView
            // 
            this.DataGridView.AllowUserToAddRows = false;
            this.DataGridView.AllowUserToDeleteRows = false;
            this.DataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.KANRI_NO,
            this.UKE_DATE,
            this.ZIP_CD,
            this.ADD_ALL,
            this.NAME_SEI,
            this.NAME_MEI,
            this.TEL_NO,
            this.ANK_1,
            this.ANK_2,
            this.ANK_3,
            this.JYOTAI_KB,
            this.NG_OUT_KB,
            this.NG_OUT_DATETIME,
            this.NG_OUT_LOGINID,
            this.HISO_OUT_KB,
            this.HISO_OUT_DATETIME,
            this.HISO_OUT_LOGINID,
            this.REGIST_DATETIME,
            this.REGIST_LOGINID,
            this.UPDATE_DATETIME,
            this.UPDATE_LOGINID});
            this.DataGridView.Location = new System.Drawing.Point(57, 290);
            this.DataGridView.Name = "DataGridView";
            this.DataGridView.ReadOnly = true;
            this.DataGridView.RowTemplate.Height = 21;
            this.DataGridView.Size = new System.Drawing.Size(888, 267);
            this.DataGridView.TabIndex = 5;
            this.DataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_CellDoubleClick);
            // 
            // KANRI_NO
            // 
            this.KANRI_NO.DataPropertyName = "KANRI_NO";
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.KANRI_NO.DefaultCellStyle = dataGridViewCellStyle2;
            this.KANRI_NO.HeaderText = "事務局管理番号";
            this.KANRI_NO.Name = "KANRI_NO";
            this.KANRI_NO.ReadOnly = true;
            this.KANRI_NO.Width = 120;
            // 
            // UKE_DATE
            // 
            this.UKE_DATE.DataPropertyName = "UKE_DATE";
            this.UKE_DATE.HeaderText = "受付日";
            this.UKE_DATE.Name = "UKE_DATE";
            this.UKE_DATE.ReadOnly = true;
            this.UKE_DATE.Width = 80;
            // 
            // ZIP_CD
            // 
            this.ZIP_CD.DataPropertyName = "ZIP_CD";
            this.ZIP_CD.HeaderText = "郵便番号";
            this.ZIP_CD.Name = "ZIP_CD";
            this.ZIP_CD.ReadOnly = true;
            this.ZIP_CD.Width = 80;
            // 
            // ADD_ALL
            // 
            this.ADD_ALL.DataPropertyName = "ADD_ALL";
            this.ADD_ALL.HeaderText = "住所";
            this.ADD_ALL.Name = "ADD_ALL";
            this.ADD_ALL.ReadOnly = true;
            this.ADD_ALL.Width = 150;
            // 
            // NAME_SEI
            // 
            this.NAME_SEI.DataPropertyName = "NAME_SEI";
            this.NAME_SEI.HeaderText = "氏名（姓）";
            this.NAME_SEI.Name = "NAME_SEI";
            this.NAME_SEI.ReadOnly = true;
            this.NAME_SEI.Width = 80;
            // 
            // NAME_MEI
            // 
            this.NAME_MEI.DataPropertyName = "NAME_MEI";
            this.NAME_MEI.HeaderText = "氏名（名）";
            this.NAME_MEI.Name = "NAME_MEI";
            this.NAME_MEI.ReadOnly = true;
            this.NAME_MEI.Width = 80;
            // 
            // TEL_NO
            // 
            this.TEL_NO.DataPropertyName = "TEL_NO";
            this.TEL_NO.HeaderText = "電話番号";
            this.TEL_NO.Name = "TEL_NO";
            this.TEL_NO.ReadOnly = true;
            this.TEL_NO.Width = 90;
            // 
            // ANK_1
            // 
            this.ANK_1.DataPropertyName = "ANK_1";
            this.ANK_1.HeaderText = "アンケート（性別）";
            this.ANK_1.Name = "ANK_1";
            this.ANK_1.ReadOnly = true;
            this.ANK_1.Width = 80;
            // 
            // ANK_2
            // 
            this.ANK_2.DataPropertyName = "ANK_2";
            this.ANK_2.HeaderText = "アンケート（年齢）";
            this.ANK_2.Name = "ANK_2";
            this.ANK_2.ReadOnly = true;
            this.ANK_2.Width = 80;
            // 
            // ANK_3
            // 
            this.ANK_3.DataPropertyName = "ANK_3";
            this.ANK_3.HeaderText = "アンケート（職業）";
            this.ANK_3.Name = "ANK_3";
            this.ANK_3.ReadOnly = true;
            this.ANK_3.Width = 80;
            // 
            // JYOTAI_KB
            // 
            this.JYOTAI_KB.DataPropertyName = "JYOTAI_KB";
            this.JYOTAI_KB.HeaderText = "状態区分";
            this.JYOTAI_KB.Name = "JYOTAI_KB";
            this.JYOTAI_KB.ReadOnly = true;
            this.JYOTAI_KB.Width = 90;
            // 
            // NG_OUT_KB
            // 
            this.NG_OUT_KB.DataPropertyName = "NG_OUT_KB";
            this.NG_OUT_KB.HeaderText = "NG票出力区分";
            this.NG_OUT_KB.Name = "NG_OUT_KB";
            this.NG_OUT_KB.ReadOnly = true;
            this.NG_OUT_KB.Width = 110;
            // 
            // NG_OUT_DATETIME
            // 
            this.NG_OUT_DATETIME.DataPropertyName = "NG_OUT_DATETIME";
            this.NG_OUT_DATETIME.HeaderText = "NG票出力日時";
            this.NG_OUT_DATETIME.Name = "NG_OUT_DATETIME";
            this.NG_OUT_DATETIME.ReadOnly = true;
            this.NG_OUT_DATETIME.Width = 110;
            // 
            // NG_OUT_LOGINID
            // 
            this.NG_OUT_LOGINID.DataPropertyName = "NG_OUT_LOGINID";
            this.NG_OUT_LOGINID.HeaderText = "NG票出力者LoginID";
            this.NG_OUT_LOGINID.Name = "NG_OUT_LOGINID";
            this.NG_OUT_LOGINID.ReadOnly = true;
            // 
            // HISO_OUT_KB
            // 
            this.HISO_OUT_KB.DataPropertyName = "HISO_OUT_KB";
            this.HISO_OUT_KB.HeaderText = "配送データ出力区分";
            this.HISO_OUT_KB.Name = "HISO_OUT_KB";
            this.HISO_OUT_KB.ReadOnly = true;
            this.HISO_OUT_KB.Width = 90;
            // 
            // HISO_OUT_DATETIME
            // 
            this.HISO_OUT_DATETIME.DataPropertyName = "HISO_OUT_DATETIME";
            this.HISO_OUT_DATETIME.HeaderText = "配送データ出力日時";
            this.HISO_OUT_DATETIME.Name = "HISO_OUT_DATETIME";
            this.HISO_OUT_DATETIME.ReadOnly = true;
            this.HISO_OUT_DATETIME.Width = 90;
            // 
            // HISO_OUT_LOGINID
            // 
            this.HISO_OUT_LOGINID.DataPropertyName = "HISO_OUT_LOGINID";
            this.HISO_OUT_LOGINID.HeaderText = "配送データ出力者LoginID";
            this.HISO_OUT_LOGINID.Name = "HISO_OUT_LOGINID";
            this.HISO_OUT_LOGINID.ReadOnly = true;
            // 
            // REGIST_DATETIME
            // 
            this.REGIST_DATETIME.DataPropertyName = "REGIST_DATETIME";
            this.REGIST_DATETIME.HeaderText = "登録日時";
            this.REGIST_DATETIME.Name = "REGIST_DATETIME";
            this.REGIST_DATETIME.ReadOnly = true;
            this.REGIST_DATETIME.Width = 90;
            // 
            // REGIST_LOGINID
            // 
            this.REGIST_LOGINID.DataPropertyName = "REGIST_LOGINID";
            this.REGIST_LOGINID.HeaderText = "登録者LoginID";
            this.REGIST_LOGINID.Name = "REGIST_LOGINID";
            this.REGIST_LOGINID.ReadOnly = true;
            // 
            // UPDATE_DATETIME
            // 
            this.UPDATE_DATETIME.DataPropertyName = "UPDATE_DATETIME";
            this.UPDATE_DATETIME.HeaderText = "更新日時";
            this.UPDATE_DATETIME.Name = "UPDATE_DATETIME";
            this.UPDATE_DATETIME.ReadOnly = true;
            this.UPDATE_DATETIME.Width = 90;
            // 
            // UPDATE_LOGINID
            // 
            this.UPDATE_LOGINID.DataPropertyName = "UPDATE_LOGINID";
            this.UPDATE_LOGINID.HeaderText = "更新者LoginID";
            this.UPDATE_LOGINID.Name = "UPDATE_LOGINID";
            this.UPDATE_LOGINID.ReadOnly = true;
            // 
            // SortPanel
            // 
            this.SortPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SortPanel.Controls.Add(this.ResetButton);
            this.SortPanel.Controls.Add(this.HisoOutKb);
            this.SortPanel.Controls.Add(this.NgOutKb);
            this.SortPanel.Controls.Add(this.JyotaiKb);
            this.SortPanel.Controls.Add(this.NgOutLabel);
            this.SortPanel.Controls.Add(this.JyotaiLabel);
            this.SortPanel.Controls.Add(this.TelNo);
            this.SortPanel.Controls.Add(this.TelNoLabel);
            this.SortPanel.Controls.Add(this.Mei);
            this.SortPanel.Controls.Add(this.MeiLabel);
            this.SortPanel.Controls.Add(this.Sei);
            this.SortPanel.Controls.Add(this.SeiLabel);
            this.SortPanel.Controls.Add(this.Address);
            this.SortPanel.Controls.Add(this.AddressLabel);
            this.SortPanel.Controls.Add(this.ZipCd);
            this.SortPanel.Controls.Add(this.ZipCdLabel);
            this.SortPanel.Controls.Add(this.HisoOutLabel);
            this.SortPanel.Controls.Add(this.AfterUkeDate);
            this.SortPanel.Controls.Add(this.BeforeUkeDate);
            this.SortPanel.Controls.Add(this.Label2);
            this.SortPanel.Controls.Add(this.UkedateLabel);
            this.SortPanel.Controls.Add(this.AfterKanriNo);
            this.SortPanel.Controls.Add(this.Label1);
            this.SortPanel.Controls.Add(this.KanriNoLabel);
            this.SortPanel.Controls.Add(this.BeforeKanriNo);
            this.SortPanel.Location = new System.Drawing.Point(57, 20);
            this.SortPanel.Name = "SortPanel";
            this.SortPanel.Size = new System.Drawing.Size(888, 252);
            this.SortPanel.TabIndex = 4;
            // 
            // ResetButton
            // 
            this.ResetButton.BackColor = System.Drawing.SystemColors.Highlight;
            this.ResetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ResetButton.FlatAppearance.BorderSize = 0;
            this.ResetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetButton.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ResetButton.ForeColor = System.Drawing.SystemColors.Window;
            this.ResetButton.Location = new System.Drawing.Point(751, 195);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(87, 35);
            this.ResetButton.TabIndex = 26;
            this.ResetButton.Text = "リセット";
            this.ResetButton.UseVisualStyleBackColor = false;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // HisoOutKb
            // 
            this.HisoOutKb.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HisoOutKb.FormattingEnabled = true;
            this.HisoOutKb.Location = new System.Drawing.Point(717, 96);
            this.HisoOutKb.Name = "HisoOutKb";
            this.HisoOutKb.Size = new System.Drawing.Size(121, 24);
            this.HisoOutKb.TabIndex = 25;
            // 
            // NgOutKb
            // 
            this.NgOutKb.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.NgOutKb.FormattingEnabled = true;
            this.NgOutKb.Location = new System.Drawing.Point(717, 59);
            this.NgOutKb.Name = "NgOutKb";
            this.NgOutKb.Size = new System.Drawing.Size(121, 24);
            this.NgOutKb.TabIndex = 24;
            // 
            // JyotaiKb
            // 
            this.JyotaiKb.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.JyotaiKb.FormattingEnabled = true;
            this.JyotaiKb.Location = new System.Drawing.Point(717, 18);
            this.JyotaiKb.Name = "JyotaiKb";
            this.JyotaiKb.Size = new System.Drawing.Size(121, 24);
            this.JyotaiKb.TabIndex = 23;
            // 
            // NgOutLabel
            // 
            this.NgOutLabel.AutoSize = true;
            this.NgOutLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.NgOutLabel.Location = new System.Drawing.Point(598, 64);
            this.NgOutLabel.Name = "NgOutLabel";
            this.NgOutLabel.Size = new System.Drawing.Size(113, 16);
            this.NgOutLabel.TabIndex = 22;
            this.NgOutLabel.Text = "NG票出力区分";
            // 
            // JyotaiLabel
            // 
            this.JyotaiLabel.AutoSize = true;
            this.JyotaiLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.JyotaiLabel.Location = new System.Drawing.Point(640, 22);
            this.JyotaiLabel.Name = "JyotaiLabel";
            this.JyotaiLabel.Size = new System.Drawing.Size(71, 16);
            this.JyotaiLabel.TabIndex = 21;
            this.JyotaiLabel.Text = "状態区分";
            // 
            // TelNo
            // 
            this.TelNo.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.TelNo.Location = new System.Drawing.Point(166, 207);
            this.TelNo.MaxLength = 11;
            this.TelNo.Name = "TelNo";
            this.TelNo.Size = new System.Drawing.Size(159, 23);
            this.TelNo.TabIndex = 20;
            // 
            // TelNoLabel
            // 
            this.TelNoLabel.AutoSize = true;
            this.TelNoLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.TelNoLabel.Location = new System.Drawing.Point(89, 210);
            this.TelNoLabel.Name = "TelNoLabel";
            this.TelNoLabel.Size = new System.Drawing.Size(71, 16);
            this.TelNoLabel.TabIndex = 19;
            this.TelNoLabel.Text = "電話番号";
            // 
            // Mei
            // 
            this.Mei.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Mei.Location = new System.Drawing.Point(359, 170);
            this.Mei.Name = "Mei";
            this.Mei.Size = new System.Drawing.Size(98, 23);
            this.Mei.TabIndex = 18;
            // 
            // MeiLabel
            // 
            this.MeiLabel.AutoSize = true;
            this.MeiLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.MeiLabel.Location = new System.Drawing.Point(288, 173);
            this.MeiLabel.Name = "MeiLabel";
            this.MeiLabel.Size = new System.Drawing.Size(67, 16);
            this.MeiLabel.TabIndex = 17;
            this.MeiLabel.Text = "氏名(名)";
            // 
            // Sei
            // 
            this.Sei.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Sei.Location = new System.Drawing.Point(166, 170);
            this.Sei.Name = "Sei";
            this.Sei.Size = new System.Drawing.Size(98, 23);
            this.Sei.TabIndex = 16;
            // 
            // SeiLabel
            // 
            this.SeiLabel.AutoSize = true;
            this.SeiLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.SeiLabel.Location = new System.Drawing.Point(95, 173);
            this.SeiLabel.Name = "SeiLabel";
            this.SeiLabel.Size = new System.Drawing.Size(67, 16);
            this.SeiLabel.TabIndex = 15;
            this.SeiLabel.Text = "氏名(姓)";
            // 
            // Address
            // 
            this.Address.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Address.Location = new System.Drawing.Point(166, 133);
            this.Address.Name = "Address";
            this.Address.Size = new System.Drawing.Size(291, 23);
            this.Address.TabIndex = 14;
            // 
            // AddressLabel
            // 
            this.AddressLabel.AutoSize = true;
            this.AddressLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.AddressLabel.Location = new System.Drawing.Point(121, 136);
            this.AddressLabel.Name = "AddressLabel";
            this.AddressLabel.Size = new System.Drawing.Size(39, 16);
            this.AddressLabel.TabIndex = 13;
            this.AddressLabel.Text = "住所";
            // 
            // ZipCd
            // 
            this.ZipCd.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ZipCd.Location = new System.Drawing.Point(166, 97);
            this.ZipCd.MaxLength = 7;
            this.ZipCd.Name = "ZipCd";
            this.ZipCd.Size = new System.Drawing.Size(110, 23);
            this.ZipCd.TabIndex = 12;
            // 
            // ZipCdLabel
            // 
            this.ZipCdLabel.AutoSize = true;
            this.ZipCdLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ZipCdLabel.Location = new System.Drawing.Point(89, 100);
            this.ZipCdLabel.Name = "ZipCdLabel";
            this.ZipCdLabel.Size = new System.Drawing.Size(71, 16);
            this.ZipCdLabel.TabIndex = 11;
            this.ZipCdLabel.Text = "郵便番号";
            // 
            // HisoOutLabel
            // 
            this.HisoOutLabel.AutoSize = true;
            this.HisoOutLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HisoOutLabel.Location = new System.Drawing.Point(560, 100);
            this.HisoOutLabel.Name = "HisoOutLabel";
            this.HisoOutLabel.Size = new System.Drawing.Size(151, 16);
            this.HisoOutLabel.TabIndex = 10;
            this.HisoOutLabel.Text = "配送データ出力区分";
            // 
            // AfterUkeDate
            // 
            this.AfterUkeDate.CalendarFont = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.AfterUkeDate.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.AfterUkeDate.Location = new System.Drawing.Point(329, 59);
            this.AfterUkeDate.Name = "AfterUkeDate";
            this.AfterUkeDate.Size = new System.Drawing.Size(128, 23);
            this.AfterUkeDate.TabIndex = 9;
            // 
            // BeforeUkeDate
            // 
            this.BeforeUkeDate.CalendarFont = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BeforeUkeDate.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BeforeUkeDate.Location = new System.Drawing.Point(166, 59);
            this.BeforeUkeDate.Name = "BeforeUkeDate";
            this.BeforeUkeDate.Size = new System.Drawing.Size(128, 23);
            this.BeforeUkeDate.TabIndex = 8;
            this.BeforeUkeDate.Value = new System.DateTime(2022, 1, 1, 0, 0, 0, 0);
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Label2.Location = new System.Drawing.Point(302, 64);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(23, 16);
            this.Label2.TabIndex = 6;
            this.Label2.Text = "～";
            // 
            // UkedateLabel
            // 
            this.UkedateLabel.AutoSize = true;
            this.UkedateLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.UkedateLabel.Location = new System.Drawing.Point(105, 62);
            this.UkedateLabel.Name = "UkedateLabel";
            this.UkedateLabel.Size = new System.Drawing.Size(55, 16);
            this.UkedateLabel.TabIndex = 5;
            this.UkedateLabel.Text = "受付日";
            // 
            // AfterKanriNo
            // 
            this.AfterKanriNo.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.AfterKanriNo.Location = new System.Drawing.Point(329, 19);
            this.AfterKanriNo.MaxLength = 5;
            this.AfterKanriNo.Name = "AfterKanriNo";
            this.AfterKanriNo.Size = new System.Drawing.Size(128, 23);
            this.AfterKanriNo.TabIndex = 3;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Label1.Location = new System.Drawing.Point(302, 23);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(23, 16);
            this.Label1.TabIndex = 2;
            this.Label1.Text = "～";
            // 
            // KanriNoLabel
            // 
            this.KanriNoLabel.AutoSize = true;
            this.KanriNoLabel.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.KanriNoLabel.Location = new System.Drawing.Point(41, 23);
            this.KanriNoLabel.Name = "KanriNoLabel";
            this.KanriNoLabel.Size = new System.Drawing.Size(119, 16);
            this.KanriNoLabel.TabIndex = 1;
            this.KanriNoLabel.Text = "事務局管理番号";
            // 
            // BeforeKanriNo
            // 
            this.BeforeKanriNo.Font = new System.Drawing.Font("HG丸ｺﾞｼｯｸM-PRO", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BeforeKanriNo.Location = new System.Drawing.Point(166, 19);
            this.BeforeKanriNo.MaxLength = 5;
            this.BeforeKanriNo.Name = "BeforeKanriNo";
            this.BeforeKanriNo.Size = new System.Drawing.Size(128, 23);
            this.BeforeKanriNo.TabIndex = 0;
            // 
            // Frm0400_SEARCH
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 639);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.SearchButton);
            this.Controls.Add(this.DataGridView);
            this.Controls.Add(this.SortPanel);
            this.Name = "Frm0400_SEARCH";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ｱﾝｹｰﾄ集計 & 謝礼発送事務局/検索";
            this.Load += new System.EventHandler(this.Frm0400_SEARCH_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView)).EndInit();
            this.SortPanel.ResumeLayout(false);
            this.SortPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Button BackButton;
        internal System.Windows.Forms.Button SearchButton;
        internal System.Windows.Forms.DataGridView DataGridView;
        internal System.Windows.Forms.DataGridViewTextBoxColumn KANRI_NO;
        internal System.Windows.Forms.DataGridViewTextBoxColumn UKE_DATE;
        internal System.Windows.Forms.DataGridViewTextBoxColumn ZIP_CD;
        internal System.Windows.Forms.DataGridViewTextBoxColumn ADD_ALL;
        internal System.Windows.Forms.DataGridViewTextBoxColumn NAME_SEI;
        internal System.Windows.Forms.DataGridViewTextBoxColumn NAME_MEI;
        internal System.Windows.Forms.DataGridViewTextBoxColumn TEL_NO;
        internal System.Windows.Forms.DataGridViewTextBoxColumn ANK_1;
        internal System.Windows.Forms.DataGridViewTextBoxColumn ANK_2;
        internal System.Windows.Forms.DataGridViewTextBoxColumn ANK_3;
        internal System.Windows.Forms.DataGridViewTextBoxColumn JYOTAI_KB;
        internal System.Windows.Forms.DataGridViewTextBoxColumn NG_OUT_KB;
        internal System.Windows.Forms.DataGridViewTextBoxColumn NG_OUT_DATETIME;
        internal System.Windows.Forms.DataGridViewTextBoxColumn NG_OUT_LOGINID;
        internal System.Windows.Forms.DataGridViewTextBoxColumn HISO_OUT_KB;
        internal System.Windows.Forms.DataGridViewTextBoxColumn HISO_OUT_DATETIME;
        internal System.Windows.Forms.DataGridViewTextBoxColumn HISO_OUT_LOGINID;
        internal System.Windows.Forms.DataGridViewTextBoxColumn REGIST_DATETIME;
        internal System.Windows.Forms.DataGridViewTextBoxColumn REGIST_LOGINID;
        internal System.Windows.Forms.DataGridViewTextBoxColumn UPDATE_DATETIME;
        internal System.Windows.Forms.DataGridViewTextBoxColumn UPDATE_LOGINID;
        internal System.Windows.Forms.Panel SortPanel;
        internal System.Windows.Forms.Button ResetButton;
        internal System.Windows.Forms.ComboBox HisoOutKb;
        internal System.Windows.Forms.ComboBox NgOutKb;
        internal System.Windows.Forms.ComboBox JyotaiKb;
        internal System.Windows.Forms.Label NgOutLabel;
        internal System.Windows.Forms.Label JyotaiLabel;
        internal System.Windows.Forms.TextBox TelNo;
        internal System.Windows.Forms.Label TelNoLabel;
        internal System.Windows.Forms.TextBox Mei;
        internal System.Windows.Forms.Label MeiLabel;
        internal System.Windows.Forms.TextBox Sei;
        internal System.Windows.Forms.Label SeiLabel;
        internal System.Windows.Forms.TextBox Address;
        internal System.Windows.Forms.Label AddressLabel;
        internal System.Windows.Forms.TextBox ZipCd;
        internal System.Windows.Forms.Label ZipCdLabel;
        internal System.Windows.Forms.Label HisoOutLabel;
        internal System.Windows.Forms.DateTimePicker AfterUkeDate;
        internal System.Windows.Forms.DateTimePicker BeforeUkeDate;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Label UkedateLabel;
        internal System.Windows.Forms.TextBox AfterKanriNo;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label KanriNoLabel;
        internal System.Windows.Forms.TextBox BeforeKanriNo;
    }
}