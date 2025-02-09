using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace GetEverythingDone
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
        private Timer taskTimer;
        private TaskItem currentTask;
        private const double TEST_SESSION_DURATION_SECONDS = 10;
        private double sessionMultiplier = TEST_SESSION_DURATION_SECONDS / 60.0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void PauseTask()
        {
            if (currentTask != null)
            {
                taskTimer.Stop();
                MessageBox.Show($"Task '{currentTask.Name}' paused at {currentTask.TimeRemaining} seconds remaining.");
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TaskInput.Text))
            {
                Tasks.Add(new TaskItem(TaskInput.Text));
                TaskInput.Clear();
            }
        }

        private void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem is TaskItem task)
            {
                Tasks.Remove(task);
            }
        }

        private void StartTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem is TaskItem task)
            {
                if (currentTask != null && currentTask != task)
                {
                    PauseTask();
                }

                currentTask = task;
                int sessionTime = (int)(sessionMultiplier * (currentTask.SessionsCompleted + 1) * 60); // Convert to seconds
                currentTask.TimeRemaining = sessionTime;

                if (taskTimer == null)
                {
                    taskTimer = new Timer(1000);
                    taskTimer.Elapsed += UpdateTaskTime;
                }

                taskTimer.Start();
                MessageBox.Show($"Task '{task.Name}' started for {sessionTime} seconds.");
            }
        }

        private void UpdateTaskTime(object sender, ElapsedEventArgs e)
        {
            if (currentTask != null && currentTask.TimeRemaining > 0)
            {
                currentTask.TimeRemaining--;
            }
            else
            {
                TaskCompleted();
            }
        }

        private void TaskCompleted()
        {
            taskTimer.Stop();
            currentTask.SessionsCompleted++;
            currentTask.TimeRemaining = 0;
            Dispatcher.Invoke(() => MessageBox.Show($"Task '{currentTask.Name}' completed session {currentTask.SessionsCompleted}.", "Task Completed"));
            currentTask = null;
        }
    }

    public class TaskItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private int _sessionsCompleted;
        private double _timeRemaining;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public int SessionsCompleted
        {
            get => _sessionsCompleted;
            set
            {
                _sessionsCompleted = value;
                OnPropertyChanged(nameof(SessionsCompleted));
            }
        }

        public double TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged(nameof(TimeRemaining));
            }
        }

        public TaskItem(string name)
        {
            Name = name;
            SessionsCompleted = 0;
            TimeRemaining = 0;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
