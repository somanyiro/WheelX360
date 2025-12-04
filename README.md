# WheelX360

This project is aimed at making games playable with racing wheels even if those games only natively support controllers. It does this by creating a virtual Xbox 360 controller and mapping the wheel's input onto it.


## Why

In many ways this project is unnecessary, there are many existing solutions for mapping onto a 360 controller. ([x360ce](https://www.x360ce.com/) for exampe) However, what other alternatives lack, is translating controller rumble into force feedback and applying a centering force to the wheel. This implementation allows for the configuration of a centering force and different rumble effects.  

## Tech Stack

- Raylib for window creation
- DearImGUI for UI
- Windows.Gaming.Input.ForceFeedback for force feedback on the wheel
- NetMQ for comunication between the main program and the background service
- ViGEm for controller emulation

## TODO

- Move to Avalonia UI
- Implement UI for changing the button mapping
- Test on more types of racing wheels

## Support 
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/F1F517KH5W)