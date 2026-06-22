# FDBC-Fractal-Calculator (分数阶差分计盒维数计算器)

![License](https://img.shields.io/badge/License-MIT-green.svg)
![.NET](https://img.shields.io/badge/.NET_Framework-4.7.2-blue.svg)
![MATLAB](https://img.shields.io/badge/MATLAB-Supported-orange.svg)

**English** | [中文简介](#中文简介)

## 📖 Project Description
This repository provides the source code for the **Fractional Difference Box Counting (FDBC)** algorithm, specifically designed to calculate the fractal dimension of 2D concentration/mass fraction matrices (e.g., from EPMA or EDS mapping in materials science). 

To accommodate different user preferences, this repository includes both a **C# GUI application** (for quick, user-friendly calculations without programming) and **MATLAB scripts** (for batch processing and advanced academic plotting).

## 🛠️ Environment Dependencies
### For C# GUI Application
* **OS:** Windows 10 / 11
* **Framework:** .NET Framework 4.7.2
* **IDE:** Visual Studio 2019 / 2022 (Required only if you want to compile the source code)

### For MATLAB Scripts
* **Software:** MATLAB R2021a or newer
* **Toolbox:** Image Processing Toolbox (Required for the `imgaussfilt` 2D Gaussian smoothing function)

## 🚀 Quick Start / Run Steps

### Option A: Using the C# GUI
1. Open `FractalCalculator.sln` with Visual Studio.
2. Compile and run the project (Press `F5`).
3. On the interface, click **"Browse"** to select your input `.csv` data matrix.
4. Set the Gaussian filter parameter `Sigma` (default is 1.0) and grid sizes.
5. Click **"Calculate"**. The results, including the linear fitting plot, smoothed matrix, and statistical properties, will be automatically saved to your selected output directory.

### Option B: Using MATLAB
1. Open the `MATLAB_Scripts` folder.
2. Ensure your input data (e.g., `Test.csv`) is placed in the same directory as the `.m` file.
3. Open `fdbc_main.m` in MATLAB and click **Run**.
4. The script will output statistical data to the console and automatically save high-resolution (600 dpi) linear fitting plots and calculation results to the current folder.

## 📂 Repository Structure
```text
FDBC-Fractal-Calculator/
 ├── CSharp_Source/        # C# WinForms source code and .sln
 ├── MATLAB_Scripts/       # MATLAB core algorithm scripts
 ├── Demo_Data/            # Sample .csv files for testing
 ├── LICENSE.txt           # MIT License
 └── README.md             # Project documentation
