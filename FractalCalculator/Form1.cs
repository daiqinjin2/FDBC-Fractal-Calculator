using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FractalCalculator
{
    public partial class Form1 : Form
    {
        private TextBox txtInputFile;
        private TextBox txtOutputFile;
        private TextBox txtGridSizes;
        private TextBox txtSigma;
        private TextBox txtGlobalMax;
        private Button btnCalculate;
        private Chart resultChart;

        public Form1()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Fractional Difference Box Counting (FDBC) Calculator";
            this.Size = new Size(600, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Select input file (选择输入文件)
            Label lblInput = new Label { Text = "Input CSV File:", Location = new Point(20, 25), AutoSize = true };
            txtInputFile = new TextBox { Location = new Point(120, 20), Width = 350 };
            Button btnInput = new Button { Text = "Browse...", Location = new Point(480, 18) };
            btnInput.Click += (s, e) => { txtInputFile.Text = SelectFile("Select CSV File", "*.csv", false) ?? txtInputFile.Text; };

            // Select output directory (选择输出目录)
            Label lblOutput = new Label { Text = "Output:", Location = new Point(20, 65), AutoSize = true };
            txtOutputFile = new TextBox { Location = new Point(120, 60), Width = 350 };
            Button btnOutput = new Button { Text = "Browse...", Location = new Point(480, 58) };
            btnOutput.Click += (s, e) => { txtOutputFile.Text = SelectFile("Save Result", "*.csv", true) ?? txtOutputFile.Text; };

            // Grid parameters (网格参数)
            Label lblGrid = new Label { Text = "Grid Size (L):", Location = new Point(20, 105), AutoSize = true };
            txtGridSizes = new TextBox { Text = "2, 4, 8, 16, 32, 64, 128, 256", Location = new Point(120, 100), Width = 350 };

            // Sigma for Gaussian filter (高斯滤波 Sigma 参数)
            Label lblSigma = new Label { Text = "Sigma (Filter):", Location = new Point(20, 145), AutoSize = true };
            txtSigma = new TextBox { Text = "1.0", Location = new Point(120, 140), Width = 80 };

            // Global maximum scale for unified Z-axis (全局最大值标度，用于统一Z轴)
            Label lblMax = new Label { Text = "Global Max (wt.%):", Location = new Point(220, 145), AutoSize = true };
            txtGlobalMax = new TextBox { Text = "100.0", Location = new Point(340, 140), Width = 80 };

            // Execution button (执行按钮)
            btnCalculate = new Button { Text = "Calculate", Location = new Point(120, 180), Width = 150, Height = 40, BackColor = Color.LightGreen };
            btnCalculate.Click += BtnCalculate_Click;

            // Chart control settings (图表控件设置)
            resultChart = new Chart { Location = new Point(20, 240), Size = new Size(540, 480) };
            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "ln(L)";
            chartArea.AxisX.TitleFont = new Font("Times New Roman", 14, FontStyle.Bold);
            chartArea.AxisY.Title = "ln(N)";
            chartArea.AxisY.TitleFont = new Font("Times New Roman", 14, FontStyle.Bold);
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            resultChart.ChartAreas.Add(chartArea);
            resultChart.Legends.Add(new Legend("Default") { Docking = Docking.Top });

            this.Controls.AddRange(new Control[] { lblInput, txtInputFile, btnInput, lblOutput, txtOutputFile, btnOutput, lblGrid, txtGridSizes, lblSigma, txtSigma, lblMax, txtGlobalMax, btnCalculate, resultChart });
        }

        private string SelectFile(string title, string filter, bool isSave)
        {
            FileDialog dlg = isSave ? (FileDialog)new SaveFileDialog() : new OpenFileDialog();
            dlg.Title = title;
            dlg.Filter = $"CSV File|{filter}";
            if (isSave) dlg.FileName = "Result-logL-logN.csv";
            return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtInputFile.Text) || string.IsNullOrEmpty(txtOutputFile.Text))
                    throw new Exception("Please ensure both input and output paths are filled.");

                int[] sValues = txtGridSizes.Text.Split(',').Select(val => int.Parse(val.Trim())).ToArray();
                double sigma = double.Parse(txtSigma.Text);
                double globalMax = double.Parse(txtGlobalMax.Text);

                // Read original matrix (读取原矩阵)
                double[,] originalImg = ReadCSV(txtInputFile.Text);
                int M = originalImg.GetLength(0);
                int G = M;

                // Apply Gaussian blur for noise reduction (应用高斯平滑滤波去噪)
                double[,] smoothedImg = ApplyGaussianBlur(originalImg, sigma);

                // Calculate global statistical features of concentration (计算浓度的全域统计学特征)
                double sum = 0, sumSq = 0;
                int totalPixels = M * M;
                for (int r = 0; r < M; r++)
                {
                    for (int c = 0; c < M; c++)
                    {
                        double val = smoothedImg[r, c];
                        sum += val;
                    }
                }

                double avgConc = sum / totalPixels;

                // Calculate effective range (有效极差计算)
                double[] flatArray = new double[totalPixels];
                int index = 0;
                for (int r = 0; r < M; r++)
                {
                    for (int c = 0; c < M; c++)
                    {
                        flatArray[index++] = smoothedImg[r, c];
                    }
                }

                // Sort all concentration points (对全部浓度点进行排序)
                Array.Sort(flatArray);

                // Extract concentration values at 1% and 99% positions to mask extreme noise (提取 1% 和 99% 位置的浓度值，屏蔽首尾的极端噪点)
                double effMinConc = flatArray[(int)(totalPixels * 0.01)];
                double effMaxConc = flatArray[(int)(totalPixels * 0.99)];
                double effRangeConc = effMaxConc - effMinConc;

                // Calculate variance and standard deviation (计算方差和标准差)
                for (int r = 0; r < M; r++)
                {
                    for (int c = 0; c < M; c++)
                    {
                        double val = smoothedImg[r, c];
                        sumSq += (val - avgConc) * (val - avgConc);
                    }
                }
                double stdDev = Math.Sqrt(sumSq / totalPixels);

                // Export independent statistics to a CSV file (导出单独的统计学 CSV 文件)
                string statsFilePath = Path.Combine(Path.GetDirectoryName(txtOutputFile.Text),
                                         Path.GetFileNameWithoutExtension(txtOutputFile.Text) + "-Statistics.csv");
                using (StreamWriter swStats = new StreamWriter(statsFilePath))
                {
                    swStats.WriteLine("Average_wt,Std_Dev_Sigma,effMax_wt,effMin_wt,effRange_wt");
                    swStats.WriteLine($"{avgConc:F4},{stdDev:F4},{effMaxConc:F4},{effMinConc:F4},{effRangeConc:F4}");
                }

                // Export smoothed mass fraction matrix (导出平滑后的质量百分数矩阵)
                string smoothedFilePath = Path.Combine(Path.GetDirectoryName(txtOutputFile.Text),
                                         Path.GetFileNameWithoutExtension(txtOutputFile.Text) + "-smoothed.csv");
                ExportMatrixToCSV(smoothedImg, smoothedFilePath);

                // Map mass fraction to unified pixel height / Z-axis scale (将质量分数映射为统一像素高度，即Z轴标度)
                double[,] P = new double[M, M];
                for (int r = 0; r < M; r++)
                {
                    for (int c = 0; c < M; c++)
                    {
                        P[r, c] = smoothedImg[r, c] * (M / globalMax);
                    }
                }

                double[] x = new double[sValues.Length];
                double[] y = new double[sValues.Length];

                // Core computation of DBC fractal dimension based on processed P matrix (基于处理后的P矩阵的分形维数核心计算)
                for (int i = 0; i < sValues.Length; i++)
                {
                    int L = sValues[i];
                    int gridNum = M / L;
                    double sumNr = 0;

                    for (int r = 0; r < gridNum; r++)
                    {
                        for (int c = 0; c < gridNum; c++)
                        {
                            double maxVal = double.MinValue;
                            double minVal = double.MaxValue;

                            for (int br = 0; br < L; br++)
                            {
                                for (int bc = 0; bc < L; bc++)
                                {
                                    double val = P[r * L + br, c * L + bc];
                                    if (val > maxVal) maxVal = val;
                                    if (val < minVal) minVal = val;
                                }
                            }
                            // Fractional Difference Box Counting, FDBC (分数阶差分计盒法)
                            sumNr += (maxVal - minVal) / (double)L + 1.0;
                        }
                    }
                    x[i] = Math.Log((double)L);
                    y[i] = Math.Log(sumNr);
                }

                LinearRegression(x, y, out double slope, out double intercept);
                double D = Math.Abs(slope);

                using (StreamWriter sw = new StreamWriter(txtOutputFile.Text))
                {
                    sw.WriteLine("ln(L),ln(N)");
                    for (int i = 0; i < x.Length; i++)
                        sw.WriteLine($"{x[i]:F6},{y[i]:F6}");
                }

                UpdateChart(x, y, slope, intercept, D);

                string imgPath = Path.Combine(Path.GetDirectoryName(txtOutputFile.Text),
                                 Path.GetFileNameWithoutExtension(txtOutputFile.Text) + "-plot.jpg");
                resultChart.SaveImage(imgPath, ChartImageFormat.Jpeg);

                MessageBox.Show($"Calculation completed!\nFractal Dimension D = {D:F6}\nSmoothed matrix and statistics have been saved to the target directory.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Runtime Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 2D Gaussian blur algorithm (二维高斯滤波算法)
        private double[,] ApplyGaussianBlur(double[,] input, double sigma)
        {
            if (sigma <= 0) return input; // Skip filtering if sigma is 0 (如果 sigma 为 0 则不滤波)

            int radius = (int)Math.Ceiling(sigma * 3);
            int size = radius * 2 + 1;
            double[,] kernel = new double[size, size];
            double sum = 0;

            // Generate Gaussian kernel (生成高斯核)
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    double val = Math.Exp(-(i * i + j * j) / (2 * sigma * sigma));
                    kernel[i + radius, j + radius] = val;
                    sum += val;
                }
            }

            // Normalize Gaussian kernel (归一化高斯核)
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= sum;
                }
            }

            int rows = input.GetLength(0);
            int cols = input.GetLength(1);
            double[,] output = new double[rows, cols];

            // Execute convolution with edge clamping to prevent out-of-bounds (执行卷积，利用边缘钳位防止越界)
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double pixelSum = 0;
                    for (int i = -radius; i <= radius; i++)
                    {
                        for (int j = -radius; j <= radius; j++)
                        {
                            int row = r + i;
                            int col = c + j;

                            if (row < 0) row = 0;
                            if (row >= rows) row = rows - 1;
                            if (col < 0) col = 0;
                            if (col >= cols) col = cols - 1;

                            pixelSum += input[row, col] * kernel[i + radius, j + radius];
                        }
                    }
                    output[r, c] = pixelSum;
                }
            }
            return output;
        }

        // Export smoothed CSV matrix (导出平滑后的CSV矩阵)
        private void ExportMatrixToCSV(double[,] matrix, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                for (int i = 0; i < rows; i++)
                {
                    string[] line = new string[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        line[j] = matrix[i, j].ToString("F4"); // Keep 4 decimal places (保留4位小数)
                    }
                    sw.WriteLine(string.Join(",", line));
                }
            }
        }

        private double[,] ReadCSV(string path)
        {
            var lines = File.ReadAllLines(path);
            int rows = lines.Length;
            int cols = lines[0].Split(',').Length;
            double[,] matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                var values = lines[i].Split(',');
                for (int j = 0; j < cols; j++)
                {
                    if (double.TryParse(values[j], out double val))
                        matrix[i, j] = val;
                }
            }
            return matrix;
        }

        private void LinearRegression(double[] x, double[] y, out double slope, out double intercept)
        {
            int n = x.Length;
            double sumX = x.Sum(), sumY = y.Sum();
            double sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                sumXY += x[i] * y[i];
                sumX2 += x[i] * x[i];
            }

            slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            intercept = (sumY - slope * sumX) / n;
        }

        private void UpdateChart(double[] x, double[] y, double slope, double intercept, double D)
        {
            resultChart.Series.Clear();

            Series scatter = new Series("Data Points") { ChartType = SeriesChartType.Point, MarkerStyle = MarkerStyle.Circle, MarkerSize = 8, Color = Color.Blue };
            Series line = new Series("Fitted Line (Box-Counting)") { ChartType = SeriesChartType.Line, BorderDashStyle = ChartDashStyle.Dash, BorderWidth = 2, Color = Color.Red };

            for (int i = 0; i < x.Length; i++)
            {
                scatter.Points.AddXY(x[i], y[i]);
                line.Points.AddXY(x[i], slope * x[i] + intercept);
            }

            resultChart.Series.Add(scatter);
            resultChart.Series.Add(line);
            resultChart.Titles.Clear();
            resultChart.Titles.Add(new Title($"Log-Log Plot of Grid Size L vs Box Count N (D={D:F6})", Docking.Top, new Font("Arial", 16, FontStyle.Bold), Color.Black));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}