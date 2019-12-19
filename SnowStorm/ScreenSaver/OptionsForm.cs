using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScreenSaver
{
    public partial class OptionsForm : Form
    {
        public OptionsForm(UserControl settingControl)
        {
            InitializeComponent( );

            this.Controls.Add( settingControl );
            settingControl.Dock = DockStyle.Fill;
        }
    }
}
