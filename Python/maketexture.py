from dataclasses import dataclass

import cv2
import imageio
import matplotlib.pyplot as plt
import numpy as np

def parse_line(line):
    pos, colo = line.split(' : ')

    xyz = [float(x) for x in pos.split(' ')]
    rgba = [float(x) for x in colo[5:-2].split(',')]
    return xyz, rgba

@dataclass
class SegmentV:
    x: float
    y_lo: float
    y_hi: float
    name: str

    # overloads in
    def __contains__(self, item):
        return item[0] == self.x and self.y_lo < item[1] < self.y_hi

    def filename(self):
        return f'segment_{self.name},{self.x},{self.y_lo}-{self.x},{self.y_hi}'

@dataclass
class SegmentH:
    y: float
    x_lo: float
    x_hi: float
    name: str

    # overloads in
    def __contains__(self, item):
        return item[1] == self.y and self.x_lo < item[0] < self.x_hi

    def filename(self):
        return f'segment_{self.name},{self.x_lo},{self.y}-{self.x_hi},{self.y}'

def tf(rgba: np.array):

    if rgba == [1, 0, 1, 1]:
        return [0, 0, 0, 1]
    #     return [171 / 255, 215 / 255, 235 / 255, 1]

    return rgba

    # xyz = [0, 0, 0]5
    # xyz[0] = 0.412453 * rgba[0] + 0.357580 * rgba[1] + 0.180423 * rgba[2]
    # xyz[1] = 0.212671 * rgba[0] + 0.715160 * rgba[1] + 0.072169 * rgba[2]
    # xyz[2] = 0.019334 * rgba[0] + 0.119193 * rgba[1] + 0.950227 * rgba[2]
    #
    # new_rgba = rgba / np.linalg.norm(rgba)
    # new_rgba[3] = 1
    #
    # return new_rgba
    # # return xyz[1]

def segment_texture(lines, seg):
    colors = np.array([tf(color) for pos, color in lines if pos in seg])

    n_points = len(colors) // n_angles
    colors_sq = colors.reshape((n_points, n_angles, 4))

    return colors_sq

if __name__ == '__main__':
    # todo proper color mapping and regen at higher sample count/resolution

    # n_angles = 5
    # path = 'radiance.txt'

    n_angles = 300
    path = '../Assets/radiance.txt'

    # scene
    a = SegmentH(3, 0, 7, 'a')
    b = SegmentV(7, -6, 3, 'b')
    c = SegmentH(-6, 2, 7, 'c')
    d = SegmentV(2, -6, -4, 'd')
    e = SegmentH(-4, 2, 5, 'e')
    f = SegmentV(5, -4, -3, 'f')
    g = SegmentH(-3, 0, 5, 'g')

    segments = [a, b, c, d, e, f, g]

    # read the radiance file
    lines = [parse_line(line) for line in open(path)]

    textures = [segment_texture(lines, seg) for seg in segments]
    big_texture = np.concatenate(textures)

    big_texture = np.concatenate([big_texture] * 1, axis=1)

    big_texture *= 5

    plt.imshow(big_texture)
    plt.show()
    #
    # imageio.imwrite(f'../Assets/global_texture.png', big_texture)
