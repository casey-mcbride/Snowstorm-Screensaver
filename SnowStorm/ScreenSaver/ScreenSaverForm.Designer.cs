﻿namespace ScreenSaver
{
    partial class ScreenSaverForm
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
            if( disposing && ( components != null ) )
            {
                components.Dispose( );
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container( );
            this.updateTimer = new System.Windows.Forms.Timer( this.components );
            this.SuspendLayout( );
            // 
            // updateTimer
            // 
            this.updateTimer.Tick += new System.EventHandler( this.updateTimer_Tick );
            // 
            // ScreenSaverForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 284, 262 );
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScreenSaverForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ScreenSaverForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Deactivate += new System.EventHandler( this.ScreenSaverForm_Deactivate );
            this.Load += new System.EventHandler( this.ScreenSaverForm_Load );
            this.Scroll += new System.Windows.Forms.ScrollEventHandler( this.ScreenSaverForm_Scroll );
            this.KeyDown += new System.Windows.Forms.KeyEventHandler( this.ScreenSaverForm_KeyDown );
            this.MouseClick += new System.Windows.Forms.MouseEventHandler( this.ScreenSaverForm_MouseClick );
            this.MouseMove += new System.Windows.Forms.MouseEventHandler( this.ScreenSaverForm_MouseMove );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Timer updateTimer;
    }
}