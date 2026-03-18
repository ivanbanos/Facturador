using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;

namespace ControladorEstacion
{
    partial class FormReporte
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
            dateTimePicker1 = new DateTimePicker();
            label1 = new Label();
            button1 = new Button();
            label2 = new Label();
            dateTimePicker2 = new DateTimePicker();
            SuspendLayout();
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.CalendarFont = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            dateTimePicker1.Location = new Point(34, 70);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(200, 23);
            dateTimePicker1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.FromArgb(1, 1, 1, 1);
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(34, 27);
            label1.Name = "label1";
            label1.Size = new Size(71, 30);
            label1.TabIndex = 1;
            label1.Text = "Desde";
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(5, 29, 56);
            button1.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            button1.Location = new Point(271, 57);
            button1.Name = "button1";
            button1.Size = new Size(208, 64);
            button1.TabIndex = 2;
            button1.Text = "Generar";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.FromArgb(1, 1, 1, 1);
            label2.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(34, 105);
            label2.Name = "label2";
            label2.Size = new Size(66, 30);
            label2.TabIndex = 4;
            label2.Text = "Hasta";
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.CalendarFont = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            dateTimePicker2.Location = new Point(34, 148);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(200, 23);
            dateTimePicker2.TabIndex = 3;
            // 
            // FormReporte
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Visualizador__Fondo__1_;
            ClientSize = new Size(501, 192);
            Controls.Add(label2);
            Controls.Add(dateTimePicker2);
            Controls.Add(button1);
            Controls.Add(label1);
            Controls.Add(dateTimePicker1);
            ForeColor = Color.FromArgb(124, 197, 252);
            Name = "FormReporte";
            Text = "Generar Reporte";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DateTimePicker dateTimePicker1;
        private Label label1;
        private Button button1;
        private Label label2;
        private DateTimePicker dateTimePicker2;
    }
}