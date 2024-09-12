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
			if (videoWidth > 0 && videoHeight > 0)
			{
				
				this.Width = videoWidth;
				this.Height = videoHeight;
			}
			else
			{
				this.Width=300;
				this.Height = 400;
			}
			// set the progress slider's maximum to the media duration(in seconds)
			progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
		}
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
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
            progressIsDragging = true;
        }

        // Detect when the user stops dragging the progress slider
        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            progressIsDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
			progressTimer.Stop(); // stop updating the progress slider
			progressSlider.Value = 1;//reset the slider
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			mediaPlayer.Volume = volumeSlider.Value;
		}
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files (*.mp4;*.mp3;*.avi)|*.mp4;*.mp3;*.avi|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Source = new Uri(openFileDialog.FileName, UriKind.Absolute);
				//progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                if (isPlaying)
                {
					mediaPlayer.Play();
                }
            }


        }
    }
}
