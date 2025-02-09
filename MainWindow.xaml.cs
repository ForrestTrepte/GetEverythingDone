using System;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Media;      // For playing sound
using System.IO;         // For reading credits.txt
using Microsoft.VisualBasic; // For Interaction.InputBox

namespace GetEverythingDone
{
    public partial class MainWindow : Window
    {
        // Collection of tasks.
        private ObservableCollection<TaskItem> tasks;
        // Global timer for the running task.
        private DispatcherTimer timer;
        // Time remaining in the current session.
        private TimeSpan remainingTime;
        // Flag to indicate if the timer is running.
        private bool isTimerRunning = false;
        // Reference to the currently running task.
        private TaskItem currentRunningTask;

        // Base duration (in seconds) for a fresh task.
        private readonly int baseDuration = 10;
        // Additional seconds to add each time a task is continued.
        private readonly int incrementDuration = 10;

        public MainWindow()
        {
            InitializeComponent();
            tasks = new ObservableCollection<TaskItem>();
            TaskListBox.ItemsSource = tasks;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Called every second while the timer is running.
        /// Updates both the display and the running task’s CurrentTime.
        /// When the timer runs out, plays a looping alert sound until the message box is dismissed.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (remainingTime.TotalSeconds > 0)
            {
                remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
                TimerTextBlock.Text = remainingTime.ToString(@"mm\:ss");
                if (currentRunningTask != null)
                {
                    // Update the running task’s CurrentTime property.
                    currentRunningTask.CurrentTime = remainingTime;
                }
            }
            else
            {
                timer.Stop();
                isTimerRunning = false;

                // Play the looping alert sound.
                // Ensure that the file exists at the given path.
                SoundPlayer player = new SoundPlayer(@"Sounds/234523__foolboymedia__notification-up-2.wav");
                try
                {
                    player.PlayLooping();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Alert sound could not be played: " + ex.Message);
                }

                // Show the "Time's up" message. Execution will pause here until the user dismisses the message.
                MessageBox.Show("Time's up for the current session!");

                // Stop the alert sound once the user dismisses the message.
                player.Stop();

                if (currentRunningTask != null)
                {
                    // When a task completes, increment its continuation count
                    // and update its time for the next run.
                    currentRunningTask.ContinuationCount++;
                    currentRunningTask.CurrentTime = TimeSpan.FromSeconds(baseDuration + currentRunningTask.ContinuationCount * incrementDuration);
                }
                TimerTextBlock.Text = "00:00";
                currentRunningTask = null;
            }
        }

        /// <summary>
        /// Adds a new task and initializes its time to the first run duration.
        /// </summary>
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var input = Interaction.InputBox("Enter task description:", "Add Task", "New Task");
            if (!string.IsNullOrWhiteSpace(input))
            {
                var newTask = new TaskItem
                {
                    Title = input,
                    ContinuationCount = 0,
                    // Initialize with the first run duration (10 seconds).
                    CurrentTime = TimeSpan.FromSeconds(baseDuration)
                };
                tasks.Add(newTask);
            }
        }

        /// <summary>
        /// Removes the selected task.
        /// If the removed task is currently running, stops the timer.
        /// </summary>
        private void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem task)
            {
                if (currentRunningTask == task)
                {
                    timer.Stop();
                    isTimerRunning = false;
                    currentRunningTask = null;
                    TimerTextBlock.Text = "00:00";
                }
                tasks.Remove(task);
            }
        }

        /// <summary>
        /// Moves the selected task up in the list.
        /// </summary>
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = TaskListBox.SelectedIndex;
            if (selectedIndex > 0)
            {
                var item = tasks[selectedIndex];
                tasks.RemoveAt(selectedIndex);
                tasks.Insert(selectedIndex - 1, item);
                TaskListBox.SelectedIndex = selectedIndex - 1;
            }
        }

        /// <summary>
        /// Moves the selected task down in the list.
        /// </summary>
        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = TaskListBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < tasks.Count - 1)
            {
                var item = tasks[selectedIndex];
                tasks.RemoveAt(selectedIndex);
                tasks.Insert(selectedIndex + 1, item);
                TaskListBox.SelectedIndex = selectedIndex + 1;
            }
        }

        /// <summary>
        /// Starts the selected task.
        /// If a different task is already running, stops it and resets its time.
        /// </summary>
        private void StartTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selectedTask)
            {
                // If another task is running, stop it and reset its time.
                if (currentRunningTask != null && currentRunningTask != selectedTask)
                {
                    timer.Stop();
                    isTimerRunning = false;
                    // Reset the previous task's time to its full scheduled duration.
                    currentRunningTask.CurrentTime = TimeSpan.FromSeconds(baseDuration + currentRunningTask.ContinuationCount * incrementDuration);
                }

                // Start or resume the selected task.
                if (currentRunningTask != selectedTask)
                {
                    currentRunningTask = selectedTask;
                    int scheduledTime = baseDuration + selectedTask.ContinuationCount * incrementDuration;
                    // Ensure the task starts with its full scheduled duration.
                    selectedTask.CurrentTime = TimeSpan.FromSeconds(scheduledTime);
                    remainingTime = TimeSpan.FromSeconds(scheduledTime);
                    TimerTextBlock.Text = remainingTime.ToString(@"mm\:ss");
                }

                timer.Start();
                isTimerRunning = true;
                CurrentTaskTextBlock.Text = selectedTask.Title;
            }
            else
            {
                MessageBox.Show("Please select a task first.");
            }
        }

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        private void PauseTask_Click(object sender, RoutedEventArgs e)
        {
            if (isTimerRunning)
            {
                timer.Stop();
                isTimerRunning = false;
            }
        }

        /// <summary>
        /// Resets the timer for the selected task to its full scheduled duration.
        /// </summary>
        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selectedTask)
            {
                int scheduledTime = baseDuration + selectedTask.ContinuationCount * incrementDuration;
                remainingTime = TimeSpan.FromSeconds(scheduledTime);
                selectedTask.CurrentTime = remainingTime;
                TimerTextBlock.Text = remainingTime.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// Reads the contents of credits.txt and displays them in a message box.
        /// </summary>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Read the attribution from credits.txt
                string credits = File.ReadAllText("credits.txt");
                MessageBox.Show(credits, "Credits", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load credits: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Represents a task with a title, a continuation count, and its current (or scheduled) run time.
    /// Implements INotifyPropertyChanged so that UI bindings update when CurrentTime changes.
    /// </summary>
    public class TaskItem : INotifyPropertyChanged
    {
        private TimeSpan currentTime;
        public string Title { get; set; }
        public int ContinuationCount { get; set; }
        public TimeSpan CurrentTime
        {
            get => currentTime;
            set
            {
                if (currentTime != value)
                {
                    currentTime = value;
                    OnPropertyChanged("CurrentTime");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Converter class to format a TimeSpan as a string in mm:ss format.
    /// </summary>
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
                return ts.ToString(@"mm\:ss");
            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
