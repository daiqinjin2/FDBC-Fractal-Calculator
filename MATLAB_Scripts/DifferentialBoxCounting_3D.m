% =========================================================================
% Full-Domain Lossless Version: Adaptive FDBC Fractal & Statistical Analysis
% EPMA 浓度矩阵/图像自适应 FDBC 分形与统计分析脚本
% 
% Features (特性): 
% 1. Supports arbitrary M x N rectangular matrices (支持任意 M x N 长方形矩阵)
% 2. Interactive settings for Gaussian smoothing & Global Z-Max (交互式高斯平滑与全局高度参数设置)
% 3. Unified nested loops to prevent data loss (统一嵌套循环防数据丢失)
% 4. Log-Log plot with negative slope matching physical definitions (物理定义的负斜率双对数图)
% =========================================================================

clc; clear; close all;

%% 1. Interactive File Selection / 交互式文件选择
[fileName, filePath] = uigetfile({'*.csv;*.tiff;*.tif;*.png;*.jpg;*.jpeg', 'Supported Files (*.csv, *.tiff, *.png, *.jpg)'}, ...
    'Select Data (CSV) or Image File / 请选择数据或图像文件');

if isequal(fileName, 0)
    disp('Operation canceled by user. / 用户取消了操作。');
    return;
end

fullFilePath = fullfile(filePath, fileName);
[~, baseName, ext] = fileparts(fileName);
ext = lower(ext);

disp(['Processing file / 正在处理文件: ', fileName]);

%% 2. Adaptive Data Reading & Color Unmapping / 自适应数据读取与颜色反向映射
if strcmp(ext, '.csv')
    % --- Process CSV Data / 处理 CSV 数据 ---
    disp('CSV file detected, reading matrix... / 检测到 CSV 文件，直接读取浓度矩阵...');
    Z_raw = readmatrix(fullFilePath);
    
else
    % --- Process Image Data / 处理图像数据 ---
    disp('Image detected, starting color unmapping... / 检测到图像文件，启动色彩映射流程...');
    I = imread(fullFilePath);
    if size(I, 3) == 4, I = I(:, :, 1:3); end % Remove Alpha channel / 丢弃透明度通道
    
    % Interactive cropping / 交互式裁剪
    figure('Name', 'Remove UI Borders / 去除界面白边');
    imshow(I); 
    title('Select [Pure Data Area], double-click to confirm / 请框选【纯数据区域】，双击确认'); 
    data_rgb = imcrop;
    
    title('Select [Colorbar Area], double-click to confirm / 请框选【Colorbar渐变色条】，双击确认'); 
    cbar_rgb = imcrop;
    close;
    
    % Emergency denoising for JPG / 针对 JPG 的抢救性去噪
    if strcmp(ext, '.jpg') || strcmp(ext, '.jpeg')
        disp(' JPG Format: Median filtering to remove artifacts... / JPG 格式：启动中值滤波抹平压缩伪影...');
        for c=1:3
            data_rgb(:,:,c) = medfilt2(data_rgb(:,:,c), [3 3]);
            cbar_rgb(:,:,c) = medfilt2(cbar_rgb(:,:,c), [3 3]);
        end
    end
    
    % Palette extraction / 提取基准调色板
    center_col = round(size(cbar_rgb, 2) / 2);
    palette = double(squeeze(cbar_rgb(:, center_col, :)));
    num_colors = size(palette, 1);
    
    % Euclidean distance matching / 欧式距离颜色匹配
    data_flat = double(reshape(data_rgb, [], 3));
    mapped_idx = zeros(size(data_flat, 1), 1);
    min_dist = inf(size(data_flat, 1), 1);
    for j = 1:num_colors
        dist = sum((data_flat - palette(j, :)).^2, 2);
        update_mask = dist < min_dist;
        min_dist(update_mask) = dist(update_mask);
        mapped_idx(update_mask) = j;
    end
    Z_matrix = num_colors - mapped_idx + 1; % Reverse idx for concentration / 反转索引对应浓度
    Z_raw = reshape(Z_matrix, size(data_rgb, 1), size(data_rgb, 2));
end

%% 3. Settings (Smoothing & Global Z-Max) / 参数设置 (高斯平滑与全局高度)
prompt = {
    '1. Enter Sigma (0 to skip) / 高斯平滑Sigma值 (0跳过):',
    '2. Enable Global Z-Max? (1=Yes, 0=No) / 启用全局高度标度? (1=是, 0=否):',
    '3. Global Max Value / 全局最大高度值 (如果上一项填1):'
};
dlgtitle = 'Analysis Settings / 分析参数设置';
dims = [1 65; 1 65; 1 65];
definput = {'1.0', '1', '100.0'}; % Default values / 默认值

answer = inputdlg(prompt, dlgtitle, dims, definput);
if isempty(answer)
    disp('Operation canceled. / 操作取消。'); return;
end

sigma = str2double(answer{1});
useGlobalMax = str2double(answer{2}) == 1;
globalMaxValue = str2double(answer{3});

% Apply Gaussian Smoothing / 执行高斯平滑
if sigma > 0
    disp(['Executing Gaussian smoothing / 执行高斯平滑, Sigma = ', num2str(sigma)]);
    P_smoothed = imgaussfilt(Z_raw, sigma);
else
    disp('Skipped Gaussian smoothing. / 跳过高斯平滑。');
    P_smoothed = double(Z_raw);
end

%% 4. Statistical Analysis / 全域浓度分布统计学分析
conc_vector = P_smoothed(:);
localMax = max(conc_vector);         % Local maximum of the matrix / 当前矩阵的局部最大值
avg_conc = mean(conc_vector);       
std_dev  = std(conc_vector);        
eff_max = prctile(conc_vector, 99);  
eff_min = prctile(conc_vector, 1);   
eff_range = eff_max - eff_min;       

% Determine Reference Z-Max / 确定 Z 轴物理缩放的基准高度
if useGlobalMax
    referenceMax = globalMaxValue;
else
    referenceMax = localMax;
end

if referenceMax <= 0
    referenceMax = 1.0; % Prevent division by zero / 防止除以零
end

fprintf('\n--- Global Concentration Statistics / 全域浓度统计分析 ---\n');
fprintf('Average (平均浓度): %.4f\n', avg_conc);
fprintf('Std Deviation (全域标准差 Sigma): %.4f\n', std_dev);
fprintf('Effective Range 1%%-99%% (有效极差): %.4f\n', eff_range);
fprintf('Local Max (局部最大值): %.4f\n', localMax);
fprintf('Z-Scale Reference (采用的高度基准): %.4f\n', referenceMax);
fprintf('------------------------------------------------------\n');

%% 5. 3D Geometric Space Mapping / 统一映射三维几何空间
[rows, cols] = size(P_smoothed);
disp(['Matrix size / 矩阵尺寸: ', num2str(rows), ' x ', num2str(cols)]);

% Use the shortest edge as global reference G / 采用最短边作为全局几何参考尺度 G
G = min(rows, cols); 

% Scale concentration height to XY physical scale / 将浓度高度缩放到与 XY 网格同等尺度
P = P_smoothed * (G / referenceMax);

%% 6. Fractional-Order DBC Calculation / 分数阶差分计盒维数计算
max_power = floor(log2(G));
s = 2.^(1:(max_power-1)); % Grid size sequence / 网格尺寸序列
Nr = zeros(1, length(s));

disp('Calculating FDBC via nested loops... / 启动嵌套循环计算 FDBC...');
for k = 1:length(s)
    r = s(k);
    h = r; % Z-axis height proportional to grid size / Z轴高度对应网格尺寸
    box_count = 0;
    
    % Traverse the full image / 遍历整个全域长方形图像
    for i = 1:r:(rows - r + 1)
        for j = 1:r:(cols - r + 1)
            sub_Z = P(i:i+r-1, j:j+r-1);
            
            % Fractional exact calculation / 分数阶精确计算无取整误差
            n = (max(sub_Z(:)) - min(sub_Z(:))) / h + 1.0;
            box_count = box_count + n;
        end
    end
    Nr(k) = box_count;
end

%% 7. Log-Log Fitting (Negative Slope) / 双对数换轴与拟合 (负斜率)
x = log(s);      % X-axis: ln(L), Log of actual grid size / X轴：网格边长对数
y = log(Nr);     % Y-axis: ln(N), Log of box count / Y轴：盒子总数对数

% Linear fit: y = -D * x + C / 线性拟合
p = polyfit(x, y, 1);
slope = p(1);    % Slope is negative / 拟合斜率为负数
FD = -slope;     % Fractal Dimension is the absolute slope / 分形维数为斜率绝对值

logp = polyval(p, x);

fprintf('\n✅ FD Calculation Completed / 分形维数计算完成:\n');
fprintf('   Slope (拟合斜率) = %.6f\n', slope);
fprintf('   Fractal Dimension D (分形维数) = %.6f\n', FD);

%% 8. SCI-Level Plotting / 标准画图
figure('Name', ['FDBC Analysis - ', baseName], 'Color', 'w', 'Position', [100, 100, 700, 550]);
scatter(x, y, 50, 'filled', 'MarkerFaceColor', '#0072BD');
hold on;
plot(x, logp, 'r--', 'LineWidth', 2);

% Bilingual Legend / 双语图例 (主英文适合SCI)
legend('Experimental Data', 'DBC Linear Fit', 'Location', 'northeast', 'FontSize', 14);

% Axes & Title / 坐标轴与标题
xlabel('ln(\it{L}\rm)', 'FontName', 'Times New Roman', 'FontSize', 20, 'FontWeight', 'bold');
ylabel('ln(\it{N}\rm)', 'FontName', 'Times New Roman', 'FontSize', 20, 'FontWeight', 'bold');
title('Log-Log Plot of Grid Length \it{L} \rm vs. Box Count \it{N}', ...
      'FontName', 'Times New Roman', 'FontSize', 18, 'FontWeight', 'bold');

% Add Equation & D-value text / 添加拟合公式与D值文字
txt = {sprintf('Slope = %.4f', slope), sprintf('D = %.4f', FD)};
text_x = x(round(length(x)/2)) + 0.6; 
text_y = logp(round(length(x)/2)) + 0.6; % Slightly shifted up / 略微上移防重叠
text(text_x, text_y, txt, 'Color', 'red', 'FontName', 'Times New Roman', 'FontSize', 16, 'FontWeight', 'bold');

set(gca, 'LineWidth', 1.5, 'FontSize', 16, 'FontName', 'Times New Roman');
box on; grid on; hold off;

%% 9. Batch Data Export / 全域数据与结果导出
disp('Exporting results... / 正在导出全域数据文件...');

% 1. High-Res Plot / 高清图片
exportgraphics(gca, sprintf('%s_FDBC_Plot.jpg', baseName), 'Resolution', 600);

% 2. Fitting Data Points / 拟合数据点
writematrix([x' y'], sprintf('%s_FDBC_DataPoints.csv', baseName));

% 3. Full Smoothed Matrix / 提取的全域平滑矩阵
writematrix(P_smoothed, sprintf('%s_P_smoothed_Full.csv', baseName));

% 4. Statistical Data / 统计学数据表 (记录采用的基准高度)
stats_table = table(avg_conc, std_dev, eff_max, eff_min, eff_range, referenceMax, ...
    'VariableNames', {'Average_wt', 'Std_Dev_wt', 'eff_Max_wt', 'eff_Min_wt', 'eff_Range_wt', 'Z_Scale_Reference'});
writetable(stats_table, sprintf('%s_Statistics.csv', baseName));

disp('✅ All data and plots exported successfully! / 全部数据与高清图像导出完毕！');
disp('=========================================================================');

% Reference:
% N. Sarkar and B. B. Chaudhuri, "An efficient differential box-counting approach to compute fractal dimension of image," in IEEE Transactions on Systems, Man, and Cybernetics, vol. 24, no. 1, pp. 115-120, Jan. 1994.