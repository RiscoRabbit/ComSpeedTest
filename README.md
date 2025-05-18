# ComSpeedTest

This program sends a byte to a COM port, wait it is echoed back, and measure how long it took.

To make this program work as expected, diretory connect the serial TX port and RX port, so that the COM port echoes back the bytes.

On FT232RL, it took about 16ms as follows.
```
PS C:\> ./ComSpeedTest.exe COM6 --baudrate 115200
Com port speed tester
BaudRate : 115200
Port: COM6
Test start.
Sent:100 Rcvd*100
Test completed.
Total time        : 1600.1211 ms
Average  latency  : 16.001211 ms
Total byte sent   : 100 bytes
Total byte rcvd   : 100 bytes
Transfer rate     : 62.49526988926026 Bytes/sec
```

On my Raspberry pi pico USB serial bridge, it tool only 0.36ms.
```
PS C:\> ./ComSpeedTest.exe COM5 --baudrate 115200
Com port speed tester
BaudRate : 115200
Port: COM5
Test start.
Sent:100 Rcvd*100
Test completed.
Total time        : 35.8245 ms
Average  latency  : 0.358245 ms
Total byte sent   : 100 bytes
Total byte rcvd   : 100 bytes
Transfer rate     : 2791.385783472205 Bytes/sec
```
