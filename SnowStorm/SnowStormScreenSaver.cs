using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnowStorm
{
	class SnowStormScreenSaver : ScreenSaver.ScreenSaverInterface
	{
		protected override ScreenSaver.ScreenDrawer ScreenAnimator
		{
			get { return new SnowStormDrawer(); }
		}

		protected override System.Windows.Forms.UserControl OptionsPanel
		{
			get { return new SnowStormOptions( ); }
		}

		protected override int UpdateSpeed
		{
			get { return 1000 / 600; /*60 FPS*/}
		}

		protected override float Opacity
		{
			get
			{
				return Properties.Settings.Default.Opacity;
			}
		}
	}
}
