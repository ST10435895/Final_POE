using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chatbot
{
    public partial class MainWindow : Window
    {
        // Activity log for tracking user actions
        private List<string> activityLog = new List<string>();

        // Quiz data
        private List<(string Question, string[] Options, int CorrectIndex)> quizQuestions;
        private int currentQuestionIndex = 0;
        private int quizScore = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeQuizQuestions();
            DisplayAsciiArt();
            PlayGreeting();
            DisplayWelcomeMessage();
        }

        // =============================
        // PART 1: Greeting + ASCII Art
        // =============================

        private void DisplayAsciiArt()
        {
            ChatDisplayTextBox.AppendText("=== Welcome to the Cybersecurity Awareness Chatbot ===\n");
        }

        private void PlayGreeting()
        {
            try
            {
                string audioPath = @"C:\Users\RC_Student_lab\source\repos\Chatbot\audio\Welcoming.wav";
                SoundPlayer player = new SoundPlayer(audioPath);
                player.Play();
                activityLog.Add("Greeting audio played.");
            }
            catch (Exception ex)
            {
                activityLog.Add($"Audio error: {ex.Message}");
            }
        }

        private void DisplayWelcomeMessage()
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Hello, what's your name?", "Welcome", "User");
            if (!string.IsNullOrWhiteSpace(name))
            {
                ChatDisplayTextBox.AppendText($"Bot: Hi {name}! I'm your Cybersecurity Assistant.\n");
                activityLog.Add($"User greeted: {name}");
            }
        }

        // =============================
        // TextBox Placeholder Handling
        // =============================

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb &&
                (tb.Text == "Task Title" || tb.Text == "Task Description (optional)" || tb.Text == "Type your message here..."))
            {
                tb.Text = "";
                tb.Foreground = Brushes.White;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
            {
                if (tb.Name == "TaskTitleTextBox") tb.Text = "Task Title";
                else if (tb.Name == "TaskDescriptionTextBox") tb.Text = "Task Description (optional)";
                else if (tb.Name == "ChatInputTextBox") tb.Text = "Type your message here...";
                tb.Foreground = Brushes.Gray;
            }
        }

        // =============================
        // PART 2: Chatbot / NLP
        // =============================

        private void ChatSendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = ChatInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            ChatDisplayTextBox.AppendText($"You: {input}\n");
            string response = GetNLPResponse(input);
            ChatDisplayTextBox.AppendText($"Bot: {response}\n");

            ChatInputTextBox.Text = "";
        }

        private string GetNLPResponse(string input)
        {
            string lower = input.ToLower();

            if (lower.Contains("task") || lower.Contains("remind"))
            {
                activityLog.Add("User asked about tasks.");
                return "You can manage your tasks in the Task Assistant tab.";
            }
            else if (lower.Contains("quiz"))
            {
                activityLog.Add("User asked about quiz.");
                return "Head over to the Cybersecurity Quiz tab to test your knowledge.";
            }
            else if (lower.Contains("log"))
            {
                ActivityLogListBox.Items.Clear();
                foreach (var log in activityLog.TakeLast(10))
                    ActivityLogListBox.Items.Add(log);
                return "Here are the recent activities logged.";
            }
            else if (lower.Contains("password") || lower.Contains("phishing") || lower.Contains("malware") || lower.Contains("safe browsing"))
            {
                activityLog.Add("User asked about cybersecurity topic.");
                return "Use strong passwords, avoid phishing scams, and update your software often!";
            }

            return "I'm not sure how to help with that. Try asking about tasks, quizzes, or cybersecurity tips.";
        }

        // =============================
        // PART 3: Task Assistant
        // =============================

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleTextBox.Text.Trim();
            string description = TaskDescriptionTextBox.Text.Trim();
            string reminder = ReminderDatePicker.SelectedDate?.ToShortDateString();

            if (string.IsNullOrEmpty(title) || title == "Task Title")
            {
                MessageBox.Show("Please enter a valid task title.");
                return;
            }

            string task = $"Task: {title}";
            if (!string.IsNullOrEmpty(description) && description != "Task Description (optional)")
                task += $" | Description: {description}";
            if (!string.IsNullOrEmpty(reminder))
                task += $" | Reminder: {reminder}";

            TaskListBox.Items.Add(task);
            activityLog.Add($"Task added: {title}");
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem != null)
            {
                string task = TaskListBox.SelectedItem.ToString();
                TaskListBox.Items.Remove(task);
                activityLog.Add($"Task completed: {task}");
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem != null)
            {
                string task = TaskListBox.SelectedItem.ToString();
                TaskListBox.Items.Remove(task);
                activityLog.Add($"Task deleted: {task}");
            }
        }

        // =============================
        // Cybersecurity Quiz
        // =============================

        private void InitializeQuizQuestions()
        {
            quizQuestions = new List<(string, string[], int)>
            {
                ("What should you do if you receive a suspicious email?", new[] { "Reply", "Ignore", "Report it as phishing" }, 2),
                ("Which is a strong password?", new[] { "password", "P@ssw0rd!", "qwerty" }, 1),
                ("True or False: Public Wi-Fi is safe for banking.", new[] { "True", "False", "" }, 1),
                ("What does 2FA stand for?", new[] { "Two-Factor Authentication", "Two-Faced Access", "Twice For Access" }, 0),
                ("Sign of phishing?", new[] { "Normal email", "Trusted link", "Unexpected attachment" }, 2),
            };
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex = 0;
            quizScore = 0;
            QuizFeedbackTextBlock.Text = "";
            QuizResultTextBlock.Text = "";
            QuizQuestionGrid.Visibility = Visibility.Visible;
            LoadQuizQuestion();
            activityLog.Add("Quiz started.");
        }

        private void LoadQuizQuestion()
        {
            if (currentQuestionIndex < quizQuestions.Count)
            {
                var q = quizQuestions[currentQuestionIndex];
                QuizQuestionTextBlock.Text = q.Question;
                QuizOptionARadio.Content = q.Options[0];
                QuizOptionBRadio.Content = q.Options[1];
                QuizOptionCRadio.Content = q.Options[2];
                QuizOptionCRadio.Visibility = string.IsNullOrEmpty(q.Options[2]) ? Visibility.Collapsed : Visibility.Visible;

                QuizOptionARadio.IsChecked = false;
                QuizOptionBRadio.IsChecked = false;
                QuizOptionCRadio.IsChecked = false;
            }
            else
            {
                QuizQuestionGrid.Visibility = Visibility.Collapsed;
                QuizResultTextBlock.Text = $"You scored {quizScore} out of {quizQuestions.Count}!";
                activityLog.Add($"Quiz completed: {quizScore}/{quizQuestions.Count}");
            }
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            int selected = -1;
            if (QuizOptionARadio.IsChecked == true) selected = 0;
            else if (QuizOptionBRadio.IsChecked == true) selected = 1;
            else if (QuizOptionCRadio.IsChecked == true) selected = 2;

            if (selected == -1)
            {
                MessageBox.Show("Please select an answer.");
                return;
            }

            if (selected == quizQuestions[currentQuestionIndex].CorrectIndex)
            {
                quizScore++;
                QuizFeedbackTextBlock.Text = "Correct!";
                QuizFeedbackTextBlock.Foreground = Brushes.Green;
            }
            else
            {
                QuizFeedbackTextBlock.Text = "Incorrect. Keep learning!";
                QuizFeedbackTextBlock.Foreground = Brushes.Red;
            }

            currentQuestionIndex++;
            LoadQuizQuestion();
        }
    }
}
