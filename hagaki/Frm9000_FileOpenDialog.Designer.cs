namespace hagaki
{
    partial class Frm9000_FileOpenDialog
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
            this.components = new System.ComponentModel.Container();
            this.Cmb_Filter = new System.Windows.Forms.ComboBox();
            this.LVw_File = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TVw_Folder = new System.Windows.Forms.TreeView();
            this.Btn_OK = new System.Windows.Forms.Button();
            this.Btn_RootSelect = new System.Windows.Forms.Button();
            this.Btn_Cancel = new System.Windows.Forms.Button();
            this.Img_TVw_Folder = new System.Windows.Forms.ImageList(this.components);
            this.Img_LVw_File = new System.Windows.Forms.ImageList(this.components);
            this.ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // Cmb_Filter
            // 
            this.Cmb_Filter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Cmb_Filter.FormattingEnabled = true;
            this.Cmb_Filter.Location = new System.Drawing.Point(354, 386);
            this.Cmb_Filter.Name = "Cmb_Filter";
            this.Cmb_Filter.Size = new System.Drawing.Size(419, 20);
            this.Cmb_Filter.TabIndex = 16;
            // 
            // LVw_File
            // 
            this.LVw_File.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2,
            this.ColumnHeader3});
            this.LVw_File.FullRowSelect = true;
            this.LVw_File.HideSelection = false;
            this.LVw_File.Location = new System.Drawing.Point(354, 10);
            this.LVw_File.Name = "LVw_File";
            this.LVw_File.Size = new System.Drawing.Size(419, 370);
            this.LVw_File.TabIndex = 15;
            this.LVw_File.UseCompatibleStateImageBehavior = false;
            this.LVw_File.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "フォルダ・ファイル名称";
            this.ColumnHeader1.Width = 150;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "種類";
            // 
            // ColumnHeader3
            // 
            this.ColumnHeader3.Text = "フルパス";
            // 
            // TVw_Folder
            // 
            this.TVw_Folder.HideSelection = false;
            this.TVw_Folder.ImageKey = "Icon01.ico";
            this.TVw_Folder.ItemHeight = 16;
            this.TVw_Folder.Location = new System.Drawing.Point(12, 10);
            this.TVw_Folder.Name = "TVw_Folder";
            this.TVw_Folder.SelectedImageKey = "Icon02.ico";
            this.TVw_Folder.Size = new System.Drawing.Size(336, 396);
            this.TVw_Folder.TabIndex = 14;
            // 
            // Btn_OK
            // 
            this.Btn_OK.Location = new System.Drawing.Point(585, 412);
            this.Btn_OK.Name = "Btn_OK";
            this.Btn_OK.Size = new System.Drawing.Size(85, 32);
            this.Btn_OK.TabIndex = 12;
            this.Btn_OK.Text = "OK";
            this.Btn_OK.UseVisualStyleBackColor = true;
            // 
            // Btn_RootSelect
            // 
            this.Btn_RootSelect.Location = new System.Drawing.Point(12, 412);
            this.Btn_RootSelect.Name = "Btn_RootSelect";
            this.Btn_RootSelect.Size = new System.Drawing.Size(85, 32);
            this.Btn_RootSelect.TabIndex = 11;
            this.Btn_RootSelect.Text = "ルート選択";
            this.Btn_RootSelect.UseVisualStyleBackColor = true;
            // 
            // Btn_Cancel
            // 
            this.Btn_Cancel.Location = new System.Drawing.Point(688, 412);
            this.Btn_Cancel.Name = "Btn_Cancel";
            this.Btn_Cancel.Size = new System.Drawing.Size(85, 32);
            this.Btn_Cancel.TabIndex = 13;
            this.Btn_Cancel.Text = "キャンセル";
            this.Btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // Img_TVw_Folder
            // 
            this.Img_TVw_Folder.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.Img_TVw_Folder.ImageSize = new System.Drawing.Size(16, 16);
            this.Img_TVw_Folder.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // Img_LVw_File
            // 
            this.Img_LVw_File.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.Img_LVw_File.ImageSize = new System.Drawing.Size(16, 16);
            this.Img_LVw_File.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.Size = new System.Drawing.Size(181, 26);
            // 
            // Frm9000_FileOpenDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 455);
            this.Controls.Add(this.Cmb_Filter);
            this.Controls.Add(this.LVw_File);
            this.Controls.Add(this.TVw_Folder);
            this.Controls.Add(this.Btn_OK);
            this.Controls.Add(this.Btn_RootSelect);
            this.Controls.Add(this.Btn_Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frm9000_FileOpenDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ファイル選択";
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ComboBox Cmb_Filter;
        internal System.Windows.Forms.ListView LVw_File;
        internal System.Windows.Forms.ColumnHeader ColumnHeader1;
        internal System.Windows.Forms.ColumnHeader ColumnHeader2;
        internal System.Windows.Forms.ColumnHeader ColumnHeader3;
        internal System.Windows.Forms.TreeView TVw_Folder;
        internal System.Windows.Forms.Button Btn_OK;
        internal System.Windows.Forms.Button Btn_RootSelect;
        internal System.Windows.Forms.Button Btn_Cancel;
        internal System.Windows.Forms.ImageList Img_TVw_Folder;
        internal System.Windows.Forms.ImageList Img_LVw_File;
        internal System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;
    }
}