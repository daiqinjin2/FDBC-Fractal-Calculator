# Fractional-Order Differential Box Counting (FDBC) Analysis Engine
# 分数阶差分计盒维数分形分析引擎

## 📝 Project Description (项目简介)
This repository provides a comprehensive and highly automated toolkit for calculating the **Fractal Dimension ($D$)** of microstructural elemental distribution maps using the advanced **Fractional-Order Differential Box Counting (FDBC)** algorithm. 

It is designed for quantitative uniformity analysis in material science, specifically targeting elemental segregation maps exported from EPMA (Electron Probe Microanalysis) or EDS (Energy Dispersive Spectroscopy). By evaluating the continuous concentration gradient (Z-axis height), it provides a rigorous mathematical characterization of homogenization processes.

本仓库提供了一套全自动、高精度的分形分析工具，利用先进的**分数阶差分计盒维数（FDBC）**算法计算微观元素分布图的**分形维数（$D$）**。
该工具专为材料科学中的定量均匀性分析而设计，完美适配从 EPMA 或 EDS 导出的元素偏析图。通过评估连续的浓度梯度（Z轴高度），为合金的均匀化过程提供严谨的数学表征。

### ✨ Key Features (核心特性升级)
* **Multi-Modal Input (多模态输入支持):** Directly imports raw data matrices (`.csv`) OR performs highly accurate Euclidean-distance color unmapping from standard RGB images (`.tiff`, `.png`, `.jpg` with automatic edge-artifact cropping).
  支持直接导入原始数据矩阵，或对标准 RGB 图像进行高精度的欧式距离色彩反向映射（并自带 JPG 边缘伪影自适应裁剪）。
* **Full-Domain Lossless Arbitrary Size (全域无损任意尺寸):** The engine uses nested fractional loops to seamlessly process rectangular matrices of any arbitrary $M \times N$ dimensions, ensuring zero data loss at the boundaries.
   引擎采用嵌套分数阶循环，完美支持任意 $M \times N$ 长方形矩阵，确保边界数据零丢失。
* **Adaptive Z-Axis Scaling (自适应Z轴物理标度):** Toggle between 'Global Z-Max' for cross-sample standardized comparison or 'Local Z-Max' for maximizing contrast of a single dataset.
  提供“全局高度基准”用于多组试样的横向对比，或“局部高度基准”以最大化提取单一数据的微弱成分起伏。
* **Intelligent Auto-Save (智能配置记忆):** Automatically memorizes grid arrays, Gaussian Sigma filters, and file paths locally.
  自动在本地静默记忆网格序列、高斯滤波 Sigma 值及文件路径，免去重复配置的烦恼。

---

## 🛠️ Environment Requirements (环境依赖)
### For Windows Desktop Engine (C# 桌面独立分析引擎):
* **OS:** Windows 10
* **Framework:** .NET Framework 4.7.2
* **IDE:** Visual Studio 2022

### For MATLAB Core Script (MATLAB 核心算法脚本 - 可选替代方案):
* **Software:** MATLAB R2023b
* **Toolboxes:** Image Processing Toolbox (required for `imgaussfilt` & `medfilt2`)

---

## 🚀 Running Steps (运行步骤)

### Method 1: Using the C# WinForms Engine (推荐：使用 C# 独立引擎)
1. Build the solution `FractalCalculator.sln` in Visual Studio 2022 to generate the standalone `.exe`.
2. Launch the Application. Choose your **Input Mode**:
   - **CSV Matrix:** Select the raw concentration data file.
   - **Image Map:** Select your cropped Data Image and the corresponding Colorbar Image.
3. Configure the `Sigma` for Gaussian noise reduction (enter 0 to skip).
4. Set the `Grid Size (L)` sequence (e.g., `2, 4, 8, 16, 32...`).
5. Choose whether to enable **Global Z-Max** for standardized scaling.
6. Click **Run FDBC Analysis**. The engine operates asynchronously to prevent UI freezing during intense color-mapping calculations. 
7. Results will be automatically saved to your chosen output directory, including:
   - High-resolution Log-Log plot (Negative Slope complying with physical definitions).
   - Detailed `.csv` statistics including Effective Range (1%-99%), Global Sigma, and fitting datapoints.

### Method 2: Using the MATLAB All-in-One Script (MATLAB 一键式脚本)
1. Open the provided `fdbc_main.m` in MATLAB.
2. Run the script. A file dialog will prompt you to select the `.csv` or `.tiff/.jpg` file.
3. If an image is selected, follow the interactive crosshair prompts to crop the pure data area and the colorbar.
4. Input the Gaussian Sigma and Global Max values in the pop-up dialog.
5. The script will automatically generate SCI-publication-ready figures and export comprehensive data tables.

---

## 📚 How to Cite (如何引用)
If you find this open-source FDBC engine or our color-unmapping methodology helpful in your research, please cite our paper:
*(The related paper concerning Cu-Ni-Sn alloy homogenization is currently under review. The formal DOI and citation format will be updated upon publication.)*

如果您在科研中使用了本开源分形分析引擎或色彩反映射方法学，请引用我们的论文：
*(关于 Cu-Ni-Sn 合金均匀化演变的相关论文目前正在审稿中，正式发表后将在此处更新 DOI 与引用格式。)*
