using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FractalCalculator
{
    /// <summary>
    /// FDBC Analysis Engine (Multi-Modal Input)
    /// FDBC 分析引擎：支持 CSV 直接导入与预裁剪图像的色彩反向映射。
    /// </summary>
    public partial class Form1 : Form
    {
        // UI Components / 界面组件
        private RadioButton rbCsvMode, rbImageMode;
        private Panel pnlCsvInput, pnlImageInput;
        private TextBox txtInputCsv, txtInputImgData, txtInputImgCbar;

        private TextBox txtOutputFile;
        private TextBox txtGridSizes;
        private TextBox txtSigma;
        private CheckBox chkUseGlobalMax;
        private TextBox txtGlobalMax;
        private Button btnCalculate;
        private Chart resultChart;

        // Configuration path / 配置文件路径
        private readonly string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fdbc_config.txt");

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            LoadConfig();
        }

        private void InitializeUI()
        {
            this.Text = "FDBC Analysis Engine (CSV & Image Support)";
            this.Size = new Size(600, 850); // Increased height for new UI / 增加高度适配新UI
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- 1. Input Mode Selection / 输入模式选择 ---
            Label lblMode = new Label { Text = "Input Mode (输入模式):", Location = new Point(20, 20), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            rbCsvMode = new RadioButton { Text = "CSV Matrix (纯数据矩阵)", Location = new Point(170, 18), Width = 180, Checked = true };
            rbImageMode = new RadioButton { Text = "Image Map (图像色彩映射)", Location = new Point(360, 18), Width = 180 };

            // Mode Toggle Event / 模式切换事件
            rbCsvMode.CheckedChanged += (s, e) => ToggleInputPanels();
            rbImageMode.CheckedChanged += (s, e) => ToggleInputPanels();

            // --- 2. CSV Input Panel / CSV 输入面板 ---
            pnlCsvInput = new Panel { Location = new Point(20, 50), Size = new Size(550, 35) };
            Label lblCsv = new Label { Text = "Data CSV:", Location = new Point(0, 8), AutoSize = true };
            txtInputCsv = new TextBox { Location = new Point(100, 5), Width = 350 };
            Button btnCsv = new Button { Text = "Browse...", Location = new Point(460, 3) };
            btnCsv.Click += (s, e) => { txtInputCsv.Text = SelectFile("Select CSV", "*.csv", false) ?? txtInputCsv.Text; };
            pnlCsvInput.Controls.AddRange(new Control[] { lblCsv, txtInputCsv, btnCsv });

            // --- 3. Image Input Panel (Hidden by default) / 图像输入面板 (默认隐藏) ---
            pnlImageInput = new Panel { Location = new Point(20, 50), Size = new Size(550, 75), Visible = false };

            Label lblImgData = new Label { Text = "Data Image:", Location = new Point(0, 8), AutoSize = true };
            txtInputImgData = new TextBox { Location = new Point(100, 5), Width = 350 };
            Button btnImgData = new Button { Text = "Browse...", Location = new Point(460, 3) };
            btnImgData.Click += (s, e) => { txtInputImgData.Text = SelectFile("Select Cropped Data Image", "*.png;*.jpg;*.jpeg;*.tiff;*.bmp", false) ?? txtInputImgData.Text; };

            Label lblImgCbar = new Label { Text = "Colorbar:", Location = new Point(0, 43), AutoSize = true };
            txtInputImgCbar = new TextBox { Location = new Point(100, 40), Width = 350 };
            Button btnImgCbar = new Button { Text = "Browse...", Location = new Point(460, 38) };
            btnImgCbar.Click += (s, e) => { txtInputImgCbar.Text = SelectFile("Select Cropped Colorbar", "*.png;*.jpg;*.jpeg;*.tiff;*.bmp", false) ?? txtInputImgCbar.Text; };

            pnlImageInput.Controls.AddRange(new Control[] { lblImgData, txtInputImgData, btnImgData, lblImgCbar, txtInputImgCbar, btnImgCbar });

            // --- 4. Common Settings / 通用设置区 ---
            // Shifted Y-coordinates downwards / 向下平移 Y 坐标以腾出空间
            int startY = 135;

            Label lblOutput = new Label { Text = "Output Dir:", Location = new Point(20, startY + 5), AutoSize = true };
            txtOutputFile = new TextBox { Location = new Point(120, startY), Width = 350 };
            Button btnOutput = new Button { Text = "Browse...", Location = new Point(480, startY - 2) };
            btnOutput.Click += (s, e) => { txtOutputFile.Text = SelectFile("Save Result", "*.csv", true) ?? txtOutputFile.Text; };

            Label lblGrid = new Label { Text = "Grid Size (L):", Location = new Point(20, startY + 45), AutoSize = true };
            txtGridSizes = new TextBox { Text = "2, 4, 8, 16, 32, 64, 128, 256", Location = new Point(120, startY + 40), Width = 350 };

            Label lblSigma = new Label { Text = "Sigma (Filter):", Location = new Point(20, startY + 85), AutoSize = true };
            txtSigma = new TextBox { Text = "1.0", Location = new Point(120, startY + 80), Width = 70 };

            chkUseGlobalMax = new CheckBox { Text = "Enable Global Z-Max (全局高度):", Location = new Point(210, startY + 83), AutoSize = true, Checked = true };
            txtGlobalMax = new TextBox { Text = "100.0", Location = new Point(430, startY + 80), Width = 60 };
            chkUseGlobalMax.CheckedChanged += (s, e) => { txtGlobalMax.Enabled = chkUseGlobalMax.Checked; };

            btnCalculate = new Button { Text = "Run FDBC Analysis", Location = new Point(410, startY + 120), Width = 150, Height = 40, BackColor = Color.LightSkyBlue };
            btnCalculate.Click += BtnCalculate_Click;

            resultChart = new Chart { Location = new Point(20, startY + 175), Size = new Size(540, 480) };
            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "ln(L)";
            chartArea.AxisY.Title = "ln(N)";
            resultChart.ChartAreas.Add(chartArea);
            resultChart.Legends.Add(new Legend("Default") { Docking = Docking.Top });

            this.Controls.AddRange(new Control[] { lblMode, rbCsvMode, rbImageMode, pnlCsvInput, pnlImageInput,
                                                   lblOutput, txtOutputFile, btnOutput, lblGrid, txtGridSizes,
                                                   lblSigma, txtSigma, chkUseGlobalMax, txtGlobalMax, btnCalculate, resultChart });
        }

        // Dynamically show/hide panels based on selection / 动态显隐面板
        private void ToggleInputPanels()
        {
            pnlCsvInput.Visible = rbCsvMode.Checked;
            pnlImageInput.Visible = rbImageMode.Checked;
        }

        private string SelectFile(string title, string filter, bool isSave)
        {
            FileDialog dlg = isSave ? (FileDialog)new SaveFileDialog() : new OpenFileDialog();
            dlg.Title = title;
            dlg.Filter = $"Files|{filter}";
            if (isSave) dlg.FileName = "FDBC_Result.csv";
            return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
        }

        private async void BtnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtOutputFile.Text)) throw new Exception("Please select an output directory.");

                // 1. 在主线程提前提取所有 UI 控件的值 (跨线程不能直接访问 UI)
                int[] sValues = txtGridSizes.Text.Split(',').Select(val => int.Parse(val.Trim())).ToArray();
                double sigma = double.Parse(txtSigma.Text);
                bool isCsvMode = rbCsvMode.Checked;
                string csvPath = txtInputCsv.Text;
                string imgDataPath = txtInputImgData.Text;
                string imgCbarPath = txtInputImgCbar.Text;
                bool useGlobalMax = chkUseGlobalMax.Checked;
                double globalMaxText = double.Parse(txtGlobalMax.Text);
                string outputDir = txtOutputFile.Text;

                // 2. 更改按钮状态，提示用户正在后台计算
                btnCalculate.Text = "Calculating... (后台计算中)";
                btnCalculate.Enabled = false; // 禁用按钮防止重复点击
                btnCalculate.BackColor = Color.LightGray;

                // 用于接收后台计算结果的变量
                double[] finalX = null;
                double[] finalY = null;
                double finalSlope = 0;
                double finalIntercept = 0;
                double finalD = 0;

                // 3. 将所有繁重的运算扔进后台线程，彻底解放 UI 防卡死
                await Task.Run(() =>
                {
                    double[,] originalMatrix;

                    if (isCsvMode)
                    {
                        if (string.IsNullOrEmpty(csvPath)) throw new Exception("Please provide a CSV file.");
                        originalMatrix = ReadCSV(csvPath);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(imgDataPath) || string.IsNullOrEmpty(imgCbarPath))
                            throw new Exception("Please provide BOTH Data Image and Colorbar Image.");

                        // 调用优化过带裁剪的映射方法
                        originalMatrix = ReadAndUnmapImage(imgDataPath, imgCbarPath);
                    }

                    int rows = originalMatrix.GetLength(0);
                    int cols = originalMatrix.GetLength(1);
                    int G = Math.Min(rows, cols);

                    double[,] smoothedImg = ApplyGaussianBlur(originalMatrix, sigma);

                    double localMax = smoothedImg.Cast<double>().Max();
                    double referenceMax = useGlobalMax ? globalMaxText : localMax;
                    if (referenceMax <= 0) referenceMax = 1.0;

                    double[,] P = new double[rows, cols];
                    for (int r = 0; r < rows; r++)
                        for (int c = 0; c < cols; c++)
                            P[r, c] = smoothedImg[r, c] * ((double)G / referenceMax);

                    finalX = new double[sValues.Length];
                    finalY = new double[sValues.Length];

                    for (int i = 0; i < sValues.Length; i++)
                    {
                        int L = sValues[i];
                        double sumNr = 0;
                        for (int r = 0; r <= rows - L; r += L)
                        {
                            for (int c = 0; c <= cols - L; c += L)
                            {
                                double maxVal = double.MinValue, minVal = double.MaxValue;
                                for (int br = 0; br < L; br++)
                                    for (int bc = 0; bc < L; bc++)
                                    {
                                        double val = P[r + br, c + bc];
                                        if (val > maxVal) maxVal = val;
                                        if (val < minVal) minVal = val;
                                    }
                                sumNr += (maxVal - minVal) / (double)L + 1.0;
                            }
                        }
                        finalX[i] = Math.Log((double)L);
                        finalY[i] = Math.Log(sumNr);
                    }

                    LinearRegression(finalX, finalY, out finalSlope, out finalIntercept);
                    finalD = Math.Abs(finalSlope);
                });

                // 4. 后台计算完毕，安全地更新图表并恢复按钮状态
                UpdateChart(finalX, finalY, finalSlope, finalIntercept, finalD);
                SaveConfig();

                MessageBox.Show($"Analysis Completed! Fractal Dimension D = {finalD:F4}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 无论成功失败，确保按钮恢复可点击状态
                btnCalculate.Text = "Run FDBC Analysis";
                btnCalculate.Enabled = true;
                btnCalculate.BackColor = Color.LightSkyBlue;
            }
        }

        #region Core Algorithms (Image Unmapping & Math) / 核心算法 (色彩反映射与数学)

        /// <summary>
        /// Reads RGB image, crops edges for JPGs to avoid compression artifacts, and maps to Z-matrix.
        /// 读取RGB图像，针对JPG格式自动裁剪边缘以避免压缩伪影，并映射为 Z 矩阵。
        /// </summary>
        private double[,] ReadAndUnmapImage(string dataImagePath, string cbarImagePath)
        {
            using (Bitmap bmpData = new Bitmap(dataImagePath))
            using (Bitmap bmpCbar = new Bitmap(cbarImagePath))
            {
                int originalRows = bmpData.Height;
                int originalCols = bmpData.Width;

                // 针对 JPG 格式自动裁剪边缘伪影
                int margin = 0;
                string ext = Path.GetExtension(dataImagePath).ToLower();
                if (ext == ".jpg" || ext == ".jpeg")
                {
                    margin = 3; // 自动裁掉边缘最外圈的 3 个像素
                }

                int rows = originalRows - 2 * margin;
                int cols = originalCols - 2 * margin;

                if (rows <= 0 || cols <= 0)
                    throw new Exception("Image is too small to crop edges. / 图像太小，无法执行边缘裁剪。");

                double[,] zMatrix = new double[rows, cols];

                // 提取调色板
                int numColors = bmpCbar.Height;
                int centerCol = bmpCbar.Width / 2;
                Color[] palette = new Color[numColors];
                for (int y = 0; y < numColors; y++)
                {
                    palette[y] = bmpCbar.GetPixel(centerCol, y);
                }

                // 开始像素映射
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        // 加上 margin 偏移量，跳过边缘的污染像素
                        Color pixel = bmpData.GetPixel(c + margin, r + margin);
                        int bestIdx = 0;
                        double minDist = double.MaxValue;

                        for (int i = 0; i < numColors; i++)
                        {
                            Color palColor = palette[i];

                            // 用直接相乘，大幅缓解算力卡顿
                            double dist = (pixel.R - palColor.R) * (pixel.R - palColor.R) +
                                          (pixel.G - palColor.G) * (pixel.G - palColor.G) +
                                          (pixel.B - palColor.B) * (pixel.B - palColor.B);

                            if (dist < minDist)
                            {
                                minDist = dist;
                                bestIdx = i;
                            }
                        }
                        zMatrix[r, c] = numColors - bestIdx;
                    }
                }
                return zMatrix;
            }
        }

        private double[,] ApplyGaussianBlur(double[,] input, double sigma)
        {
            if (sigma <= 0) return input;
            int radius = (int)Math.Ceiling(sigma * 3);
            int size = radius * 2 + 1;
            double[,] kernel = new double[size, size];
            double sum = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    double val = Math.Exp(-(i * i + j * j) / (2 * sigma * sigma));
                    kernel[i + radius, j + radius] = val;
                    sum += val;
                }
            for (int i = 0; i < size; i++) for (int j = 0; j < size; j++) kernel[i, j] /= sum;

            int rows = input.GetLength(0), cols = input.GetLength(1);
            double[,] output = new double[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    double pixelSum = 0;
                    for (int i = -radius; i <= radius; i++)
                        for (int j = -radius; j <= radius; j++)
                        {
                            int row = Math.Max(0, Math.Min(rows - 1, r + i));
                            int col = Math.Max(0, Math.Min(cols - 1, c + j));
                            pixelSum += input[row, col] * kernel[i + radius, j + radius];
                        }
                    output[r, c] = pixelSum;
                }
            return output;
        }

        private double[,] ReadCSV(string path)
        {
            var lines = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            int rows = lines.Length, cols = lines[0].Split(',').Length;
            double[,] matrix = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                var values = lines[i].Split(',');
                for (int j = 0; j < cols; j++) if (double.TryParse(values[j], out double val)) matrix[i, j] = val;
            }
            return matrix;
        }

        private void LinearRegression(double[] x, double[] y, out double slope, out double intercept)
        {
            int n = x.Length;
            double sumX = x.Sum(), sumY = y.Sum(), sumXY = 0, sumX2 = 0;
            for (int i = 0; i < n; i++) { sumXY += x[i] * y[i]; sumX2 += x[i] * x[i]; }
            slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            intercept = (sumY - slope * sumX) / n;
        }

        #endregion

        #region Charting & Configuration / 图表更新与参数记忆
        private void UpdateChart(double[] x, double[] y, double slope, double intercept, double D)
        {
            resultChart.Series.Clear();
            Series scatter = new Series("Data") { ChartType = SeriesChartType.Point, MarkerSize = 9, Color = Color.Blue };
            Series line = new Series("DBC Fit") { ChartType = SeriesChartType.Line, BorderDashStyle = ChartDashStyle.Dash, BorderWidth = 2, Color = Color.Red };
            for (int i = 0; i < x.Length; i++) { scatter.Points.AddXY(x[i], y[i]); line.Points.AddXY(x[i], slope * x[i] + intercept); }
            resultChart.Series.Add(scatter); resultChart.Series.Add(line);
            resultChart.Titles.Clear();
            resultChart.Titles.Add(new Title($"D = {D:F4} (Slope = {slope:F4})", Docking.Top, new Font("Arial", 16, FontStyle.Bold), Color.Black));
            resultChart.ChartAreas[0].RecalculateAxesScale();
        }

        private void SaveConfig()
        {
            try
            {
                File.WriteAllLines(configFilePath, new string[] {
                    rbCsvMode.Checked.ToString(), txtInputCsv.Text, txtInputImgData.Text, txtInputImgCbar.Text,
                    txtGridSizes.Text, txtSigma.Text, chkUseGlobalMax.Checked.ToString(), txtGlobalMax.Text, txtOutputFile.Text
                });
            }
            catch { }
        }

        private void LoadConfig()
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(configFilePath);
                    if (lines.Length >= 9)
                    {
                        if (bool.Parse(lines[0])) rbCsvMode.Checked = true; else rbImageMode.Checked = true;
                        txtInputCsv.Text = lines[1]; txtInputImgData.Text = lines[2]; txtInputImgCbar.Text = lines[3];
                        txtGridSizes.Text = lines[4]; txtSigma.Text = lines[5];
                        chkUseGlobalMax.Checked = bool.Parse(lines[6]); txtGlobalMax.Text = lines[7]; txtOutputFile.Text = lines[8];
                    }
                }
                catch { }
            }
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e) { }
    }
}