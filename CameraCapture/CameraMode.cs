using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace CameraCapture
{
    class CameraMode
    {
        private Capture capture;            /* a Capture object that gets images from the camera */
        DispatcherTimer timer;              /* dispatchertimer object used for ticks */

        public CameraMode(EventHandler myEventHandler)
        {
            /* initialize the camera object */
            capture = new Capture();

            /* create a new timer */
            timer = new DispatcherTimer();
            /* attach the event handler */
            timer.Tick += new EventHandler(myEventHandler);
            /* Set the clock's tick intervals */
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        /* Stop the timer if not stopped already */
        public void stopTimer()
        {
            /* if timer is started */
            if(timer.IsEnabled)
                timer.Stop();
        }

        /* Start the timer if not started already*/
        public void startTimer()
        {
            /* if timer is not started */
            if(!timer.IsEnabled)
                timer.Start();
        }

        /* return a single frame from the camera */
        public Image<Bgr, Byte> queryFrame()
        {
            return capture.QueryFrame();
        }
    }
}
