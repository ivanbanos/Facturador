using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;

namespace ControladorEstacion
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            panel1 = new Panel();
            button2 = new Button();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(5, 29, 56);
            pictureBox1.BackgroundImage = Properties.Resources.Visualizador_Título__1_;
            pictureBox1.ErrorImage = Properties.Resources.Visualizador_Título__1_;
            pictureBox1.InitialImage = Properties.Resources.Visualizador_Título__1_;
            pictureBox1.Location = new Point(16, 8);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(311, 92);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(1, 5, 29, 56);
            panel1.BackgroundImage = Properties.Resources.Visualizador_03__1_;
            panel1.BackgroundImageLayout = ImageLayout.Stretch;
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1217, 749);
            panel1.TabIndex = 0;
            panel1.Paint += panel1_Paint;
            // 
            // button2
            // 
            button2.BackColor = Color.FromArgb(5, 29, 56);
            button2.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            button2.ForeColor = Color.FromArgb(124, 197, 252);
            button2.Location = new Point(760, 106);
            button2.Name = "button2";
            button2.Size = new Size(208, 64);
            button2.TabIndex = 1;
            button2.Text = "Reporte de Ventas";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(5, 29, 56);
            button1.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            button1.ForeColor = Color.FromArgb(124, 197, 252);
            button1.Location = new Point(509, 106);
            button1.Name = "button1";
            button1.Size = new Size(208, 64);
            button1.TabIndex = 0;
            button1.Text = "Reporte de lecturas";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            BackgroundImage = Properties.Resources.Visualizador__Fondo__1_;
            ClientSize = new Size(1216, 749);
            Controls.Add(pictureBox1);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Administrador SIGES";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private Panel panel1;
        private Button button2;
        private Button button1;
    }
}