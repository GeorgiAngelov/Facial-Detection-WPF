using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace CameraCapture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HaarCascade haarCascade;            /* haarCascade object to import facial detection library */
        private const int IMAGE = 0;                /* constant for IMAGE mode*/
        private const int CAMERA = 1;               /* constant for CAMERA mode*/
        private CameraMode camera_object = null;    /* The CameraMode object used to work with the camera's image extraction */
        private BitmapSource modified_image = null;        /* A BitmapSource image that will be used to save the currently modified image */
        Image<Bgr, Byte> image_to_filter = null;
        Image<Bgr, Byte> filtered_image = null;

        public MainWindow()
        {
            InitializeComponent();
            /* create a new HaarCascade to be used to detect faces using alt_tree algorithm */
            haarCascade = new HaarCascade(@"haarcascade_frontalface_alt_tree.xml");
        }

        /* The threaded function that takes care of the drawing of rectangles. */
        void timer_Tick(object sender, EventArgs e)
        {
            /* Grab a frame from the camera */
            Image<Bgr, Byte> currentFrame = camera_object.queryFrame();

            /* Check to see that there was a frame collected */
            if (currentFrame != null)
            {
                detectFaces(currentFrame, CAMERA);
            }
        }

        private void detectFaces(Image<Bgr,Byte> image, int mode)
        {
            /* Check to see that there was a frame collected */
            if (image != null)
            {
                if (mode == CAMERA)
                    mode_name.Content = "Camera";
                else
                    mode_name.Content = "Image";

                /* convert the frame from the camera to a transformed Image that improves facial detection */
                Image<Gray, Byte> grayFrame = image.Convert<Gray, Byte>();
                /* Detect how many faces are there on the image */
                var detectedFaces = grayFrame.DetectHaarCascade(haarCascade)[0];
                
                /* update the faces count */
                faces_count.Content = detectedFaces.Length;
                /* loop through all faces that were detected and draw a rectangle */
                foreach (var face in detectedFaces)
                {
                    image.Draw(face.rect, new Bgr(0, double.MaxValue, 0), 3);
                }
                image_to_filter = image;
                filtered_image = image;
                modified_image = ToBitmapSource(image);
                /* set the transformed image to the image1 object */
                image1.Source = modified_image;
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /* Function to convert an IImage to a BitmapSource. This is needed because
         * otherwise I have to use a hardcoded ImageBox object that is defined in EmguCV
         * but this will hault my design so I decided to convert the Image object to the proper
         * format to use a normal Image object that is used in WPF
         */
        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); /* obtain the Hbitmap */
                
                /* Transform the IImage to a BitmapSource */
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); /* release the HBitmap */
                return bs; /* return the newly converted BitmapSource */
            }
        }

        /* Function to handle clicking of exit button in File/Exit */
        private void exit_menuItem_Click(object sender, RoutedEventArgs e)
        {
            if (cleanUpBeforeExit())
            {
                Close();
            }
        }

        /* Function to clean up before exit */
        private bool cleanUpBeforeExit()
        {
            return true;
        }

        private void save_current_image_click(object sender, RoutedEventArgs e)
        {
            /* No image to save */
            if (modified_image == null)
            {
                no_image_label.Visibility = Visibility.Visible;
                return;
            }
            /* stop the timer so we can save the image */
            if (camera_object != null)
                camera_object.stopTimer();

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "PNG Image File (*.PNG)|*.PNG|All files (*.*)|*.*";

            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                saveImage(saveFileDialog.FileName);
                mode_name.Content = "The image was saved to your computer.";
            }

            /* restart the timer */
            if (camera_object != null)
                camera_object.startTimer();
        }

        /* Save the image with the specified name */
        public void saveImage(String filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Interlace = PngInterlaceOption.Off;
            // see notes below about BitmapFrame.Create()
            encoder.Frames.Add(BitmapFrame.Create(modified_image));
            encoder.Save(stream);
            stream.Close();
        }

        private void about_menuItem_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "This application detects and draws a square around every face that is in the image uploaded by the user or by using the camera as a continuous stream of images.\n\nYou can save the current image displayed in the pannel by clicking on the Save Image button and then selecting a name and folder.\n\nThe right panel displays the number of faces that are detected by the software.\n\nThe software uses EmguCV(An open source OpenCV wrapper library) to perform its functionality.\n\nCopyright 2012, Georgi Angelov, Inc.";
            string caption = "About";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            // Display message box
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            // No need to process message box results since OK is the only option.
        }

        private void use_image_button_click(object sender, RoutedEventArgs e)
        {
            no_image_label.Visibility = Visibility.Hidden;
            if(camera_object != null)
                camera_object.stopTimer(); /* stop the camera timer */

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Multiselect = false;
            // http://msdn.microsoft.com/en-us/library/system.windows.forms.filedialog.filter.aspx
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            // http://msdn.microsoft.com/en-us/library/system.windows.forms.filedialog.filterindex.aspx
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mode_name.Content = "Opening image...";

                try
                {
                    openImage(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    mode_name.Content = "An exception occurred opening the image.";
                }
            }
        }

        /* Open an image with the given filename */
        private void openImage(String filename)
        {
            try
            {
                Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(filename);
                /* set the image's source to the new bitmapimage that we created */
                detectFaces(img1, IMAGE);
            }
            catch (Exception ex)
            {
                // re-throw exception
                throw ex;
            }
        }

        /* The function starts the timer that runs the camera and facial detection */
        private void use_camera_button_click(object sender, RoutedEventArgs e)
        {
            no_image_label.Visibility = Visibility.Hidden;
            if (camera_object == null)
            {
                /* initialize the cameramode object and pass it the event handler */
                camera_object = new CameraMode(timer_Tick);
            }
            
            camera_object.startTimer();
        }

        private void filter1_check_Click(object sender, RoutedEventArgs e)
        {
            if (image_to_filter != null)
            {
                /* if the filter is to be applied */
                if (filter1_checkbox.IsChecked == true)
                {
                    filtered_image = filtered_image.SmoothMedian(15);
                    image1.Source = ToBitmapSource(filtered_image);
                }
                else
                {
                    if (filter2_checkbox.IsChecked == true)
                        image1.Source = ToBitmapSource(image_to_filter.SmoothGaussian(3, 3, 34.3, 45.3));
                    else
                    {
                        image1.Source = ToBitmapSource(image_to_filter);
                        filtered_image = image_to_filter;
                    }
                }
            }
        }

        private void filter2_check_Click(object sender, RoutedEventArgs e)
        {
            if(image_to_filter != null)
            {
                Console.WriteLine(filter2_checkbox.IsChecked);
                /* if the filter is to be applied */
                if (filter2_checkbox.IsChecked == true)
                {
                    filtered_image = filtered_image.SmoothGaussian(3, 3, 34.3, 45.3);
                    image1.Source = ToBitmapSource(filtered_image);
                }
                else
                {
                    if (filter1_checkbox.IsChecked == true)
                        image1.Source = ToBitmapSource(image_to_filter.SmoothMedian(15));
                    else{
                        image1.Source = ToBitmapSource(image_to_filter);
                        filtered_image = image_to_filter;
                    }
                }
            }
        }
    }
}
