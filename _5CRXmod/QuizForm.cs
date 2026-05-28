using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace _5CRXmod;

public class AnswerRecord
{
    public Question Question { get; set; }
    public bool IsCorrect { get; set; }
    public int SelectedOption { get; set; } = -1;
}

public class QuizForm : Form
{
    private readonly string _subjectName;
    private readonly int _totalMinutes;

    private readonly Label _lblHeader;
    private readonly Label _lblProgress;
    private readonly Label _lblQuestion;
    private readonly Panel _answerPanel;
    private readonly Label _lblStats;

    private List<Question> _questions = new();
    private int _currentIndex;
    private int _correct;
    private int _wrong;
    private int _streak;
    private int _maxStreak;
    private int _totalScore;
    private TimeSpan _timeRemaining;
    private readonly Timer _hiddenTimer;

    private readonly List<AnswerRecord> _answers = new();

    private bool _isCorrectionMode;
    private int _correctionIndex;

    public int TotalScore => _totalScore;
    public int MinutesStudied => _totalMinutes - (int)_timeRemaining.TotalMinutes;

    private readonly Color _correctColor = Color.FromArgb(88, 204, 2);
    private readonly Color _wrongColor = Color.FromArgb(255, 68, 68);

    public QuizForm(string subjectName, int minutes)
    {
        _subjectName = subjectName;
        _totalMinutes = minutes;
        _timeRemaining = TimeSpan.FromMinutes(minutes);

        Text = "";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(240, 420);
        BackColor = Color.FromArgb(30, 30, 30);
        ShowInTaskbar = false;
        TopMost = true;

        _lblHeader = new Label
        {
            Height = 24,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.FromArgb(88, 204, 2),
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _lblProgress = new Label
        {
            Height = 18,
            Dock = DockStyle.Top,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 7f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        Panel questionBg = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(10, 0, 10, 0)
        };

        _lblQuestion = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        questionBg.Controls.Add(_lblQuestion);

        _answerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(10, 10, 10, 5)
        };

        _lblStats = new Label
        {
            Height = 18,
            Dock = DockStyle.Bottom,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 7f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        Label lblEsc = new Label
        {
            Text = "ESC para salir",
            Height = 14,
            Dock = DockStyle.Bottom,
            ForeColor = Color.FromArgb(60, 60, 60),
            Font = new Font("Segoe UI", 6f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        Controls.Add(lblEsc);
        Controls.Add(_lblStats);
        Controls.Add(_answerPanel);
        Controls.Add(questionBg);
        Controls.Add(_lblProgress);
        Controls.Add(_lblHeader);

        _hiddenTimer = new Timer { Interval = 1000 };
        _hiddenTimer.Tick += HiddenTimer_Tick;

        LoadQuestions();

        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_isCorrectionMode)
                    ShowCorrectionSummary();
                else
                    EndQuiz();
            }
        };
    }

    private void LoadQuestions()
    {
        var levels = QuestionBank.Load(_subjectName);
        if (levels == null || levels.Count == 0)
        {
            _lblQuestion.Text = "No hay preguntas disponibles.";
            return;
        }
        _questions = QuestionBank.GetRandomQuestions(levels, 15, 3);
        if (_questions.Count == 0)
        {
            _lblQuestion.Text = "No hay preguntas disponibles.";
            return;
        }
        _hiddenTimer.Start();
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        _isCorrectionMode = false;
        _answerPanel.Visible = true;
        _lblStats.Visible = true;

        if (_currentIndex >= _questions.Count)
        {
            EndQuiz();
            return;
        }
        var q = _questions[_currentIndex];

        _lblHeader.Text = $"{_subjectName}   ⭐ {_totalScore} pts";
        _lblProgress.Text = $"Pregunta {_currentIndex + 1} / {_questions.Count}";
        _lblQuestion.Text = q.Text;

        _answerPanel.Controls.Clear();

        if (q.Type == "truefalse")
            CreateTrueFalseButtons(q);
        else
            CreateOptionButtons(q);

        UpdateStats();
    }

    private void CreateTrueFalseButtons(Question q)
    {
        Button btnTrue = new Button
        {
            Text = "VERDADERO",
            Size = new Size(100, 50),
            Location = new Point(10, 15),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = _correctColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnTrue.Click += (_, _) => AnswerQuestion(q, true, -1);

        Button btnFalse = new Button
        {
            Text = "FALSO",
            Size = new Size(100, 50),
            Location = new Point(120, 15),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = _wrongColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnFalse.Click += (_, _) => AnswerQuestion(q, false, -1);

        _answerPanel.Controls.Add(btnTrue);
        _answerPanel.Controls.Add(btnFalse);
    }

    private void CreateOptionButtons(Question q)
    {
        int y = 5;
        int btnH = 35;
        string[] labels = { "A", "B", "C", "D" };

        for (int i = 0; i < q.Options.Count; i++)
        {
            int idx = i;
            Button btn = new Button
            {
                Text = $"{labels[i]}: {q.Options[i]}",
                Width = 220,
                Height = btnH,
                Location = new Point(0, y),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(80, 80, 80) },
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0)
            };
            btn.Click += (_, _) => AnswerQuestion(q, idx == q.CorrectIndex, idx);
            _answerPanel.Controls.Add(btn);
            y += btnH + 4;
        }
    }

    private void AnswerQuestion(Question q, bool isCorrect, int selectedOption)
    {
        _answers.Add(new AnswerRecord
        {
            Question = q,
            IsCorrect = isCorrect,
            SelectedOption = selectedOption
        });

        if (isCorrect)
        {
            _correct++;
            _streak++;
            if (_streak > _maxStreak) _maxStreak = _streak;
            int multiplier = _streak >= 3 ? 3 : 2;
            _totalScore += q.Points * multiplier / 2;
        }
        else
        {
            _wrong++;
            _streak = 0;
        }

        _currentIndex++;
        ShowQuestion();
    }

    private void HiddenTimer_Tick(object? sender, EventArgs e)
    {
        _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
        if (_timeRemaining.TotalSeconds <= 0)
        {
            EndQuiz();
        }
    }

    private void EndQuiz()
    {
        _hiddenTimer.Stop();
        _answerPanel.Controls.Clear();
        ShowCorrectionSummary();
    }

    private void ShowCorrectionSummary()
    {
        _isCorrectionMode = true;
        _correctionIndex = 0;
        _answerPanel.Visible = true;
        _lblStats.Visible = false;

        _lblHeader.Text = $"{_subjectName}   ✅ {_correct}   ❌ {_wrong}   ⭐ {_totalScore}";
        _lblProgress.Text = _maxStreak >= 3 ? $"🔥 Mejor racha: {_maxStreak}" : "";

        _answerPanel.Controls.Clear();

        Label lblSummary = new Label
        {
            Text = $"Preguntas: {_questions.Count}\nCorrectas: {_correct}\nIncorrectas: {_wrong}\nPuntos: {_totalScore}",
            Width = 220,
            Height = 80,
            Location = new Point(10, 5),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        _answerPanel.Controls.Add(lblSummary);

        Button btnReview = new Button
        {
            Text = "VER CORRECCIÓN",
            Size = new Size(180, 35),
            Location = new Point(30, 95),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = Color.FromArgb(88, 204, 2),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold)
        };
        btnReview.Click += (_, _) => ShowCorrection(0);
        _answerPanel.Controls.Add(btnReview);

        Button btnOk = new Button
        {
            Text = "OK",
            Size = new Size(180, 30),
            Location = new Point(30, 140),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(80, 80, 80) },
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold)
        };
        btnOk.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        _answerPanel.Controls.Add(btnOk);

        _lblQuestion.Text = "Sesión completada";
    }

    private void ShowCorrection(int index)
    {
        if (index < 0 || index >= _answers.Count) return;
        _correctionIndex = index;
        var rec = _answers[index];
        var q = rec.Question;

        _lblHeader.Text = $"Corrección {index + 1}/{_answers.Count}";
        _lblProgress.Text = rec.IsCorrect ? "✅ Correcta" : "❌ Incorrecta";
        _lblQuestion.Text = q.Text;

        _answerPanel.Controls.Clear();

        if (q.Type == "truefalse")
        {
            ShowTrueFalseCorrection(q, rec.IsCorrect);
        }
        else
        {
            ShowOptionsCorrection(q, rec);
        }

        int y = _answerPanel.Height - 60;

        if (index > 0)
        {
            Button btnPrev = new Button
            {
                Text = "◀",
                Size = new Size(40, 30),
                Location = new Point(10, y),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(80, 80, 80) },
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnPrev.Click += (_, _) => ShowCorrection(index - 1);
            _answerPanel.Controls.Add(btnPrev);
        }

        if (index < _answers.Count - 1)
        {
            Button btnNext = new Button
            {
                Text = "▶",
                Size = new Size(40, 30),
                Location = new Point(190, y),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(80, 80, 80) },
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnNext.Click += (_, _) => ShowCorrection(index + 1);
            _answerPanel.Controls.Add(btnNext);
        }

        Button btnBack = new Button
        {
            Text = "VOLVER",
            Size = new Size(80, 30),
            Location = new Point(90, y),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(80, 80, 80) },
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 7f, FontStyle.Bold)
        };
        btnBack.Click += (_, _) => ShowCorrectionSummary();
        _answerPanel.Controls.Add(btnBack);
    }

    private void ShowTrueFalseCorrection(Question q, bool userCorrect)
    {
        Color trueColor = q.CorrectAnswer ? _correctColor : Color.FromArgb(50, 50, 50);
        Color falseColor = !q.CorrectAnswer ? _correctColor : Color.FromArgb(50, 50, 50);

        Button btnTrue = new Button
        {
            Text = "VERDADERO",
            Size = new Size(100, 45),
            Location = new Point(10, 10),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = trueColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold)
        };
        _answerPanel.Controls.Add(btnTrue);

        Button btnFalse = new Button
        {
            Text = "FALSO",
            Size = new Size(100, 45),
            Location = new Point(120, 10),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = falseColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold)
        };
        _answerPanel.Controls.Add(btnFalse);

        Label lblResult = new Label
        {
            Text = userCorrect ? "✅ ¡Correcto!" : $"❌ Respuesta: {(q.CorrectAnswer ? "VERDADERO" : "FALSO")}",
            Width = 220,
            Height = 25,
            Location = new Point(10, 65),
            ForeColor = userCorrect ? _correctColor : _wrongColor,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        _answerPanel.Controls.Add(lblResult);
    }

    private void ShowOptionsCorrection(Question q, AnswerRecord rec)
    {
        int y = 5;
        int btnH = 35;
        string[] labels = { "A", "B", "C", "D" };

        for (int i = 0; i < q.Options.Count; i++)
        {
            Color bkColor;
            if (i == q.CorrectIndex)
                bkColor = _correctColor;
            else if (i == rec.SelectedOption && !rec.IsCorrect)
                bkColor = _wrongColor;
            else
                bkColor = Color.FromArgb(45, 45, 45);

            Color fgColor = (i == q.CorrectIndex || (i == rec.SelectedOption && !rec.IsCorrect))
                ? Color.White : Color.LightGray;

            Button btn = new Button
            {
                Text = $"{labels[i]}: {q.Options[i]}",
                Width = 220,
                Height = btnH,
                Location = new Point(0, y),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = bkColor,
                ForeColor = fgColor,
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0),
                Enabled = false
            };
            _answerPanel.Controls.Add(btn);
            y += btnH + 4;
        }

        Label lblResult = new Label
        {
            Text = rec.IsCorrect ? "✅ ¡Correcto!" : "❌ Incorrecta",
            Width = 220,
            Height = 20,
            Location = new Point(10, y + 2),
            ForeColor = rec.IsCorrect ? _correctColor : _wrongColor,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        _answerPanel.Controls.Add(lblResult);
    }

    private void UpdateStats()
    {
        int remaining = _questions.Count - _currentIndex;
        string streakText = _streak >= 3 ? $" 🔥x{_streak}" : "";
        _lblStats.Text = $"✅ {_correct}  ·  ❌ {_wrong}  ·  ⏳ {remaining}{streakText}";
    }
}
