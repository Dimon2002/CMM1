import matplotlib
import numpy as np
import matplotlib.pyplot as plt
from scipy.interpolate import griddata

# Функция для чтения данных из файла
def read_data(filename):
    x, y, z = [], [], []
    with open(filename, 'r') as file:
        for line in file:
            data = line.strip().split()
            x.append(float(data[0]))
            y.append(float(data[1]))
            z.append(float(data[2]))
    return np.array(x), np.array(y), np.array(z)

def read_config(filename):
    with open(filename, 'r') as file:
        for line in file:
            data = line
    return data

def read_points(filename):
    x, y = [], []
    with open(filename, 'r') as file:
        for line in file:
            data = line.strip().split()
            x.append(float(data[0]))
            y.append(float(data[1]))
    return np.array(x), np.array(y)

directory = read_config('config.txt')
# Чтение данных из файлов
filename1_2D = f'{directory}/dataFEM2D.txt'
filename1_3D = f'{directory}/dataFEM3D.txt'
filename2_2D = f'{directory}/dataSpline2D.txt'
filename2_3D = f'{directory}/dataSpline3D.txt'
filename3_2D = f'{directory}/dataTrue2D.txt'
filename3_3D = f'{directory}/dataTrue3D.txt'
points_file = f'{directory}/points.txt'

x1, y1, z1 = read_data(filename1_2D)
x2, y2, z2 = read_data(filename2_2D)
x3, y3, z3 = read_data(filename3_2D)

x1_3D, y1_3D, z1_3D = read_data(filename1_3D)
x2_3D, y2_3D, z2_3D = read_data(filename2_3D)
x3_3D, y3_3D, z3_3D = read_data(filename3_3D)

px, py = read_points(points_file)

# Определение минимальных и максимальных значений для x и y
min_x = min(min(x1), min(x2), min(x3))
max_x = max(max(x1), max(x2), max(x3))
min_y = min(min(y1), min(y2), min(y3))
max_y = max(max(y1), max(y2), max(y3))

# Создание сетки для отображения поверхностей с шагом 1
xi = np.arange(min_x, max_x + 1, 1)
yi = np.arange(min_y, max_y + 1, 1)
xi, yi = np.meshgrid(xi, yi)

# Интерполяция данных на сетку
zi1 = griddata((x1, y1), z1, (xi, yi), method='nearest')
zi2 = griddata((x2, y2), z2, (xi, yi), method='nearest')
zi3 = griddata((x3, y3), z3, (xi, yi), method='nearest')

zi1_3D = griddata((x1_3D, y1_3D), z1_3D, (xi, yi), method='nearest')
zi2_3D = griddata((x2_3D, y2_3D), z2_3D, (xi, yi), method='nearest')
zi3_3D = griddata((x3_3D, y3_3D), z3_3D, (xi, yi), method='nearest')

# Определение минимальных и максимальных значений для z
min_z = min(np.min(zi1), np.min(zi2), np.min(zi3))
max_z = max(np.max(zi1), np.max(zi2), np.max(zi3))

min_z_3D = min(np.min(zi1_3D), np.min(zi2_3D), np.min(zi3_3D))
max_z_3D = max(np.max(zi1_3D), np.max(zi2_3D), np.max(zi3_3D))

# Создание первого изображения для поверхностей
fig1 = plt.figure(figsize=(8, 6))
ax1 = fig1.add_subplot(111, projection='3d')
surf1 = ax1.plot_surface(xi, yi, zi1_3D, color='blue', label='Fem')
surf2 = ax1.plot_surface(xi, yi, zi2_3D, color='purple', label='Spline')
surf3 = ax1.plot_surface(xi, yi, zi3_3D, color='green', label='True')

ax1.set_xlabel('X')
ax1.set_ylabel('Y')
ax1.set_zlabel('Function value')
ax1.set_zlim(min_z_3D, max_z_3D)  # Установка шкалы по оси Z

# Добавление легенды для поверхностей
ax1.legend()

# Создание второго изображения для проекций
fig2 = plt.figure(figsize=(8, 6))
ax2 = fig2.add_subplot(111)

# Проекция по оси X и значению функции
ax2.plot(x1, z1, color='blue', label='Fem')
ax2.plot(x2, z2, color='purple', label='Spline')
ax2.plot(x3, z3, color='green', label='True')
ax2.scatter(px, py, color='red', label='Points', zorder=5)

ax2.set_xlabel('X')
ax2.set_ylabel('Function value')
ax2.set_ylim(min_z, max_z)  # Установка шкалы по оси Y

# Добавление легенды для проекций
ax2.legend()

ax1.set_xticks(np.arange(min_x, max_x + 1, 1))
ax1.set_yticks(np.arange(min_y, max_y + 1, 1))
ax2.set_xticks(np.arange(min_x, max_x + 1, 1))

# Отображение графиков
plt.show()