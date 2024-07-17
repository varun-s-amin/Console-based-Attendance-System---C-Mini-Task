using System;
using System.IO;
using System.Windows.Forms;

namespace AttendanceSystem
{
    public delegate void LateArrivalHandler(string message);

    public class AttendanceRecorder
    {
        public event LateArrivalHandler LateArrivalEvent;

        private const int MaxLateAllowedPerMonth = 3;
        private const int WorkingHoursStart = 9;
        private const int WorkingHoursEnd = 18;
        private const string LogFilePath = "attendance_log.txt";

        public void RecordAttendance(DateTime entryTime)
        {
            // Check if it's within working hours
            if (entryTime.Hour < WorkingHoursStart || entryTime.Hour >= WorkingHoursEnd)
            {
                MessageBox.Show("Sorry, you cannot log attendance outside working hours.");
                return;
            }

            // Check if the log file exists, if not, create it
            if (!File.Exists(LogFilePath))
            {
                using (StreamWriter sw = File.CreateText(LogFilePath))
                {
                    sw.WriteLine("Date,Time");
                }
            }

            // Check if the worker has logged in before
            bool isLate = false;
            string[] previousLogs = File.ReadAllLines(LogFilePath);
            foreach (string log in previousLogs)
            {
                string[] logData = log.Split(',');
                DateTime previousLogTime = DateTime.Parse(logData[1]);
                TimeSpan timeDifference = entryTime - previousLogTime;
                if (timeDifference.TotalHours > 1)
                {
                    isLate = true;
                    break;
                }
            }

            // Record attendance
            using (StreamWriter sw = File.AppendText(LogFilePath))
            {
                sw.WriteLine($"{entryTime.ToShortDateString()},{entryTime.ToShortTimeString()}");
            }

            // Check if late more than 3 times in a month
            if (isLate)
            {
                OnLateArrival("You are late! Casual leave will be deducted.");
            }
            else
            {
                MessageBox.Show("Attendance recorded successfully!");
            }
        }

        protected virtual void OnLateArrival(string message)
        {
            LateArrivalEvent?.Invoke(message);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private AttendanceRecorder recorder = new AttendanceRecorder();
        private DateTimePicker dateTimePicker;
        private Button recordButton;

        public MainForm()
        {
            InitializeComponents(); // Call method to initialize components

            recorder.LateArrivalEvent += HandleLateArrival;
        }

        private void InitializeComponents()
        {
            // Create and configure DateTimePicker
            dateTimePicker = new DateTimePicker();
            dateTimePicker.Format = DateTimePickerFormat.Custom;
            dateTimePicker.CustomFormat = "dd/MM/yyyy HH:mm";
            dateTimePicker.Location = new Point(20, 20);
            this.Controls.Add(dateTimePicker);

            // Create and configure Record Button
            recordButton = new Button();
            recordButton.Text = "Record Attendance";
            recordButton.Location = new Point(20, 60);
            recordButton.Click += RecordButton_Click; // Subscribe Click event
            this.Controls.Add(recordButton);
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            DateTime entryTime = dateTimePicker.Value;
            recorder.RecordAttendance(entryTime);
        }

        private void HandleLateArrival(string message)
        {
            MessageBox.Show(message);
        }
    }
}

