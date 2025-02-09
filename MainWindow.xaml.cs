using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GetEverythingDone
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
        private Timer taskTimer;
        private TaskItem currentTask;
        private int sessionMultiplier = 10;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
                currentTask = task;
                int sessionTime = sessionMultiplier * (currentTask.SessionsCompleted + 1);
                taskTimer = new Timer(sessionTime * 60000);
                taskTimer.Elapsed += TaskCompleted;
                taskTimer.Start();
                MessageBox.Show($"Task '{task.Name}' started for {sessionTime} minutes.");
            }
        }

        private void TaskCompleted(object sender, ElapsedEventArgs e)
        {
            taskTimer.Stop();
            currentTask.SessionsCompleted++;
            Dispatcher.Invoke(() => MessageBox.Show($"Task '{currentTask.Name}' completed session {currentTask.SessionsCompleted}.", "Task Completed"));
        }
    }

    public class TaskItem
    {
        public string Name { get; set; }
        public int SessionsCompleted { get; set; }

        public TaskItem(string name)
        {
            Name = name;
            SessionsCompleted = 0;
        }
    }
}
