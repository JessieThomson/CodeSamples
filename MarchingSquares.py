import pygame, sys, time, random
from pygame.locals import *
import numpy as np
import math


class Particle:

    """
    @summary: Data class to store particle details i.e. Position, Direction and speed of movement, radius, etc
    """
    def __init__(self):
        self.__version = 0
        """@type: int"""
        self.__position = []
        # Sub dicts of the whole vertexMarkupDict
        self.__movement = []
        self.__radius = 0

    # Python overrides -------------------------------------------------------------------------------------------------

    def __str__(self):
        printStr = ''
        printStr += 'Position: (' + str(self.__position[0]) + ',' + str(self.__position[1]) + ') '
        printStr += 'Direction and Speed: (' + str(self.__movement[0]) + ',' + str(self.__movement[1]) + ') '
        printStr += 'Radius: ' + str(self.__radius)
        return printStr

    def __setitem__(self, position, movement, rad, c):
        print position, movement, rad
        # TODO: Check inputs
        self.__position = position
        self.__movement = movement
        self.__radius = rad

    # Properties -------------------------------------------------------------------------------------------------------

    @property
    def Position(self):
        return self.__position

    @property
    def Movement(self):
        return self.__movement

    @property
    def Radius(self):
        return self.__radius

    # Methods ----------------------------------------------------------------------------------------------------------

    def SetPosition(self, pos):
        self.__position = pos

    def SetMovement(self, move):
        self.__movement = move

    def SetRadius(self, rad):
        self.__radius = rad


def CalculateGrid(screenWidth, screenHeight, resolution):
    x_size = resolution + divmod(screenWidth, resolution)[1]
    y_size = resolution + divmod(screenHeight, resolution)[1]
    print x_size, y_size
    grid = []

    for y in range(0, y_size):
        temp_list = []
        for x in range(0, x_size):
            temp_list += [[x * (screenWidth / x_size), y * (screenHeight / y_size)]]
        grid += [temp_list]

    print np.array(grid).shape
    return grid

# Takes a binary string of all the four corners of the cell, where 1 = occupied and 0 = empty.
#   The corners ('0000') read from left to right: top-left, top-right, bottom-right, bottom-left
# Returns a list with pairs of x,y co-ordinates for the start and end of the line(s)
#  A--B      0--1
#  |  |  or  |  |
#  C--D      3--2
def DrawLine(score, x, y, sizex, sizey, sum_corners):
    # top_centre = [x + int(sizex/2), y]
    # left_centre = [x, int(y + sizey/2)]
    # bottom_centre = [x + int(sizex/2), y + sizey]
    # right_centre = [x + sizex, y + int(sizey/2)]

    # Interpolated points:
    P = [x + ((x + sizex) - x) * ((1 - sum_corners[0]) / (sum_corners[1] - sum_corners[0])), y]
    Q = [x + sizex, y + ((y + sizey) - y) * ((1 - sum_corners[1]) / (sum_corners[2] - sum_corners[1]))]
    R = [x + ((x + sizex) - x) * ((1 - sum_corners[3]) / (sum_corners[2] - sum_corners[3])), y + sizey]
    S = [x, y + ((y + sizey) - y) * ((1 - sum_corners[0]) / (sum_corners[3] - sum_corners[0]))]
    if int(score, 2) == 0 or int(score) == 15:
        return []
    elif int(score, 2) == 1 or int(score, 2) == 14:
        return [S, R]
    elif int(score, 2) == 2 or int(score, 2) == 13:
        return [R, Q]
    elif int(score, 2) == 3 or int(score, 2) == 12:
        return [S, Q]
    elif int(score, 2) == 4 or int(score, 2) == 11:
        return [P, Q]
    elif int(score, 2) == 5 or int(score, 2) == 10:
        return [S, P, R, Q]
    elif int(score, 2) == 6 or int(score, 2) == 9:
        return [P, R]
    elif int(score, 2) == 7 or int(score, 2) == 8:
        return [S, P]


pygame.init()
windowSurface = pygame.display.set_mode((500, 400), 0, 32)
pygame.display.set_caption("Paint")
# get screen size
info = pygame.display.Info()
sw = info.current_w
sh = info.current_h

grid = CalculateGrid(sw, sh, 12)  # 12
y_size = len(grid[:])
x_size = len(grid[0])
cell_size_x = sw / x_size
cell_size_y = sh / y_size
print x_size, y_size
print cell_size_x, cell_size_y

max_dx = 5
max_dy = 5
min_radius = 15
max_radius = 60

circle_objs = []
num_circles = 10

for i in range(0, num_circles):
    p = Particle()
    p.SetRadius(random.randrange(min_radius, max_radius))
    p.SetPosition([random.randrange(p.Radius, sw - p.Radius), random.randrange(p.Radius, sh - p.Radius)])
    p.SetMovement([random.random() * max_dx + 1, random.random() * max_dy + 1])
    circle_objs += [p]

BLACK = (0, 0, 0)
GREEN = (0, 255, 0)

windowSurface.fill(BLACK)
while True:
    for event in pygame.event.get():
        if event.type == QUIT:
            pygame.quit()
            sys.exit()

    windowSurface.fill(BLACK)
    sum_circle_position = [0, 0]
    sum_radius = 0
    # Make parallel
    for particle in circle_objs:
        dx = particle.Movement[0]
        dy = particle.Movement[1]
        radius = particle.Radius

        # update position with direction
        particle.SetPosition([particle.Position[0] + dx, particle.Position[1] + dy])
        pos = particle.Position

        # check bounds
        if (pos[0] - radius) + dx < 0 or (pos[0] + radius) + dx > sw:
            dx = -dx
            particle.SetMovement([dx, dy])
        if (pos[1] - radius) + dy < 0 or (pos[1] + radius) + dy > sh:
            dy = -dy
            particle.SetMovement([dx, dy])

        pygame.draw.circle(windowSurface, GREEN, (int(pos[0]), int(pos[1])), radius+1, 1)
        # pygame.draw.rect(windowSurface, GREEN, [grid[celly][cellx][0], grid[celly][cellx][1], 3, 3], 0)

    # Make parallel
    for cellx in range(0, x_size):
        for celly in range(0, y_size):
            score = ''
            sum_corner = [0, 0, 0, 0]

            x = grid[celly][cellx][0]
            y = grid[celly][cellx][1]

            for p in circle_objs:
                sum_corner[0] += pow(p.Radius, 2) / (pow(x - p.Position[0], 2) + pow(y - p.Position[1], 2))
                sum_corner[1] += pow(p.Radius, 2) / (pow((x + cell_size_x) - p.Position[0], 2) + pow(y - p.Position[1], 2))
                sum_corner[2] += pow(p.Radius, 2) / (pow((x + cell_size_x) - p.Position[0], 2) + pow((y + cell_size_y) - p.Position[1], 2))
                sum_corner[3] += pow(p.Radius, 2) / (pow(x - p.Position[0], 2) + pow((y + cell_size_y) - p.Position[1], 2))

            for corner in sum_corner:
                if corner > 1:
                    score += '1'
                else:
                    score += '0'

            if int(score, 2) > 0 and int(score, 2) < 15:
                lines = DrawLine(score, x, y, cell_size_x, cell_size_y, sum_corner)
                pygame.draw.lines(windowSurface, GREEN, False, lines, 1)

    pygame.time.Clock().tick(20)
    pygame.display.update()
