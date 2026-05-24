# .NET: Application Insights Availability Checks
This is a .NET application that runs in Kubernetes, that is configuration driven, and will monitor internal web applications.   

# Log Test
I always include a key in here, it's not entirely necessary but it does add an extra gate in the process. This can reduce exploitation.   
While this is an internal service, and not likely to be exploited, we also don't want other processes, users, to pound this with traffic.   
If this were to happen it would blow up the logs with data.  
If there were ever a sitatuation and we need to reduce traffic, the service is accidentally exposed publicly, rolling the log key (guid) can instantly reduce and drop traffic.  

```
https://localhost:7127/up/log?logKey=3484991B-8304-4B3E-8784-BBDABF6DE346
```
