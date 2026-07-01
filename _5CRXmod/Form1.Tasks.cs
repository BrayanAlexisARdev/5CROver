using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace _5CRXmod;

partial class Form1
{
	private Panel? _activeTaskPanel;

	private double _activeTaskTotalSeconds;

	private void btnAddTask_Click(object? sender, EventArgs e)
	{
		using EditTaskForm newTaskForm = new EditTaskForm("NEW TASK", "", TimeSpan.Zero, "tasks_TSK.png");
		newTaskForm.Location = new Point(Left - newTaskForm.Width, Top);
		if (newTaskForm.ShowDialog(this) == DialogResult.OK)
		{
			AddTaskToPanel(newTaskForm.TaskName, newTaskForm.TaskTime, "", newTaskForm.SelectedIcon);
		}
	}

	private void AddTaskToPanel(string taskName, TimeSpan taskTime, string m3uPath, string iconName = "tasks_TSK.png", bool isFixed = false)
	{
		(Color, Color, Color, Color) theme = _themes[_currentThemeIndex];
		Panel taskPanel = new Panel
		{
			Height = 60,
			Width = base.Width / 2,
			BackColor = (isFixed ? Color.FromArgb(216, theme.Item2) : theme.Item2),
			Margin = new Padding(0),
			BorderStyle = BorderStyle.None,
			Tag = new TaskData
			{
				Time = taskTime,
				M3uPath = m3uPath,
				IsFixed = isFixed
			}
		};
		PictureBox picTask = new PictureBox
		{
			Size = new Size(32, 32),
			SizeMode = PictureBoxSizeMode.Zoom,
			BackColor = Color.Transparent,
			Location = new Point(5, 12)
		};
		try
		{
		string testPath = PathHelper.ResolveImg(iconName);
		if (File.Exists(testPath))
			picTask.Image = PathHelper.LoadImage(testPath);
		}
		catch (Exception ex)
		{
			Logger.Error("Form1.AddTaskToPanel.Icon", ex);
		}
		Label lblTaskName = new BufferedLabel
		{
			Text = taskName.ToUpper(),
			Location = new Point(42, 8),
			AutoSize = false,
			Size = new Size(taskPanel.Width - 45, 20),
			BackColor = Color.Transparent,
			Font = ((FontHelper.CustomFontFamily != null) ? new Font(FontHelper.CustomFontFamily, 6.5f, FontStyle.Bold) : new Font("Segoe UI", 6.5f, FontStyle.Bold))
		};
		Label lblTaskTime = new BufferedLabel
		{
			Text = FormatTaskTime(taskTime),
			Location = new Point(42, 28),
			AutoSize = true,
			BackColor = Color.Transparent,
			Font = ((FontHelper.CustomFontFamily != null) ? new Font(FontHelper.CustomFontFamily, 7f, FontStyle.Regular) : new Font("Segoe UI", 7f, FontStyle.Regular))
		};

		taskPanel.Click += delegate
		{
			ToggleTask(taskPanel, (TaskData)taskPanel.Tag);
		};
		lblTaskName.Click += delegate
		{
			ToggleTask(taskPanel, (TaskData)taskPanel.Tag);
		};
		lblTaskTime.Click += delegate
		{
			ToggleTask(taskPanel, (TaskData)taskPanel.Tag);
		};
		picTask.Click += delegate
		{
			ToggleTask(taskPanel, (TaskData)taskPanel.Tag);
		};
		ContextMenuStrip menu = new ContextMenuStrip();
		ToolStripMenuItem itemEdit = new ToolStripMenuItem("EDITAR");
		itemEdit.Click += delegate
		{
			TaskData data = (TaskData)taskPanel.Tag;
			using EditTaskForm editForm = new EditTaskForm("EDIT TASK", lblTaskName.Text, data.Time, "tasks_TSK.png");
			editForm.Location = new Point(Left - editForm.Width, Top);
			if (editForm.ShowDialog(this) == DialogResult.OK)
			{
				lblTaskName.Text = editForm.TaskName.Substring(0, Math.Min(8, editForm.TaskName.Length)).ToUpper();
				data.Time = editForm.TaskTime;
				lblTaskTime.Text = FormatTaskTime(editForm.TaskTime);
				try
				{
					string iconPath = PathHelper.ResolveImg(editForm.SelectedIcon);
					if (File.Exists(iconPath)) picTask.Image = PathHelper.LoadImage(iconPath);
				}
				catch (Exception ex) { Logger.Error("Form1.EditTaskIcon", ex); }
			}
		};
		menu.Items.Add(itemEdit);
		taskPanel.ContextMenuStrip = menu;
		lblTaskName.ContextMenuStrip = menu;
		lblTaskTime.ContextMenuStrip = menu;
		picTask.ContextMenuStrip = menu;
		taskPanel.Controls.Add(lblTaskName);
		taskPanel.Controls.Add(lblTaskTime);
		taskPanel.Controls.Add(picTask);
		int index = tasksListPanel.Controls.Count;
		for (int i2 = 0; i2 < tasksListPanel.Controls.Count; i2++)
		{
			if (tasksListPanel.Controls[i2].Name == "pnlAddSlot")
			{
				index = i2;
				break;
			}
		}
		if (index < tasksListPanel.Controls.Count)
		{
			Control slot = tasksListPanel.Controls[index];
			tasksListPanel.Controls.Remove(slot);
			tasksListPanel.Controls.Add(taskPanel);
			tasksListPanel.Controls.SetChildIndex(taskPanel, index);
			slot.Dispose();
		}
		else
		{
			tasksListPanel.Controls.Add(taskPanel);
		}
		UpdateControlContrast(taskPanel, theme.Item2);
	}

	private void ToggleTask(Panel taskPanel, TaskData data)
	{
		if (_activeTaskPanel == taskPanel)
		{
			StopTimer();
			_activeTaskPanel = null;
			_timeRemaining = TimeSpan.Zero;
			UpdateTimerDisplay();
			ResetTaskProgress(taskPanel);
		}
		else if (!_timerRunning)
		{
			_activeTaskPanel = taskPanel;
			_activeTaskTotalSeconds = data.Time.TotalSeconds;
			_timeRemaining = data.Time;
			UpdateTimerDisplay();
			StartTimer();
		}
		UpdateTaskInfo();
	}

	private void UpdateTaskInfo()
	{
		if (_activeTaskPanel != null)
		{
			Label? nameLabel = null;
			foreach (Control c in _activeTaskPanel.Controls)
			{
				if (c is Label lbl)
				{
					nameLabel = lbl;
					break;
				}
			}
			if (nameLabel != null && _activeTaskPanel.Tag is TaskData data)
			{
				lblTaskInfo.Text = $"{nameLabel.Text}  •  {FormatTaskTime(data.Time)}";
				lblTaskInfo.ForeColor = Color.White;
			}
		}
		else
		{
			lblTaskInfo.Text = "NO TASK";
			lblTaskInfo.ForeColor = Color.Gray;
		}
	}

	private void UpdateTaskProgress()
	{
		if (_activeTaskPanel == null)
		{
			pnlProgressFill.Width = 0;
			return;
		}
		double percent = (_activeTaskTotalSeconds - _timeRemaining.TotalSeconds) / _activeTaskTotalSeconds;
		pnlProgressFill.Width = (int)((double)pnlProgressBg.Width * percent);
	}

	private void ResetTaskProgress(Panel taskPanel)
	{
		pnlProgressFill.Width = 0;
	}

	private static string FormatTaskTime(TimeSpan t)
	{
		if (t.TotalHours >= 1.0)
			return $"{(int)t.TotalHours}H {t.Minutes}M";
		return $"{(int)t.TotalMinutes} MIN";
	}

	private void AddAutoButtons()
	{
		Panel pnlAuto = new Panel
		{
			Dock = DockStyle.Right,
			Width = 80,
			BackColor = Color.FromArgb(20, 20, 20)
		};
		tasksHeaderPanel.Controls.Add(pnlAuto);
		for (int i = 0; i < 2; i++)
		{
			Button btnAuto = new Button
			{
				Text = "AUTO",
				Width = 35,
				Height = 20,
				Top = 2,
				Left = 5 + i * 40,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 5f, FontStyle.Bold),
				BackColor = Color.FromArgb(50, Color.Gray),
				ForeColor = Color.White
			};
			btnAuto.FlatAppearance.BorderSize = 0;
			pnlAuto.Controls.Add(btnAuto);
		}
		lblTasks.SendToBack();
	}

	private void AddEmptySlot()
	{
		Panel pnlAdd = new Panel
		{
			Height = 60,
			Width = base.Width / 2,
			BackColor = Color.FromArgb(40, Color.Black),
			BorderStyle = BorderStyle.None,
			Margin = new Padding(0),
			Name = "pnlAddSlot"
		};
		Button btnPlus = new Button
		{
			Text = "+",
			Dock = DockStyle.Fill,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 20f, FontStyle.Bold),
			ForeColor = Color.White,
			BackColor = Color.FromArgb(20, 20, 20)
		};
		btnPlus.FlatAppearance.BorderSize = 0;
		btnPlus.Click += delegate(object? s, EventArgs e)
		{
			btnAddTask_Click(s, e);
		};
		pnlAdd.Controls.Add(btnPlus);
		tasksListPanel.Controls.Add(pnlAdd);
	}
}
