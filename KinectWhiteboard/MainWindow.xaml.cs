using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;
using System.Diagnostics;

namespace KinectWhiteboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        /// <summary>
        /// Gets the Kinect runtime object
        /// </summary>
        public Microsoft.Research.Kinect.Nui.Runtime NuiRuntime { get; private set; }
        
        public MainWindow()
        {
            InitializeComponent();

            // Make sure only one MainWindow ever gets created
            Debug.Assert(Instance == null);

            Instance = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set up the Kinects
                NuiRuntime = Microsoft.Research.Kinect.Nui.Runtime.Kinects[0];
                NuiRuntime.Initialize(RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
                NuiRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                NuiRuntime.SkeletonEngine.TransformSmooth = true;
                var parameters = new TransformSmoothParameters
                {
                    Smoothing = 0.3f,
                    Correction = 0.0f,
                    Prediction = 0.0f,
                    JitterRadius = 1.0f,
                    MaxDeviationRadius = 0.5f
                };
                NuiRuntime.SkeletonEngine.SmoothParameters = parameters;
                
                // Add KinectCursor Handler
                AddRectangleHandler();
            }
            catch (Exception)
            {
                // Failed to set up the Kinect. Show the error onscreen (app will switch to using mouse movement)
                NuiRuntime = null;
                PART_ErrorText.Visibility = Visibility.Visible;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (NuiRuntime != null)
                NuiRuntime.Uninitialize();
            Environment.Exit(0);
        }

        // Called when the user presses the 'Quit' button
        private void OnQuit(object sender, RoutedEventArgs args)
        {
            //if (_imageUnsaved)
            //    CurrentPopup = new ConfirmationPopup("Quit without saving?", ActionAwaitingConfirmation.Close, this);
            //else
            Close();
        }

        #region register KinectCursor Handler

        // This variable will be used to tract which rectangle the user selected.
        public int rectangleNumber;

        private void AddRectangleHandler()
        {
            KinectCursor.AddCursorEnterHandler(R1, RectangleOnCursorEnter1);
            KinectCursor.AddCursorLeaveHandler(R1, RectangleOnCursorLeave1);

            KinectCursor.AddCursorEnterHandler(R2, RectangleOnCursorEnter2);
            KinectCursor.AddCursorLeaveHandler(R2, RectangleOnCursorLeave2);

            KinectCursor.AddCursorEnterHandler(R3, RectangleOnCursorEnter3);
            KinectCursor.AddCursorLeaveHandler(R3, RectangleOnCursorLeave3);
            
            KinectCursor.AddCursorEnterHandler(R4, RectangleOnCursorEnter4);
            KinectCursor.AddCursorLeaveHandler(R4, RectangleOnCursorLeave4);

            KinectCursor.AddCursorEnterHandler(R5, RectangleOnCursorEnter5);
            KinectCursor.AddCursorLeaveHandler(R5, RectangleOnCursorLeave5);

            KinectCursor.AddCursorEnterHandler(R6, RectangleOnCursorEnter6);
            KinectCursor.AddCursorLeaveHandler(R6, RectangleOnCursorLeave6);
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter1(object sender, CursorEventArgs args)
        {
            rectangleNumber = 1;

            args.Cursor.BeginHover();
            args.Cursor.HoverFinished += Cursor_HoverFinished;
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave1(object sender, CursorEventArgs args)
        {
            rectangleNumber = 0;

            args.Cursor.EndHover();
            args.Cursor.HoverFinished -= Cursor_HoverFinished;
        }
        
        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter2(object sender, CursorEventArgs args)
        {
            args.Cursor.BeginHover();
            rectangleNumber = 2;
            args.Cursor.HoverFinished += Cursor_HoverFinished;
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave2(object sender, CursorEventArgs args)
        {
            args.Cursor.EndHover();
            rectangleNumber = 0;
            args.Cursor.HoverFinished -= Cursor_HoverFinished;
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter3(object sender, CursorEventArgs args)
        {
            args.Cursor.BeginHover();
            rectangleNumber = 3;
            args.Cursor.HoverFinished += Cursor_HoverFinished;
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave3(object sender, CursorEventArgs args)
        {
            args.Cursor.EndHover();
            rectangleNumber = 0;
            args.Cursor.HoverFinished -= Cursor_HoverFinished;
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter4(object sender, CursorEventArgs args)
        {
            args.Cursor.BeginHover();
            rectangleNumber = 4;
            args.Cursor.HoverFinished += Cursor_HoverFinished;
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave4(object sender, CursorEventArgs args)
        {
            args.Cursor.EndHover();
            rectangleNumber = 0;
            args.Cursor.HoverFinished -= Cursor_HoverFinished;
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter5(object sender, CursorEventArgs args)
        {
            args.Cursor.BeginHover();
            rectangleNumber = 5;
            args.Cursor.HoverFinished += Cursor_HoverFinished;
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave5(object sender, CursorEventArgs args)
        {
            args.Cursor.EndHover();
            rectangleNumber = 0;
            args.Cursor.HoverFinished -= Cursor_HoverFinished;
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter6(object sender, CursorEventArgs args)
        {
            args.Cursor.BeginHover();
            rectangleNumber = 6;
            args.Cursor.HoverFinished += Cursor_HoverFinished;
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave6(object sender, CursorEventArgs args)
        {
            args.Cursor.EndHover();
            rectangleNumber = 0;
            args.Cursor.HoverFinished -= Cursor_HoverFinished;
        }

        // Called when the hover action has finished, and the button should be pressed
        private void Cursor_HoverFinished(object sender, EventArgs e)
        {
            ((KinectCursor)sender).HoverFinished -= Cursor_HoverFinished;
        }

        #endregion
    }
}
