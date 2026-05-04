--- HIGHEST PRIORITY ---

* Create .NET project base and push to repo
 - As a developer I want a ready foundation of the project structure in the repo
   so we can work from the same point.

* Implement service core logic
 - As a developer I want the service to log return whether specified URL returned a 200 OK or not

* Configure service
 - As a system administrator I want the service to read a specified URL and a set time interval to monitor

--- MEDIUM PRIORITY ---

* Extend service capabilities
 - As a system administrator I want to perform GET request on more than one URL at a time

* Service logging 
 - As a user I want the service to log request finished time in millisecond so that I can see
   if a site is performing unusually slow

--- LOW PRIORITY ---

* Warnings
 - As a user I want a warning to be logged on longer than 1000ms requests

* Docker
 - As a developer I want a Dockerfile implemented in the repo root for easy deployment
