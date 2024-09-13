using Microsoft.Win32;
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
using System.Windows.Threading;

namespace Medialejatszo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DispatcherTimer mouseTimer; // timer to the mouse, which is movig
		private bool controlsVisible = true; // flag for the controlpanel visiblity
        private DispatcherTimer progressTimer; // timer for the progress bar
        private bool progressIsDragging = false;  // flag for the progress bar
		private bool isPlaying = false; //Track playback state
        
        public MainWindow()
		{
			InitializeComponent();
			volumeSlider.Value = mediaPlayer.Volume;
			
			mouseTimer= new DispatcherTimer();//initialize and configure the mouse inactivitiy timer
			mouseTimer.Interval = TimeSpan.FromSeconds(2); // time to wait
            mouseTimer.Tick += MouseTimer_Tick; ;

			progressTimer = new DispatcherTimer(); //timer to update the progress
			progressTimer.Interval = TimeSpan.FromMilliseconds(500); // update every 0.5 seconds
            progressTimer.Tick += ProgressTimer_Tick; ;
		}

		private void ProgressTimer_Tick(object sender, EventArgs e)
		{
			if (!progressIsDragging) // only update when it's not been moved
			{
				progressSlider.Value = mediaPlayer.Position.TotalSeconds;
			}
		}
        private void MouseTimer_Tick(object sender, EventArgs e)
        {
			if (controlsVisible)//hide the controls if they are currently visible
			{
				controlsPanel.Visibility = Visibility.Hidden;
				controlsVisible = false;
			}
			mouseTimer.Stop();// stop the timer, we don't need it
        }
		private void Window_MouseMove(object sender, MouseEventArgs e)// event handler to show the controls when the mouse movin
		{
			if (!controlsVisible)
			{
				controlsPanel.Visibility = Visibility.Visible; //show controls if they are hidden
				controlsVisible = true;
			}
			mouseTimer.Stop();//restart the timer every time the mouse moves
			mouseTimer.Start();
		}
		private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
		{
			double videoWidth = mediaPlayer.NaturalVideoWidth;
			double videoHeight = mediaPlayer.NaturalVideoHeight;
            double maxHeight = SystemParameters.WorkArea.Height; // Get the available screen height (excluding taskbar)
            double maxWidth = SystemParameters.WorkArea.Width;   // Get the available screen width

            if (videoWidth > 0 && videoHeight > 0)//video
			{
                progressSlider.Width = 300;
                volumeSlider.Width = 100;
                openButton.Width = 50;
                playPauseButton.Width = 50;
                this.Height = videoHeight > 450 ? (videoHeight > maxHeight ? maxHeight : videoHeight) : 450;
                this.Width = videoWidth > 800 ? (videoWidth>maxWidth? maxWidth:videoWidth) : 800;
			}
			else//music
			{
				this.Width= 300;
				this.Height = 400;
				progressSlider.Width = 140;
				volumeSlider.Width = 50;
				openButton.Width = 35;
				playPauseButton.Width = 35;
			}
			// set the progress slider's maximum to the media duration(in seconds)
			progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
		    
        }
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            playpause();
        }
        private void playpause()
        {
            if (mediaPlayer.Source == null) return; // No file loaded
            if (isPlaying)
            {
                mediaPlayer.Pause();
                playPauseButton.Content = "Play";
            }
            else
            {
                mediaPlayer.Play();
                progressTimer.Start();
                playPauseButton.Content = "Pause";
            }
            isPlaying = !isPlaying;
        }
		private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			// handle progress slider value change(seek functionality)
			if (mediaPlayer.NaturalDuration.HasTimeSpan && progressIsDragging)// only seek when draging
			{
				mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);
			}
		}
        // Detect when the user starts dragging the progress slider
        private void ProgressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            //stop the file
            playpause();
            progressIsDragging = true;
        }
        // Detect when the user stops dragging the progress slider
        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            progressIsDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);
            //play the file
            playpause();

        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			mediaPlayer.Volume = volumeSlider.Value;
		}
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files (*.mp4;*.mp3;*.avi;*.wav;*.wma;*.aac)|*.mp4;*.mp3;*.avi;*.wav;*.wma;*.aac|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Source = new Uri(openFileDialog.FileName, UriKind.Absolute);
                mediaPlayer.Play();
                progressTimer.Start();
                playPauseButton.Content = "Pause";
				isPlaying = true;
                txtFileName.Text = "";
            }


        }
        // Handle the DragEnter event to change the cursor when dragging files
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        // Handle the Drop event to load the dropped file into the media player
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    string filePath = files[0];
                    string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();

                    // Only accept specific audio formats
                    if (fileExtension == ".mp3" || fileExtension == ".wav" || fileExtension == ".wma" || fileExtension == ".aac" || fileExtension == ".mp4" || fileExtension == ".avi")
                    {
                        mediaPlayer.Source = new Uri(filePath);
                        mediaPlayer.Play();
                        isPlaying = true;
                        playPauseButton.Content = "Pause";
                        //txtFileName.Text = System.IO.Path.GetFileName(filePath); // Display the file name
                        txtFileName.Text = "";
                        progressTimer.Start();



                    }
                    else
                    {
                        MessageBox.Show("Unsupported file format. Please drop an audio file (.mp3, .wav, .wma, .aac, .mp4, .avi).");
                    }
                }
            }
        }
    }
}
