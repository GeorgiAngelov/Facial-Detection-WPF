c# Facial-Detection-WPF
====================

This program is a facial detection program that either uses the camera to continuously take images at small
intervals and detects the number of faces, or the user has the option to upload an image from their computer.
Once an image is captured, whether from the camera or from upload, the image is passed to a detectFaces function
that does the neccessary manipulations to detect and draw rectangles around the faces of every person in the
image. Afterwards, the image is displayed on the screen along with the number of faces detect which is located
on the right panel.

The left panel has 3 buttons and 2 checkboxes:
----------------------------------------------
The first two buttons are the modes that the user can select to acquire an image.
The third button is if the user wants to save the modified image to their computer.
The checkboxes provide simple filters to manipulate the image but they are not that helpful to the image
detection. If time allowed, I would have spent more time on improving the image quality and also detection
algorithm to better detect faces. However, the current state of the software is still very functional
in a way that captures all close up faces in the image.

The sofware is set up to be be compiled as a 32 bit program because EmguCV's 64 bit version is pretty hard
to set up and so I decided to spend my time on the actualy software rather than spending the time to take
advantage of the 64 bit architecture.

Notes for use:
--------------
If the users starts the sofware and then tries to immediately save an image, without selecting any image mode,
the button will not be functional, even though the user may try to select it.
When using the Camera as input, the program is a bit laggy and that is a common problem with EmguCV facial
detection and as far as I know there are ways to fix this but they will require more time and it will be
something that I do later on along with the better filters.
