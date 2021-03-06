TODO:
- Requirement: Upload new SLN
- Requirement: Move storage table code over to new sln
- Requirement: Console web view
- Requirement: PowerBI implementation
- Requirement: Implementation of maintenance engines
- Test Requirement: 'this' implementation so that echo-net records itself
- Experimental: Attempt machine learning to auto categorize messages based on contents of echo messages.
- Experimental: Use this data to auto generate reports by auto categorizing kinds of messages and then generating reports from selected sets of kinds of messages (i.e. basic perf messages can be inferred from start/stop kinds) 
 

# echo-net
Meant to be used as a 'printf-like diagnostic tool' to aid in tracing distributed systems.

To the end user `echo-net` consists of two logical pieces: The first is a console-like aspect for viewing trace messages that are emitted via http calls to the `echo-net` service. The second is a PowerBI event viewing piece. I use PowerBI to act as a configurable "View" of the trace events that are emitted, again, by calling in to an http service. 

The backend of `echo-net` is a Service Fabric ASP.Net Core application.

All messages echoed to the `console` have a relatively ephemeral life span of 1 hour. 

## goals
- Create a cloud based printf-based console to help with debugging distributed systems.
- Make sharing information easy.
- Give developers the ability to inform their managers explicitly about their producitivty, and get in their way as little as possible. Prioritizing situations where if possible, not at all. 
- Gather feedback on usefulness
 
## examples
## emitting information
### `echo-net` to the 'console' to get developer-centric information...
We 'echo' messages in to a `channel: dotnet`, under the `category: noise`

```
curl --data-urlencode "message=Hello world" --get http://localhost/echonet/console?channel=dotnet&category=noise
```

### `echo-net` business intelligence to get an idea of the larger picture...

Emit a row in to a PowerBI `container: dotnet` and a PowerBI `table: noise`.
The event is of:
- `order: 0` 
- `cardinality: 1`
- `kind: start`
- `category: recv-file`
```
curl --get http://localhost/echonet/bi?container=dotnet&table=noise&order=0&cardinality=1&kind=start&category=recv-file
```
## viewing information
### console view
`Coming soon`
### business intelligence view
`Coming soon`


# planned features
- Private channels
- Ability to prevent messages from being deleted.

