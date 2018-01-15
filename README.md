# BasicTransfer

[![](https://img.shields.io/badge/version-1.0-brightgreen.svg)]() ![](https://img.shields.io/maintenance/yes/2018.svg)

Ping-like program that can send HTTP requests and color the results.

Download it here: [[Stable Releases]](https://github.com/Killeroo/Requester/releases)
***
![alt text](docs/screenshots/screenshot.png "PowerPing in action")

## Features

- Results coloration 
- Ping like functionality
- HTTP and HTTPS support

## Usage: 
     Requester.exe web_address [-d] [-t] [-ts] [-n count] [-i interval]
               
## Arguments:
     [-d] Detailed mode: shows server and cache info
     [-t] Infinite mode: Keep sending requests until stopped (Ctrl-C)
     [-n count] Send a specific number of requests
     [-ts] Include timestamp of when each request was sent
     [-i interval] Interval between each request in milliseconds 
                   (default 30000)
     
## License

Requester is licensed under the [MIT license](LICENSE).

*Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=*
