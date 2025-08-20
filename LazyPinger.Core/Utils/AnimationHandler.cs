using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyPinger.Base.Localization;

namespace LazyPinger.Core.Utils
{
    public class AnimationHandler
    {
        public int WaitTimeToHideGrey { get; set; } = 1000;

        public int WaitTimeToHideLogo { get; set; } = 5000;

        public int WaitTimeToHideSplash { get; set; } = 2000;

        public bool IsGreyVisible { get; set; } = true;

        public bool IsTopLogoVisible { get; set; } = true;

        public bool IsSplashVisible { get; set; } = true;

        public int DevicesRow { get; set; } = 1;

        public int DevicesRowSpan { get; set; } = 1;

        public string SplashRandomText { get; set; } = AppResources.PingFast;
    }
}
