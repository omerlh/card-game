# Cards Game
A small matching game I build for my son.
The goal is to match between a card (represent, for example, an animal home) and the animal displayed on the screen.
To run it you need (this is what I'm using, there are many alternatives out there):
* [Raspberry PI](https://www.raspberrypi.org/)
* [RFID reader](http://www.ebay.com/itm/222514227900?_trksid=p2060353.m2749.l2649&ssPageName=STRK%3AMEBIDX%3AIT)
* [RFID cards](http://www.ebay.com/itm/172411560187?_trksid=p2060353.m2749.l2649&var=471241240634&ssPageName=STRK%3AMEBIDX%3AIT)

## Running
Currently, building .net core is not supported on Raspberry PI (more details [here](https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md)). 
So, you need to build it on your Mac/PC for ARM architecture using:
```
dotnet publish -r linux-arm
```
And then, use `scp` to copy the output folder to the Raspberry PI.
Now, you can start the server on the Raspberry using:
```
sudo ./server
```
Make sure you enabled GPIO for your Raspberry PI.
Now, on your Mac/PC run the web using (from the web folder):
```
elm-reactor
```
Don't forget to change the websocket ip.