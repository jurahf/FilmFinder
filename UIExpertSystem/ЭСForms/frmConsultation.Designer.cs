namespace ЭС
{
    partial class frmConsultation
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnAnswer = new System.Windows.Forms.Button();
            this.lblQText = new System.Windows.Forms.Label();
            this.lblQName = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lblYear = new System.Windows.Forms.Label();
            this.lblCountry = new System.Windows.Forms.Label();
            this.lblProducer = new System.Windows.Forms.Label();
            this.lblActor = new System.Windows.Forms.Label();
            this.lblSlogan = new System.Windows.Forms.Label();
            this.lblGenre = new System.Windows.Forms.Label();
            this.lblRating = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblTags = new System.Windows.Forms.Label();
            this.lblImdb = new System.Windows.Forms.LinkLabel();
            this.lblPoster = new System.Windows.Forms.LinkLabel();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(870, 614);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Controls.Add(this.panel2);
            this.tabPage1.Controls.Add(this.lblQText);
            this.tabPage1.Controls.Add(this.lblQName);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(862, 541);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 153);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(856, 336);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnAnswer);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(3, 489);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(856, 49);
            this.panel2.TabIndex = 3;
            // 
            // btnAnswer
            // 
            this.btnAnswer.Location = new System.Drawing.Point(731, 7);
            this.btnAnswer.Name = "btnAnswer";
            this.btnAnswer.Size = new System.Drawing.Size(107, 37);
            this.btnAnswer.TabIndex = 0;
            this.btnAnswer.Text = "Дальше";
            this.btnAnswer.UseVisualStyleBackColor = true;
            this.btnAnswer.Click += new System.EventHandler(this.btnAnswer_Click);
            // 
            // lblQText
            // 
            this.lblQText.AutoSize = true;
            this.lblQText.Location = new System.Drawing.Point(29, 64);
            this.lblQText.Name = "lblQText";
            this.lblQText.Size = new System.Drawing.Size(46, 51);
            this.lblQText.TabIndex = 1;
            this.lblQText.Text = "QText\r\n\r\ncf\r\n";
            // 
            // lblQName
            // 
            this.lblQName.AutoSize = true;
            this.lblQName.Location = new System.Drawing.Point(29, 18);
            this.lblQName.Name = "lblQName";
            this.lblQName.Size = new System.Drawing.Size(56, 17);
            this.lblQName.TabIndex = 0;
            this.lblQName.Text = "QName";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lblPoster);
            this.tabPage2.Controls.Add(this.lblImdb);
            this.tabPage2.Controls.Add(this.lblTags);
            this.tabPage2.Controls.Add(this.lblYear);
            this.tabPage2.Controls.Add(this.lblCountry);
            this.tabPage2.Controls.Add(this.lblProducer);
            this.tabPage2.Controls.Add(this.lblActor);
            this.tabPage2.Controls.Add(this.lblSlogan);
            this.tabPage2.Controls.Add(this.lblGenre);
            this.tabPage2.Controls.Add(this.lblRating);
            this.tabPage2.Controls.Add(this.lblDescription);
            this.tabPage2.Controls.Add(this.lblName);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(862, 585);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lblYear
            // 
            this.lblYear.AutoSize = true;
            this.lblYear.Location = new System.Drawing.Point(585, 30);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(32, 17);
            this.lblYear.TabIndex = 8;
            this.lblYear.Text = "Год";
            // 
            // lblCountry
            // 
            this.lblCountry.AutoSize = true;
            this.lblCountry.Location = new System.Drawing.Point(33, 263);
            this.lblCountry.Name = "lblCountry";
            this.lblCountry.Size = new System.Drawing.Size(58, 17);
            this.lblCountry.TabIndex = 7;
            this.lblCountry.Text = "Страны";
            // 
            // lblProducer
            // 
            this.lblProducer.AutoSize = true;
            this.lblProducer.Location = new System.Drawing.Point(33, 361);
            this.lblProducer.Name = "lblProducer";
            this.lblProducer.Size = new System.Drawing.Size(82, 17);
            this.lblProducer.TabIndex = 6;
            this.lblProducer.Text = "Режиссеры";
            // 
            // lblActor
            // 
            this.lblActor.AutoSize = true;
            this.lblActor.Location = new System.Drawing.Point(33, 312);
            this.lblActor.Name = "lblActor";
            this.lblActor.Size = new System.Drawing.Size(57, 17);
            this.lblActor.TabIndex = 5;
            this.lblActor.Text = "Актеры";
            // 
            // lblSlogan
            // 
            this.lblSlogan.AutoSize = true;
            this.lblSlogan.Location = new System.Drawing.Point(33, 175);
            this.lblSlogan.Name = "lblSlogan";
            this.lblSlogan.Size = new System.Drawing.Size(54, 17);
            this.lblSlogan.TabIndex = 4;
            this.lblSlogan.Text = "Слоган";
            // 
            // lblGenre
            // 
            this.lblGenre.AutoSize = true;
            this.lblGenre.Location = new System.Drawing.Point(33, 214);
            this.lblGenre.Name = "lblGenre";
            this.lblGenre.Size = new System.Drawing.Size(45, 17);
            this.lblGenre.TabIndex = 3;
            this.lblGenre.Text = "Жанр";
            // 
            // lblRating
            // 
            this.lblRating.AutoSize = true;
            this.lblRating.Location = new System.Drawing.Point(757, 30);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(61, 17);
            this.lblRating.TabIndex = 2;
            this.lblRating.Text = "Рейтинг";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(33, 73);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(74, 102);
            this.lblDescription.TabIndex = 1;
            this.lblDescription.Text = "Описание\r\n-\r\n-\r\n-\r\n-\r\n-";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(33, 30);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(72, 17);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Название";
            // 
            // lblTags
            // 
            this.lblTags.AutoSize = true;
            this.lblTags.Location = new System.Drawing.Point(33, 519);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(38, 17);
            this.lblTags.TabIndex = 11;
            this.lblTags.Text = "Теги";
            // 
            // lblImdb
            // 
            this.lblImdb.AutoSize = true;
            this.lblImdb.Location = new System.Drawing.Point(33, 429);
            this.lblImdb.Name = "lblImdb";
            this.lblImdb.Size = new System.Drawing.Size(41, 17);
            this.lblImdb.TabIndex = 12;
            this.lblImdb.TabStop = true;
            this.lblImdb.Text = "IMDB";
            // 
            // lblPoster
            // 
            this.lblPoster.AutoSize = true;
            this.lblPoster.Location = new System.Drawing.Point(33, 475);
            this.lblPoster.Name = "lblPoster";
            this.lblPoster.Size = new System.Drawing.Size(56, 17);
            this.lblPoster.TabIndex = 13;
            this.lblPoster.TabStop = true;
            this.lblPoster.Text = "Постер";
            // 
            // frmConsultation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(870, 614);
            this.Controls.Add(this.tabControl1);
            this.Name = "frmConsultation";
            this.Text = "Консультация";
            this.Load += new System.EventHandler(this.frmConsultation_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblQText;
        private System.Windows.Forms.Label lblQName;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnAnswer;
        private System.Windows.Forms.Label lblYear;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.Label lblProducer;
        private System.Windows.Forms.Label lblActor;
        private System.Windows.Forms.Label lblSlogan;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.Label lblRating;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.LinkLabel lblPoster;
        private System.Windows.Forms.LinkLabel lblImdb;
    }
}