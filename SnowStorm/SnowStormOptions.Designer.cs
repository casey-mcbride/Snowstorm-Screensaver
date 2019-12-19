namespace SnowStorm
{
    partial class SnowStormOptions
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.opacityValue = new System.Windows.Forms.NumericUpDown( );
            this.opacityLabel = new System.Windows.Forms.Label( );
            this.applyButton = new System.Windows.Forms.Button( );
            this.cancelButton = new System.Windows.Forms.Button( );
            this.trailLengthLabel = new System.Windows.Forms.Label( );
            this.trailLengthValue = new System.Windows.Forms.NumericUpDown( );
            this.maximumFlakesLabel = new System.Windows.Forms.Label( );
            this.maximumFlakesUpDown = new System.Windows.Forms.NumericUpDown( );
            ( ( System.ComponentModel.ISupportInitialize )( this.opacityValue ) ).BeginInit( );
            ( ( System.ComponentModel.ISupportInitialize )( this.trailLengthValue ) ).BeginInit( );
            ( ( System.ComponentModel.ISupportInitialize )( this.maximumFlakesUpDown ) ).BeginInit( );
            this.SuspendLayout( );
            // 
            // opacityValue
            // 
            this.opacityValue.DecimalPlaces = 2;
            this.opacityValue.Increment = new decimal( new int[] {
            5,
            0,
            0,
            131072} );
            this.opacityValue.Location = new System.Drawing.Point( 118, 10 );
            this.opacityValue.Maximum = new decimal( new int[] {
            1,
            0,
            0,
            0} );
            this.opacityValue.Minimum = new decimal( new int[] {
            5,
            0,
            0,
            131072} );
            this.opacityValue.Name = "opacityValue";
            this.opacityValue.Size = new System.Drawing.Size( 60, 20 );
            this.opacityValue.TabIndex = 0;
            this.opacityValue.Value = new decimal( new int[] {
            1,
            0,
            0,
            0} );
            // 
            // opacityLabel
            // 
            this.opacityLabel.AutoSize = true;
            this.opacityLabel.Location = new System.Drawing.Point( 3, 10 );
            this.opacityLabel.Name = "opacityLabel";
            this.opacityLabel.Size = new System.Drawing.Size( 43, 13 );
            this.opacityLabel.TabIndex = 1;
            this.opacityLabel.Text = "Opacity";
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point( 118, 231 );
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size( 60, 23 );
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler( this.applyButton_Click );
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point( 195, 231 );
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size( 75, 23 );
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler( this.cancelButton_Click );
            // 
            // trailLengthLabel
            // 
            this.trailLengthLabel.AutoSize = true;
            this.trailLengthLabel.Location = new System.Drawing.Point( 3, 39 );
            this.trailLengthLabel.Name = "trailLengthLabel";
            this.trailLengthLabel.Size = new System.Drawing.Size( 63, 13 );
            this.trailLengthLabel.TabIndex = 4;
            this.trailLengthLabel.Text = "Trail Length";
            // 
            // trailLengthValue
            // 
            this.trailLengthValue.Location = new System.Drawing.Point( 118, 37 );
            this.trailLengthValue.Name = "trailLengthValue";
            this.trailLengthValue.Size = new System.Drawing.Size( 60, 20 );
            this.trailLengthValue.TabIndex = 5;
            // 
            // maximumFlakesLabel
            // 
            this.maximumFlakesLabel.AutoSize = true;
            this.maximumFlakesLabel.Location = new System.Drawing.Point( 6, 66 );
            this.maximumFlakesLabel.Name = "maximumFlakesLabel";
            this.maximumFlakesLabel.Size = new System.Drawing.Size( 85, 13 );
            this.maximumFlakesLabel.TabIndex = 6;
            this.maximumFlakesLabel.Text = "Maximum Flakes";
            // 
            // maximumFlakesUpDown
            // 
            this.maximumFlakesUpDown.Location = new System.Drawing.Point( 118, 66 );
            this.maximumFlakesUpDown.Maximum = new decimal( new int[] {
            5000,
            0,
            0,
            0} );
            this.maximumFlakesUpDown.Minimum = new decimal( new int[] {
            100,
            0,
            0,
            0} );
            this.maximumFlakesUpDown.Name = "maximumFlakesUpDown";
            this.maximumFlakesUpDown.Size = new System.Drawing.Size( 60, 20 );
            this.maximumFlakesUpDown.TabIndex = 7;
            this.maximumFlakesUpDown.Value = new decimal( new int[] {
            100,
            0,
            0,
            0} );
            // 
            // SnowStormOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.maximumFlakesUpDown );
            this.Controls.Add( this.maximumFlakesLabel );
            this.Controls.Add( this.trailLengthValue );
            this.Controls.Add( this.trailLengthLabel );
            this.Controls.Add( this.cancelButton );
            this.Controls.Add( this.applyButton );
            this.Controls.Add( this.opacityLabel );
            this.Controls.Add( this.opacityValue );
            this.Name = "SnowStormOptions";
            this.Size = new System.Drawing.Size( 273, 262 );
            this.Load += new System.EventHandler( this.SnowStormOptions_Load );
            ( ( System.ComponentModel.ISupportInitialize )( this.opacityValue ) ).EndInit( );
            ( ( System.ComponentModel.ISupportInitialize )( this.trailLengthValue ) ).EndInit( );
            ( ( System.ComponentModel.ISupportInitialize )( this.maximumFlakesUpDown ) ).EndInit( );
            this.ResumeLayout( false );
            this.PerformLayout( );

        }

        #endregion

        private System.Windows.Forms.NumericUpDown opacityValue;
        private System.Windows.Forms.Label opacityLabel;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label trailLengthLabel;
        private System.Windows.Forms.NumericUpDown trailLengthValue;
        private System.Windows.Forms.Label maximumFlakesLabel;
        private System.Windows.Forms.NumericUpDown maximumFlakesUpDown;
    }
}
