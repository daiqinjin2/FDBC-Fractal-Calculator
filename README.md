# Fractional Difference Box Counting (FDBC) Calculator

## 📝 Project Description (项目简介)
This repository provides a comprehensive toolkit for calculating the **Fractal Dimension** of concentration distribution matrices using the **Fractional Difference Box Counting (FDBC)** method. It is highly suitable for quantitative microstructural analysis in material science (e.g., elemental distribution mapping from EPMA or EDS). 

The toolkit consists of two parts:
1. A C# Windows Forms application for data preprocessing (Gaussian smoothing, global statistical extraction).
2. A MATLAB script for core FDBC calculation, linear regression, and visualization.

> **⚠️ Important Note on Data Dimension (重要提示):** > The default expected input for the concentration data matrix is **512x512**. If your matrix is rectangular or has a different resolution, please ensure you crop or interpolate it to a square matrix (preferably with side lengths that are powers of 2) before running the MATLAB core script to avoid grid division errors.
> 
> 本程序默认处理的数据矩阵尺寸为 **512x512**。如果您的原始原始数据是矩形或其他分辨率，请在导入 MATLAB 核心计算脚本前，将其裁剪或插值为正方形矩阵（边长建议为 2 的 n 次方），以避免网格划分时报错。

---

## 🛠️ Environment Requirements (环境依赖)
### For C# Preprocessing Tool (C# 预处理工具):
* **OS:** Windows 10 / 11
* **Framework:** .NET Framework 4.7.2
* **IDE:** Visual Studio 2019 / 2022

### For MATLAB Calculation Script (MATLAB 计算脚本):
* **Software:** MATLAB R2018a or newer
* **Toolboxes:** Image Processing Toolbox (required for `imgaussfilt` function)

---

## 🚀 Running Steps (运行步骤)

### Step 1: Data Preprocessing (数据预处理)
1. Run `FractalCalculator.sln` via Visual Studio.
2. Select your raw concentration CSV file (`Input CSV File`).
3. Set the smoothing parameter `Sigma` (default: 1.0) and the `Global Max` scale.
4. Click **Calculate**. The tool will output the smoothed matrix (e.g., `Result-smoothed.csv`) and statistical data.

### Step 2: FDBC Calculation & Plotting (分形维数计算与绘图)
1. Open `MATLAB_Scripts/fdbc_main.m` in MATLAB.
2. Ensure the generated smoothed CSV file is in the same directory and rename it to `Test.csv` (or modify the `readmatrix` path in the script).
3. Run the script. It will automatically:
   - Calculate the box-counting dimension $D$.
   - Plot the log-log linear regression figure (`1_Ni-DBC-ln(L)-ln(N).jpg`).
   - Export detailed box-counting step data.

---

## 📚 How to Cite (如何引用)
If you find this code helpful in your research, please cite our paper:
*(The related paper is currently under review. The formal DOI and citation format will be updated upon publication.)*

如果您在研究中使用了本代码，请引用我们的论文：
*(相关论文目前正在审稿中，正式发表后将在此处更新 DOI 与引用格式。)*
