using System;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;
// For a simple input dialog
using Microsoft.VisualBasic;

namespace GetEverythingDone
{
    public partial class MainWindow : Window
    {
        // A collection to hold our tasks.
        private ObservableCollection<TaskItem> tasks;
        // DispatcherTimer to manage the countdown.
        private DispatcherTimer timer;
        private TimeSpan remainingTime;
        private bool isTimerRunning = false;

        // Base duration in minutes for a fresh task.
        private readonly int baseDuration = 15;
        // Additional minutes to add each time a task is continued.
        private readonly int incrementDuration = 5;

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
        /// Called each second while the timer is running.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (remainingTime.TotalSeconds > 0)
            {
                remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
                TimerTextBlock.Text = remainingTime.ToString(@"mm\:ss");
            }
            else
            {
                timer.Stop();
                isTimerRunning = false;
                MessageBox.Show("Time's up for the current session!");

                // Increase the continuation count so that next time the task duration increases.
                if (TaskListBox.SelectedItem is TaskItem currentTask)
                {
                    currentTask.ContinuationCount++;
                }
                TimerTextBlock.Text = "00:00";
            }
        }

        /// <summary>
        /// Adds a new task after prompting the user for a description.
        /// </summary>
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // Using VisualBasic.Interaction.InputBox for simplicity
            var input = Interaction.InputBox("Enter task description:", "Add Task", "New Task");
            if (!string.IsNullOrWhiteSpace(input))
            {
                tasks.Add(new TaskItem { Title = input, ContinuationCount = 0 });
            }
        }

        /// <summary>
        /// Removes the selected task.
        /// </summary>
        private void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem task)
            {
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
        /// Starts the timer for the selected task using the recommended duration.
        /// </summary>
        private void StartTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem task)
            {
                CurrentTaskTextBlock.Text = task.Title;
                // Calculate duration: base time + (number of continuations * increment)
                int durationMinutes = baseDuration + task.ContinuationCount * incrementDuration;
                remainingTime = TimeSpan.FromMinutes(durationMinutes);
                TimerTextBlock.Text = remainingTime.ToString(@"mm\:ss");
                timer.Start();
                isTimerRunning = true;
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
        /// Resets the timer for the selected task.
        /// </summary>
        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem task)
            {
                int durationMinutes = baseDuration + task.ContinuationCount * incrementDuration;
                remainingTime = TimeSpan.FromMinutes(durationMinutes);
                TimerTextBlock.Text = remainingTime.ToString(@"mm\:ss");
            }
        }
    }

    /// <summary>
    /// Represents a task with a title and a count of how many times it has been continued.
    /// </summary>
    public class TaskItem
    {
        public string Title { get; set; }
        public int ContinuationCount { get; set; }
    }
}
