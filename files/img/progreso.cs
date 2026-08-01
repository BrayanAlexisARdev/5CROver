using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Timers;

namespace TemporizadorNodos
{
    public partial class MainForm : Form
    {
        // Configuración
        private const int TOTAL_NODOS = 10;
        private const int TOTAL_SEGUNDOS = 300; // 5 minutos
        private const double INTERVALO_MS = 50;

        // Estado
        private double segundosTranscurridos = 0;
        private System.Timers.Timer temporizador;
        private bool corriendo = false;
        private bool pausado = false;
        private DateTime tiempoInicio;

        // Controles
        private Panel panelNodos;
        private Label labelTiempo;
        private Button botonIniciar;
        private Button botonPausar;
        private Button botonReiniciar;
        private Label labelEstado;

        public MainForm()
        {
            InitializeComponent();
            ConfigurarControles();
            InicializarNodos();
            ActualizarUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Temporizador 5 minutos";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(11, 13, 16);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void ConfigurarControles()
        {
            // Panel de nodos
            panelNodos = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(445, 70),
                BackColor = Color.Transparent
            };
            this.Controls.Add(panelNodos);

            // Label tiempo
            labelTiempo = new Label
            {
                Location = new Point(20, 110),
                Size = new Size(445, 50),
                Text = "05:00",
                Font = new Font("Segoe UI", 24, FontStyle.Regular),
                ForeColor = Color.FromArgb(240, 244, 250),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(30, 35, 42)
            };
            this.Controls.Add(labelTiempo);

            // Botón Iniciar
            botonIniciar = new Button
            {
                Location = new Point(60, 180),
                Size = new Size(100, 35),
                Text = "▶ Iniciar",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(61, 140, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            botonIniciar.Click += BotonIniciar_Click;
            this.Controls.Add(botonIniciar);

            // Botón Pausar
            botonPausar = new Button
            {
                Location = new Point(180, 180),
                Size = new Size(100, 35),
                Text = "⏸ Pausa",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 56, 69),
                ForeColor = Color.FromArgb(220, 227, 236),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            botonPausar.Click += BotonPausar_Click;
            this.Controls.Add(botonPausar);

            // Botón Reiniciar
            botonReiniciar = new Button
            {
                Location = new Point(300, 180),
                Size = new Size(100, 35),
                Text = "⟲ Reiniciar",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(74, 58, 58),
                ForeColor = Color.FromArgb(240, 216, 216),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            botonReiniciar.Click += BotonReiniciar_Click;
            this.Controls.Add(botonReiniciar);

            // Label estado
            labelEstado = new Label
            {
                Location = new Point(20, 230),
                Size = new Size(445, 25),
                Text = "● listo",
                ForeColor = Color.FromArgb(109, 123, 138),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(labelEstado);
        }

        private void InicializarNodos()
        {
            panelNodos.Controls.Clear();
            int anchoNodo = Math.Min(60, (panelNodos.Width - 10) / TOTAL_NODOS);
            int espacioTotal = panelNodos.Width - (TOTAL_NODOS * anchoNodo);
            int espaciado = espacioTotal / (TOTAL_NODOS + 1);
            int altoNodo = anchoNodo;

            for (int i = 0; i < TOTAL_NODOS; i++)
            {
                double escalaBase = CalcularEscalaBase(i, TOTAL_NODOS);
                int x = espaciado + i * (anchoNodo + espaciado);
                int y = (panelNodos.Height - altoNodo) / 2;

                Panel nodo = new Panel
                {
                    Location = new Point(x, y),
                    Size = new Size(anchoNodo, altoNodo),
                    BackColor = Color.FromArgb(40, 46, 54),
                    Tag = escalaBase
                };

                // Dibujar el círculo con Graphics
                nodo.Paint += (sender, e) =>
                {
                    DibujarNodo(sender, e, 0.5, 0.35);
                };

                panelNodos.Controls.Add(nodo);
            }
        }

        private double CalcularEscalaBase(int indice, int total)
        {
            if (total <= 1) return 1.0;
            double centro = (total - 1) / 2.0;
            double rango = centro;
            if (rango == 0) return 1.0;

            double t = (indice - centro) / rango;
            double cosVal = Math.Cos(t * Math.PI / 2);
            double minScale = 0.70;
            double maxScale = 1.0;
            return minScale + (maxScale - minScale) * cosVal;
        }

        private void DibujarNodo(object sender, PaintEventArgs e, double intensidad, double opacidad)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;

            double escalaBase = (double)panel.Tag;
            double escalaFinal = escalaBase * (0.5 + 0.6 * intensidad);
            
            int tamaño = (int)(Math.Min(panel.Width, panel.Height) * escalaFinal);
            int x = (panel.Width - tamaño) / 2;
            int y = (panel.Height - tamaño) / 2;

            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle rect = new Rectangle(x, y, tamaño, tamaño);
                path.AddEllipse(rect);

                // Color con intensidad
                int r = (int)(60 + 130 * intensidad);
                int g = (int)(130 + 100 * intensidad);
                int b = (int)(220 + 35 * intensidad);
                Color color = Color.FromArgb(
                    Math.Min(255, r),
                    Math.Min(255, g),
                    Math.Min(255, b)
                );

                using (SolidBrush brush = new SolidBrush(color))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Brillo exterior (glow)
                if (intensidad > 0.1)
                {
                    double glowIntensity = intensidad * 0.9;
                    int shadowSize = (int)(8 + 28 * glowIntensity);
                    int alpha = (int)((0.2 + 0.7 * glowIntensity) * 255);
                    Color glowColor = Color.FromArgb(alpha, 80, 170, 255);

                    using (Pen pen = new Pen(glowColor, 2))
                    {
                        pen.Width = 2;
                        e.Graphics.DrawEllipse(pen, rect);
                    }
                }
            }
        }

        private void ActualizarNodos()
        {
            double progreso = Math.Min(segundosTranscurridos / TOTAL_SEGUNDOS, 1.0);
            double floatIndex = progreso * TOTAL_NODOS;

            for (int i = 0; i < panelNodos.Controls.Count; i++)
            {
                Panel nodo = panelNodos.Controls[i] as Panel;
                if (nodo == null) continue;

                double nodeStart = (double)i / TOTAL_NODOS;
                double nodeEnd = (double)(i + 1) / TOTAL_NODOS;
                double raw = 0;

                if (progreso >= nodeEnd)
                    raw = 1;
                else if (progreso > nodeStart)
                    raw = (progreso - nodeStart) / (nodeEnd - nodeStart);

                // Easing cúbico
                double eased = raw < 0.5 ? 4 * raw * raw * raw : 1 - Math.Pow(-2 * raw + 2, 3) / 2;
                double intensidad = Math.Min(eased * 1.2, 1);

                // Actualizar el nodo
                nodo.Invalidate();
                
                // Redibujar con los nuevos valores
                nodo.Paint -= (sender, e) => { };
                nodo.Paint += (sender, e) =>
                {
                    DibujarNodo(sender, e, intensidad, 0.35 + 0.65 * intensidad);
                };
                nodo.Invalidate();
            }
        }

        private void ActualizarUI()
        {
            // Actualizar tiempo
            int totalSeg = Math.Min((int)segundosTranscurridos, TOTAL_SEGUNDOS);
            int minutos = totalSeg / 60;
            int segundos = totalSeg % 60;
            labelTiempo.Text = $"{minutos:D2}:{segundos:D2}";

            // Actualizar nodos
            ActualizarNodos();

            // Actualizar botones
            botonIniciar.Enabled = !(corriendo && !pausado);
            botonPausar.Enabled = corriendo;

            // Actualizar estado
            if (corriendo && !pausado)
            {
                labelEstado.Text = "● corriendo";
                labelEstado.ForeColor = Color.FromArgb(127, 201, 255);
            }
            else if (corriendo && pausado)
            {
                labelEstado.Text = "⏸ pausado";
                labelEstado.ForeColor = Color.FromArgb(212, 163, 115);
            }
            else
            {
                labelEstado.Text = "● listo";
                labelEstado.ForeColor = Color.FromArgb(109, 123, 138);
            }

            if (segundosTranscurridos >= TOTAL_SEGUNDOS && corriendo)
            {
                labelEstado.Text = "✓ completado";
                labelEstado.ForeColor = Color.FromArgb(139, 195, 74);
            }
        }

        private void IniciarTemporizador()
        {
            if (corriendo && !pausado) return;
            if (segundosTranscurridos >= TOTAL_SEGUNDOS)
            {
                segundosTranscurridos = 0;
            }

            corriendo = true;
            pausado = false;
            tiempoInicio = DateTime.Now - TimeSpan.FromSeconds(segundosTranscurridos);

            if (temporizador == null)
            {
                temporizador = new System.Timers.Timer(INTERVALO_MS);
                temporizador.Elapsed += (s, e) =>
                {
                    TimeSpan diff = DateTime.Now - tiempoInicio;
                    segundosTranscurridos = Math.Min(diff.TotalSeconds, TOTAL_SEGUNDOS);

                    this.Invoke(new Action(() =>
                    {
                        ActualizarUI();
                        if (segundosTranscurridos >= TOTAL_SEGUNDOS)
                        {
                            DetenerTemporizador();
                        }
                    }));
                };
            }

            temporizador.Start();
            ActualizarUI();
        }

        private void PausarTemporizador()
        {
            if (!corriendo || pausado) return;
            if (temporizador != null)
                temporizador.Stop();
            pausado = true;
            ActualizarUI();
        }

        private void DetenerTemporizador()
        {
            if (temporizador != null)
                temporizador.Stop();
            corriendo = false;
            pausado = false;
            ActualizarUI();
        }

        private void ReiniciarTemporizador()
        {
            if (temporizador != null)
                temporizador.Stop();
            corriendo = false;
            pausado = false;
            segundosTranscurridos = 0;
            ActualizarUI();
            
            // Resetear nodos
            foreach (Panel nodo in panelNodos.Controls)
            {
                nodo.Paint -= (sender, e) => { };
                nodo.Paint += (sender, e) =>
                {
                    DibujarNodo(sender, e, 0, 0.35);
                };
                nodo.Invalidate();
            }
            labelEstado.Text = "● listo";
            labelEstado.ForeColor = Color.FromArgb(109, 123, 138);
            labelTiempo.Text = "05:00";
        }

        // Eventos
        private void BotonIniciar_Click(object sender, EventArgs e)
        {
            IniciarTemporizador();
        }

        private void BotonPausar_Click(object sender, EventArgs e)
        {
            PausarTemporizador();
        }

        private void BotonReiniciar_Click(object sender, EventArgs e)
        {
            ReiniciarTemporizador();
        }
    }
}