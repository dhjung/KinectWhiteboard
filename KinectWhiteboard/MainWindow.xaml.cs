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
using System.ServiceModel;
using System.Configuration;
using System.Windows.Threading;
using System.Threading;
using System.Net;

namespace KinectWhiteboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window, IChatCallback
    {
        public static MainWindow Instance { get; private set; }

        /// <summary>
        /// Gets the Kinect runtime object
        /// </summary>
        public Microsoft.Research.Kinect.Nui.Runtime NuiRuntime { get; private set; }

        // Rectangle selected by the remote user
        public Rectangle remoteRec = new Rectangle();

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

        #region Client Events and Methods

        private ChatProxy proxy;
        private string myNick;

        // private PleaseWaitDialog pwDlg;
        private delegate void HandleDelegate(string[] list);
        private delegate void HandleErrorDelegate();

        private void OnConnect(object sender, RoutedEventArgs args)
        {
            InstanceContext site = new InstanceContext(this);
            proxy = new ChatProxy(site);
            
            // Get Local IP Address
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            myNick = localIPs[1].ToString();

            // Use IP Address as a user name
            IAsyncResult iar = proxy.BeginJoin(myNick, new AsyncCallback(OnEndJoin), null);
        }

        private void OnEndJoin(IAsyncResult iar)
        {
            try
            {
                string[] list = proxy.EndJoin(iar);
                HandleEndJoin(list);
            }
            catch (Exception e)
            {
                // Configure the message box to be displayed
                string messageBoxText = e.Message;
                string caption = "Connection Error : OnEndJoin";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;

                // Display message box
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
             
        }

        private void HandleEndJoin(string[] list)
        {
            if (list == null)
            {
                    // pwDlg.ShowError("Error: Existing User Name");
                    // ExitChatSession();
            }
            else
            {
                foreach (string name in list)
                {
                    //lstChatters.Items.Add(name);
                }
                // AppendText("Connected " + DateTime.Now.ToString() + " User: " + myNick + Environment.NewLine);
            }
        }

        private void OnDisconnect(object sender, RoutedEventArgs args)
        {
            InfoLabel.Content = "Disconnect";
            try
            {
                proxy.Leave();
            }
            catch (Exception e){
                // Configure the message box to be displayed
                string messageBoxText = e.Message;
                string caption = "Connection Error : OnDisconnect";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;

                // Display message box
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
            finally
            {
                AbortProxyAndUpdateUI();
            }
        }

        // Abort and Close Proxy and Update UI?
        private void AbortProxyAndUpdateUI()
        {
            if (proxy != null)
            {
                proxy.Abort();
                proxy.Close();
                proxy = null;
            }
            // ShowConnectMenuItem(true);
        }

        // Send current cursor position to server (Server will resend it to client)
        public void SendCursorPosition(int x, int y)
        {
            try
            {
                //InfoLabel.Content = x.ToString() + " " + y.ToString();
                Console.WriteLine("SendCursorPosition: " + x.ToString() + " " + y.ToString());

                CommunicationState cs = proxy.State;
                proxy.Say(x, y);
                //if (!pvt)
                //    proxy.Say(msg);
                //else
                //    proxy.Whisper(to, msg);

                //txtMessage.Text = "";
            }
            catch
            {
                //AbortProxyAndUpdateUI();
                //AppendText("연결이 종료되었습니다. " + DateTime.Now.ToString() + Environment.NewLine);
                //Error("에러: 서버가 죽었습니다.!");
            }
        }

        public void SendMovingImage(int imageNumber, bool isMoving)
        {
            try
            {
                Console.WriteLine("SendMovingImage: " + imageNumber + "," + isMoving);

                CommunicationState cs = proxy.State;
                proxy.Whisper(imageNumber, isMoving);
            }
            catch
            {

            }
        }

        #endregion

        #region Implementation IChatCallback (Message from the server)

        // Cursor location from the server
        public int cursorX;
        public int cursorY;

        // Moving location from the server
        public int movingX;
        public int movingY;

        // public void Receive(string senderName, string message)
        public void Receive(string senderName, int x, int y)
        {
            Console.WriteLine("Receive: " + x.ToString() + " " + y.ToString());
            
            // Set cursor position from the server
            cursorX = x;
            cursorY = y;

            // Set cursor position when the remote user is moving image
            movingX = x;
            movingY = y;

            //InfoLabel.Content = "Receive from: " + senderName + " x:" + x.ToString() + " y:" + y.ToString();
            if (senderName != myNick)
            {
                // AppendText(senderName + ": " + message + Environment.NewLine);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    Canvas.SetLeft(Partner, cursorX);
                    Canvas.SetTop(Partner, cursorY);
                });
            }
        }

        //public void ReceiveWhisper(string senderName, string message)
        public void ReceiveWhisper(string senderName, int imageNumber, bool isMoving)
        {
            // AppendText(senderName + " whisper: " + message + Environment.NewLine);
            Console.WriteLine("ReceiveWhisper: " + senderName + " " + imageNumber + " " + isMoving);

            if (senderName != myNick)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    switch (imageNumber)
                    {
                        case 1:
                            remoteRec = R1;
                            break;
                        case 2:
                            remoteRec = R2;
                            break;
                        case 3:
                            remoteRec = R3;
                            break;
                        case 4:
                            remoteRec = R4;
                            break;
                        case 5:
                            remoteRec = R5;
                            break;
                        case 6:
                            remoteRec = R6;
                            break;
                    }

                    if (isMoving)
                    {
                        remoteRec.Opacity = 0.5;
                        // Set rectangle's position and properties
                        Canvas.SetLeft(remoteRec, movingX - remoteRec.ActualWidth / 2);
                        Canvas.SetTop(remoteRec, movingY - (140 + remoteRec.ActualHeight / 2));
                        Canvas.SetZIndex(remoteRec, 10);

                        ImageLabel.Content = "Your partner is moving Image " + remoteRec.Name;
                    }
                    else
                    {
                        remoteRec.Opacity = 1.0;
                        Canvas.SetZIndex(remoteRec, 0);

                        rectangleNumber = 0;
                        remoteRec = null;
                    }

                });
            }

        }

        public void UserEnter(string name)
        {
            // AppendText("User " + name + " enter at " + DateTime.Now.ToString() + Environment.NewLine);
            // lstChatters.Items.Add(name);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                InfoLabel.Content = "Client " + name + " joined.";
                Console.WriteLine("Client " + name + " joined.");
            });
        }

        public void UserLeave(string name)
        {
            // AppendText("User " + name + " leave at " + DateTime.Now.ToString() + Environment.NewLine);
            // lstChatters.Items.Remove(name);
            // AdjustWhisperButton();
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                InfoLabel.Content = "Client " + name + " left.";
                Console.WriteLine("Client " + name + " left.");
            });
        }
        #endregion

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
            if (remoteRec != R1 || remoteRec == null)
            {
                args.Cursor.BeginHover();
                args.Cursor.HoverFinished += Cursor_HoverFinished;
                rectangleNumber = 1;
            }
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave1(object sender, CursorEventArgs args)
        {
            if (remoteRec != R1 || remoteRec == null)
            {
                args.Cursor.EndHover();
                args.Cursor.HoverFinished -= Cursor_HoverFinished;
                rectangleNumber = 0;
            }
        }
        
        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter2(object sender, CursorEventArgs args)
        {
            if (remoteRec != R2 || remoteRec == null)
            {
                args.Cursor.BeginHover();
                args.Cursor.HoverFinished += Cursor_HoverFinished;
                rectangleNumber = 2;
            }
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave2(object sender, CursorEventArgs args)
        {
            if (remoteRec != R2 || remoteRec == null)
            {
                args.Cursor.EndHover();
                args.Cursor.HoverFinished -= Cursor_HoverFinished;
                rectangleNumber = 0;
            }
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter3(object sender, CursorEventArgs args)
        {
            if (remoteRec != R3 || remoteRec == null)
            {
                args.Cursor.BeginHover();
                args.Cursor.HoverFinished += Cursor_HoverFinished;
                rectangleNumber = 3;
            }
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave3(object sender, CursorEventArgs args)
        {
            if (remoteRec != R3 || remoteRec == null)
            {
                args.Cursor.EndHover();
                args.Cursor.HoverFinished -= Cursor_HoverFinished;
                rectangleNumber = 0;
            }
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter4(object sender, CursorEventArgs args)
        {
            if (remoteRec != R4 || remoteRec == null)
            {
                args.Cursor.BeginHover();
                args.Cursor.HoverFinished += Cursor_HoverFinished;
                rectangleNumber = 4;
            }
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave4(object sender, CursorEventArgs args)
        {
            if (remoteRec != R4 || remoteRec == null)
            {
                args.Cursor.EndHover();
                args.Cursor.HoverFinished -= Cursor_HoverFinished;
                rectangleNumber = 0;
            }
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter5(object sender, CursorEventArgs args)
        {
            if (remoteRec != R5 || remoteRec == null)
            {
                args.Cursor.BeginHover();
                args.Cursor.HoverFinished += Cursor_HoverFinished;
                rectangleNumber = 5;
            }
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave5(object sender, CursorEventArgs args)
        {
            if (remoteRec != R5 || remoteRec == null)
            {
                args.Cursor.EndHover();
                args.Cursor.HoverFinished -= Cursor_HoverFinished;
                rectangleNumber = 0;
            }
        }

        // Called when the cursor enters this polygon's visible area
        private void RectangleOnCursorEnter6(object sender, CursorEventArgs args)
        {
            if (remoteRec != R6 || remoteRec == null)
            {
                args.Cursor.BeginHover();
                args.Cursor.HoverFinished += Cursor_HoverFinished;
                rectangleNumber = 6;
            }
        }

        // Called when the cursor leaves this polygon's visible area
        private void RectangleOnCursorLeave6(object sender, CursorEventArgs args)
        {
            if (remoteRec != R6 || remoteRec == null)
            {
                args.Cursor.EndHover();
                args.Cursor.HoverFinished -= Cursor_HoverFinished;
                rectangleNumber = 0;
            }
        }

        // Called when the hover action has finished, and the button should be pressed
        private void Cursor_HoverFinished(object sender, EventArgs e)
        {
            ((KinectCursor)sender).HoverFinished -= Cursor_HoverFinished;
        }

        #endregion

    }
}
