## **HIGHEST PRIORITY**


* ### `Create .NET project base and push to repo`
    - As a **developer**, I want a ready foundation of the project structure in the repo
      so we can work from the same point.


* ### `Implement service core logic`
    - As a **user**, I want the service to log return whether specified URL returned a 200 OK or not


* ### `Configure service`
    - As a **system administrator**, I want the service to read a specified URL and a set time interval to monitor


* ### `Persistent Storage (Database Logging)`
    - As an **analyst**, I want the uptime results saved to a local SQLite database, so I can query historical uptime and response time data later.


* ### `Resilience & Retries (Polly Integration)`
    - As a **system administrator**, I want the monitor to retry a failed connection three times with a short delay before officially logging it as "down," so that temporary network blips don't trigger false alarms.


## **MEDIUM PRIORITY**


* ### `Extend service capabilities`
    - As a **system administrator**, I want to perform GET request on more than one URL at a time


* ### `Service logging` 
    - As a **user**, I want the service to log request finished time in millisecond so that I can see
      if a site is performing unusually slow


* ### `Webhook Notifications`
    - As a **developer**, I want the application to send an HTTP POST request to a Discord or Slack webhook when a URL goes offline, so the team is notified immediately without watching the console.


## **LOW PRIORITY**

* ### `Concurrent Monitoring`
    - As a **user**, I want the service to ping multiple URLs asynchronously at the exact same time, so that monitoring a list of 50 sites doesn't take 50 times longer than monitoring one.


* ### `Warnings`
    - As a **user**, I want a warning to be logged on longer than 1000ms requests


* ### `Docker`
    - As a **developer**, I want a Dockerfile implemented in the repo root for easy deployment

