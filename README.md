# YAPC
Yet another player controller (rigidbody, physics based FPS) for the Flax Engine

Setup:
You can use the prefabs but basically it's a rigidbody with the script attached and three child nodes.
There is the collider necessary for the rididbody, a static model used for a more convenient representation
in the editor and obviously the camera.
The HUD simply places the crosshair texture in the middle of the screen and switches it on and off
dpending on whether the mouse is grabbed by the controller.
The simple reason for the abstract base class is that you can implement your own controller but keep
interfacing it from e.g. the GUI when it's necessary to get hold of the mouse and a visible cursor.

