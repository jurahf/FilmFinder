﻿namespace FilmForms
{
    partial class frmMain
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.заполнитьФильмToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.выгрузитьСвойстваФильмовВExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.загрузитьСвойстваФильмовИзExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сущностиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.справочникиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.актёрыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.жанрыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.режиссерыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.страныToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.пользовательскиеСвойстваToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.фильмыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.советыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.связьСЭСToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.синхронизироватьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Название = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Год = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Описание = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.openExcelDialog = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.сущностиToolStripMenuItem,
            this.связьСЭСToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.заполнитьФильмToolStripMenuItem,
            this.toolStripSeparator2,
            this.выгрузитьСвойстваФильмовВExcelToolStripMenuItem,
            this.загрузитьСвойстваФильмовИзExcelToolStripMenuItem,
            this.toolStripSeparator1,
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(57, 24);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // заполнитьФильмToolStripMenuItem
            // 
            this.заполнитьФильмToolStripMenuItem.Name = "заполнитьФильмToolStripMenuItem";
            this.заполнитьФильмToolStripMenuItem.Size = new System.Drawing.Size(343, 26);
            this.заполнитьФильмToolStripMenuItem.Text = "Заполнить фильм";
            this.заполнитьФильмToolStripMenuItem.Click += new System.EventHandler(this.заполнитьФильмToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(340, 6);
            // 
            // выгрузитьСвойстваФильмовВExcelToolStripMenuItem
            // 
            this.выгрузитьСвойстваФильмовВExcelToolStripMenuItem.Name = "выгрузитьСвойстваФильмовВExcelToolStripMenuItem";
            this.выгрузитьСвойстваФильмовВExcelToolStripMenuItem.Size = new System.Drawing.Size(343, 26);
            this.выгрузитьСвойстваФильмовВExcelToolStripMenuItem.Text = "Выгрузить свойства фильмов в Excel";
            this.выгрузитьСвойстваФильмовВExcelToolStripMenuItem.Click += new System.EventHandler(this.выгрузитьСвойстваФильмовВExcelToolStripMenuItem_Click);
            // 
            // загрузитьСвойстваФильмовИзExcelToolStripMenuItem
            // 
            this.загрузитьСвойстваФильмовИзExcelToolStripMenuItem.Name = "загрузитьСвойстваФильмовИзExcelToolStripMenuItem";
            this.загрузитьСвойстваФильмовИзExcelToolStripMenuItem.Size = new System.Drawing.Size(343, 26);
            this.загрузитьСвойстваФильмовИзExcelToolStripMenuItem.Text = "Загрузить свойства фильмов из Excel";
            this.загрузитьСвойстваФильмовИзExcelToolStripMenuItem.Click += new System.EventHandler(this.загрузитьСвойстваФильмовИзExcelToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(340, 6);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(343, 26);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // сущностиToolStripMenuItem
            // 
            this.сущностиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.справочникиToolStripMenuItem,
            this.фильмыToolStripMenuItem,
            this.советыToolStripMenuItem});
            this.сущностиToolStripMenuItem.Name = "сущностиToolStripMenuItem";
            this.сущностиToolStripMenuItem.Size = new System.Drawing.Size(89, 24);
            this.сущностиToolStripMenuItem.Text = "Сущности";
            // 
            // справочникиToolStripMenuItem
            // 
            this.справочникиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.актёрыToolStripMenuItem,
            this.жанрыToolStripMenuItem,
            this.режиссерыToolStripMenuItem,
            this.страныToolStripMenuItem,
            this.пользовательскиеСвойстваToolStripMenuItem});
            this.справочникиToolStripMenuItem.Name = "справочникиToolStripMenuItem";
            this.справочникиToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.справочникиToolStripMenuItem.Text = "Справочники";
            // 
            // актёрыToolStripMenuItem
            // 
            this.актёрыToolStripMenuItem.Name = "актёрыToolStripMenuItem";
            this.актёрыToolStripMenuItem.Size = new System.Drawing.Size(279, 26);
            this.актёрыToolStripMenuItem.Text = "Актёры";
            this.актёрыToolStripMenuItem.Click += new System.EventHandler(this.актёрыToolStripMenuItem_Click);
            // 
            // жанрыToolStripMenuItem
            // 
            this.жанрыToolStripMenuItem.Name = "жанрыToolStripMenuItem";
            this.жанрыToolStripMenuItem.Size = new System.Drawing.Size(279, 26);
            this.жанрыToolStripMenuItem.Text = "Жанры";
            this.жанрыToolStripMenuItem.Click += new System.EventHandler(this.жанрыToolStripMenuItem_Click);
            // 
            // режиссерыToolStripMenuItem
            // 
            this.режиссерыToolStripMenuItem.Name = "режиссерыToolStripMenuItem";
            this.режиссерыToolStripMenuItem.Size = new System.Drawing.Size(279, 26);
            this.режиссерыToolStripMenuItem.Text = "Режиссеры";
            this.режиссерыToolStripMenuItem.Click += new System.EventHandler(this.режиссерыToolStripMenuItem_Click);
            // 
            // страныToolStripMenuItem
            // 
            this.страныToolStripMenuItem.Name = "страныToolStripMenuItem";
            this.страныToolStripMenuItem.Size = new System.Drawing.Size(279, 26);
            this.страныToolStripMenuItem.Text = "Страны";
            this.страныToolStripMenuItem.Click += new System.EventHandler(this.страныToolStripMenuItem_Click);
            // 
            // пользовательскиеСвойстваToolStripMenuItem
            // 
            this.пользовательскиеСвойстваToolStripMenuItem.Name = "пользовательскиеСвойстваToolStripMenuItem";
            this.пользовательскиеСвойстваToolStripMenuItem.Size = new System.Drawing.Size(279, 26);
            this.пользовательскиеСвойстваToolStripMenuItem.Text = "Пользовательские свойства";
            this.пользовательскиеСвойстваToolStripMenuItem.Click += new System.EventHandler(this.пользовательскиеСвойстваToolStripMenuItem_Click);
            // 
            // фильмыToolStripMenuItem
            // 
            this.фильмыToolStripMenuItem.Name = "фильмыToolStripMenuItem";
            this.фильмыToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.фильмыToolStripMenuItem.Text = "Фильмы";
            this.фильмыToolStripMenuItem.Click += new System.EventHandler(this.фильмыToolStripMenuItem_Click);
            // 
            // советыToolStripMenuItem
            // 
            this.советыToolStripMenuItem.Name = "советыToolStripMenuItem";
            this.советыToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.советыToolStripMenuItem.Text = "Советы";
            this.советыToolStripMenuItem.Click += new System.EventHandler(this.советыToolStripMenuItem_Click);
            // 
            // связьСЭСToolStripMenuItem
            // 
            this.связьСЭСToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.синхронизироватьToolStripMenuItem});
            this.связьСЭСToolStripMenuItem.Name = "связьСЭСToolStripMenuItem";
            this.связьСЭСToolStripMenuItem.Size = new System.Drawing.Size(94, 24);
            this.связьСЭСToolStripMenuItem.Text = "Связь с ЭС";
            // 
            // синхронизироватьToolStripMenuItem
            // 
            this.синхронизироватьToolStripMenuItem.Name = "синхронизироватьToolStripMenuItem";
            this.синхронизироватьToolStripMenuItem.Size = new System.Drawing.Size(218, 26);
            this.синхронизироватьToolStripMenuItem.Text = "Синхронизировать";
            this.синхронизироватьToolStripMenuItem.Click += new System.EventHandler(this.синхронизироватьToolStripMenuItem_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Название,
            this.Год,
            this.Описание});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 28);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(800, 422);
            this.dataGridView1.TabIndex = 1;
            // 
            // Название
            // 
            this.Название.HeaderText = "Название";
            this.Название.Name = "Название";
            this.Название.ReadOnly = true;
            this.Название.Width = 300;
            // 
            // Год
            // 
            this.Год.HeaderText = "Год";
            this.Год.Name = "Год";
            this.Год.ReadOnly = true;
            // 
            // Описание
            // 
            this.Описание.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Описание.HeaderText = "Описание";
            this.Описание.Name = "Описание";
            this.Описание.ReadOnly = true;
            // 
            // openExcelDialog
            // 
            this.openExcelDialog.Filter = "Файлы Excel|*.xls;*.xlsx";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Text = "База данных фильмов";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сущностиToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Название;
        private System.Windows.Forms.DataGridViewTextBoxColumn Год;
        private System.Windows.Forms.DataGridViewTextBoxColumn Описание;
        private System.Windows.Forms.ToolStripMenuItem справочникиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem актёрыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem жанрыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem режиссерыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem страныToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem пользовательскиеСвойстваToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem фильмыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem советыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem связьСЭСToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem синхронизироватьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem заполнитьФильмToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem выгрузитьСвойстваФильмовВExcelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem загрузитьСвойстваФильмовИзExcelToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.OpenFileDialog openExcelDialog;
    }
}

