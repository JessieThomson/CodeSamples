import pygame, sys, time, random
from pygame.locals import *
import numpy as np


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

pygame.init()
windowSurface = pygame.display.set_mode((500, 400), 0, 32)
pygame.display.set_caption("Paint")
# get screen size
info = pygame.display.Info()
sw = info.current_w
sh = info.current_h

grid = CalculateGrid(sw, sh, 50)  # NEED TO CALCULATE OCCUPIED VALUE FOR ALL GRID CELLS!!!!!!!!!!!!!!!!
y_size = len(grid[:])
x_size = len(grid[0])
cell_size_x = sw / x_size
cell_size_y = sh / y_size
print x_size, y_size

# for celly in range(0, y_size):
#     for cellx in range(0, x_size):
#         print grid[celly][cellx][0]

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

        # pygame.draw.circle(windowSurface, GREEN, (int(pos[0]), int(pos[1])), radius, 1)
    for cellx in range(0, x_size):
        for celly in range(0, y_size):
            sum_cell = 0
            for p in circle_objs:
                sum_cell += pow(p.Radius, 2) / (pow((grid[celly][cellx][0]) - p.Position[0], 2) + pow((grid[celly][cellx][1]) - p.Position[1], 2))

            if sum_cell > 1:
                pygame.draw.rect(windowSurface, GREEN, [grid[celly][cellx][0], grid[celly][cellx][1], 3, 3], 0)

    pygame.time.Clock().tick(20)
    pygame.display.update()