using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _5CRXmod;

partial class Form1
{
	private TimeSpan _timeRemaining;

	private bool _timerRunning;

	private Timer countdownTimer;

	private void btnAddTime_Click(object? sender, EventArgs e)
	{
		using SetTimeForm setTimeForm = new SetTimeForm();
		setTimeForm.StartPosition = FormStartPosition.Manual;
		setTimeForm.Location = new Point(base.Location.X - setTimeForm.Width, base.Location.Y);
		if (setTimeForm.ShowDialog() == DialogResult.OK)
		{
			_timeRemaining = new TimeSpan(setTimeForm.SelectedHours, setTimeForm.SelectedMinutes, 0);
			UpdateTimerDisplay();
			if (setTimeForm.StartImmediately && _timeRemaining.TotalSeconds > 0.0)
			{
				StartTimer();
			}
			else if (_timeRemaining.TotalSeconds > 0.0)
			{
				_timerRunning = false;
				countdownTimer.Stop();
				btnP.Text = "▶";
				btnS.Text = "⏹";
			}
			else
			{
				StopTimer();
			}
		}
	}

	private void btnP_Click(object? sender, EventArgs e)
	{
		if (_timerRunning)
		{
			PauseTimer();
		}
		else if (_timeRemaining.TotalSeconds > 0.0)
		{
			StartTimer();
		}
	}

	private void btnS_Click(object? sender, EventArgs e)
	{
		if (_activeTaskPanel != null)
		{
			StopTimer();
			StopM3u();
			_activeTaskPanel = null;
			_timeRemaining = TimeSpan.Zero;
			UpdateTimerDisplay();
		}
		else
		{
			StopTimer();
			_timeRemaining = TimeSpan.Zero;
			UpdateTimerDisplay();
		}
	}

	private void StartTimer()
	{
		countdownTimer.Start();
		_timerRunning = true;
		btnP.Text = "⏸";
		btnS.Text = "⏹";
	}

	private void StopTimer()
	{
		countdownTimer.Stop();
		_timerRunning = false;
		btnP.Text = "▶";
		btnS.Text = "⏹";
	}

	private void PauseTimer()
	{
		countdownTimer.Stop();
		_timerRunning = false;
		btnP.Text = "▶";
		btnS.Text = "⏹";
	}

	private async void countdownTimer_Tick(object? sender, EventArgs e)
	{
		if (_timeRemaining.TotalSeconds > 0.0)
		{
			_timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1L));
			UpdateTimerDisplay();
			if (_activeTaskPanel != null)
			{
				UpdateTaskProgress();
			}
			return;
		}
		countdownTimer.Stop();
		_timerRunning = false;
		btnP.Text = "▶";
		btnS.Text = "⏹";
		_timeRemaining = TimeSpan.Zero;
		UpdateTimerDisplay();
		if (_isLearningSession)
		{
			return;
		}
		PlayDoneSound();
		CompleteLearningSession();
		if (_activeTaskPanel != null)
		{
			UpdateTaskProgress();
			Panel panelToDelete = _activeTaskPanel;
			bool isFixed = (panelToDelete.Tag as TaskData)?.IsFixed ?? false;
			_activeTaskPanel = null;
			await Task.Delay(3000);
			if (!isFixed)
			{
				tasksListPanel.Controls.Remove(panelToDelete);
				panelToDelete.Dispose();
			}
			else
			{
				ResetTaskProgress(panelToDelete);
			}
			UpdateTaskInfo();
		}
	}

	private void UpdateTimerDisplay()
	{
		lblHours.Text = _timeRemaining.Hours.ToString("00");
		lblMinutes.Text = _timeRemaining.Minutes.ToString("00");
		lblSeconds.Text = _timeRemaining.Seconds.ToString("00");
	}
}
