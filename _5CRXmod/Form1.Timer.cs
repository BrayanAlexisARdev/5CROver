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

	private int _manualHours;

	private int _manualMinutes;

	private double _manualTotalSeconds;

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
			else
			{
				UpdateNodeProgressFromTimer();
			}
			return;
		}
		countdownTimer.Stop();
		_timerRunning = false;
		btnP.Text = "▶";
		btnS.Text = "⏹";
		_timeRemaining = TimeSpan.Zero;
		UpdateTimerDisplay();
		ResetNodeProgress();
		if (_isLearningSession)
		{
			return;
		}
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

	private void UpdateNodeProgressFromTimer()
	{
		if (_nodeIntensities == null) return;
		double progress = _manualTotalSeconds > 0 ? 1 - (_timeRemaining.TotalSeconds / _manualTotalSeconds) : 0;
		for (int i = 0; i < _nodeCount; i++)
		{
			double nodeStart = (double)i / _nodeCount;
			double nodeEnd = (double)(i + 1) / _nodeCount;
			double raw = 0;
			if (progress >= nodeEnd)
				raw = 1;
			else if (progress > nodeStart)
				raw = (progress - nodeStart) / (nodeEnd - nodeStart);
			double eased = raw < 0.5 ? 4 * raw * raw * raw : 1 - Math.Pow(-2 * raw + 2, 3) / 2;
			_nodeIntensities[i] = Math.Min(eased * 1.2, 1);
		}
		pnlProgressFill?.Invalidate();
	}

	private void ResetNodeProgress()
	{
		if (_nodeIntensities == null) return;
		for (int i = 0; i < _nodeCount; i++)
			_nodeIntensities[i] = 0;
		pnlProgressFill?.Invalidate();
	}
}
