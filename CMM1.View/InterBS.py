import numpy as np
import matplotlib.pyplot as plt
from scipy.interpolate import make_interp_spline

# Исходные данные (точки, через которые должен проходить сплайн)
x = np.array([0, 1, 3, 4, 6])
y = np.array([0, 2, -1, 4, 3])

# Создание интерполяционного B-сплайна (степень 3)
spline = make_interp_spline(x, y, k=1)

# Генерация точек для гладкого отображения сплайна
x_smooth = np.linspace(x.min(), x.max(), 100)
y_smooth = spline(x_smooth)

# Визуализация
plt.figure(figsize=(10, 6))
plt.scatter(x, y, color='red', label='Исходные точки', zorder=3)  # Исходные точки
plt.plot(x_smooth, y_smooth, 'b-', label='Интерполяционный B-сплайн')  # Кривая сплайна
plt.title('Интерполяционный B-сплайн степени 3')
plt.xlabel('x')
plt.ylabel('y')
plt.grid(True, linestyle='--', alpha=0.7)
plt.legend()
plt.show()