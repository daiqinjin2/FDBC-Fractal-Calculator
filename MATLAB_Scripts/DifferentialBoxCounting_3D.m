clear;

% Read 2D matrix directly from CSV file (直接从CSV文件读取二维矩阵)
img = readmatrix('Test.csv'); 

% In the solid solution state, macroscopic segregation may have smoothed out. 
% The statistical fluctuation (shot noise) of the EPMA probe can form many pseudo-rugged "zigzags".
% (可能固溶态下宏观偏析已经抹平，EPMA探针的统计涨落(Shot noise)会形成大量伪崎岖的"锯齿"。)
% 
% The sigma value controls the smoothing radius. Recommended range is between 0.5 and 1.5.
% (sigma 值控制平滑半径。建议取 0.5 到 1.5 之间。)
%
% A larger value means stronger denoising and a smoother surface, but if set too large, 
% it will also smooth out the true concentration gradient.
% (值越大，去噪越强，表面越平滑；但设置过大也会把真实的浓度梯度抹平。)
sigma = 1.0; 
P_smoothed = imgaussfilt(img, sigma);

% W = size(img,1)/3; % If the original matrix is a 4:3 rectangle, merge 4*3 grids into a square 
% (如果原矩阵为4:3的矩形，现在4*3个格子合并成正方形)
W = size(P_smoothed, 1);
M = W; % Default image size is square, e.g., 512 (图像大小默认长宽一样)
Global_Max = 100;

% Flatten the 2D matrix into a 1D vector for statistical analysis (将二维矩阵展开为一维向量，方便统计)
conc_vector = P_smoothed(:);

% Statistical parameters (统计学参数)
avg_conc = mean(conc_vector);       % Average concentration / 平均浓度 (wt.%)
std_dev  = std(conc_vector);        % Standard deviation Sigma / 标准差 (wt.%)

% Introduce 1% and 99% percentiles to mask extreme outlier noise (引入 1% 和 99% 分位数，屏蔽极端异常噪点)
eff_max = prctile(conc_vector, 99);  % Concentration at 99% percentile (99% 分位数的浓度)
eff_min = prctile(conc_vector, 1);   % Concentration at 1% percentile (1% 分位数的浓度)
eff_range = eff_max - eff_min;       % Effective range of the core matrix (核心基体的有效极差)

% Print statistics to console (输出到控制台)
fprintf('--- Statistical Analysis of Concentration Distribution ---\n');
fprintf('Average Concentration: %.4f wt.%%\n', avg_conc);
fprintf('Global Std Dev (Sigma): %.4f wt.%%\n', std_dev);
fprintf('Effective Range (1%%-99%%): %.4f wt.%%\n', eff_range);
fprintf('------------------------------------------------------\n');

% Forcibly map the concentration matrix P into a unified 3D geometric space 
% (将浓度矩阵 P 强制映射到统一的三维几何空间中)
P = P_smoothed * (M / Global_Max);

G = W;          % Matrix size (矩阵尺寸)
s = 2.^[1:8]; 
M = size(P, 1); % Default image size is square (图像大小默认长宽一样)
h = s;          % Grid height (网格高度)
Grid_num = M ./ s; % Number of grids per row (每行网格数)
Nr = zeros(1, length(s));

% Optimization: Use mat2cell to divide the image matrix into a cell array, corresponding to grid division.
% (优化：使用mat2cell把图像矩阵划分为元胞数组，这一过程对应网格的划分)
% Then use cellfun to process each cell directly, improving efficiency and reducing nested for-loops.
% (然后使用cellfun直接对每个元胞数组进行处理提升效率，减少for循环层数)
% Use continuous relative height calculation to eliminate boundary quantization errors.
% (使用连续相对高度计算，消除边界量子化误差)
for j = 1:length(s)
    L = s(j) * ones(1, Grid_num(j));  
    Nr(j) = sum(sum(cellfun(@(x) (max(x(:)) - min(x(:))) / h(j) + 1.0, mat2cell(P, L, L))));
end

y = log(Nr);
x = log(G ./ s);

% Linear regression (线性拟合)
p = polyfit(x, y, 1);
FD = abs(p(1)); % Fractal Dimension (分形维数)
logp = polyval(p, x);

% String for D value (k与维数D的字符串)
txt = {['D = ', num2str(FD, 6)]}; 

% Scatter plot of log box count vs log grid size (盒子计数对数与格子数的数据散点作图)
scatter(x, y, 25, 'filled'); 
hold on;

% Plot fitted line (拟合线作图)
plot(x, logp, 'r--', 'LineWidth', 1.5); 

% Legend labels (标签)
legend('Data Points', 'Fitted Line (Box-Counting)', 'Location', 'northwest'); 
hold off;

% Axes settings (X轴和Y轴设置)
xlabel('ln(L)', 'FontName', 'Times New Roman', 'FontSize', 20, 'FontWeight', 'bold', 'LineWidth', 1.5); 
ylabel('ln(N)', 'FontName', 'Times New Roman', 'FontSize', 20, 'FontWeight', 'bold', 'LineWidth', 1.5); 

% Title setting (图片标题设置)
title('Log-Log Plot of Grid Size L vs Box Count N', 'FontName', 'Arial', 'FontSize', 20, 'FontWeight', 'bold'); 

% Add text to plot (字符串写在坐标图上)
text(x(4), logp(4)-1, txt, 'Color', 'red', 'FontName', 'Times New Roman', 'FontSize', 14); 

set(gca, 'LineWidth', 1.5);
set(gca, 'FontSize', 16);
box on;

% Export image (输出图片)
exportgraphics(gca, 'Test-ln(L)-ln(N).jpg', 'Resolution', 600); 

% Export standalone CSV files (导出为独立的 CSV 文件)
writematrix([x' y'], 'Test-logL-logN.csv');
writematrix(P_smoothed, 'Test_smoothed.csv');

stats_table = table(avg_conc, std_dev, eff_max, eff_min, eff_range, ...
    'VariableNames', {'Average_wt', 'Std_Dev_wt', 'eff_Max_wt', 'eff_Min_wt', 'eff_Range_wt'});
writetable(stats_table, 'Test_Statistics.csv');

% Reference:
% N. Sarkar and B. B. Chaudhuri, "An efficient differential box-counting approach to compute fractal dimension of image," in IEEE Transactions on Systems, Man, and Cybernetics, vol. 24, no. 1, pp. 115-120, Jan. 1994.