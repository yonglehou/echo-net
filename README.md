# echo-net
Meant to be used as a 'printf-like diagnostic tool' to aid in tracing distributed systems.

To the end user `echo-net` consists of two logical pieces: The first is a console-like aspect for viewing trace messages that are emitted via http calls to the echo service. The second is a power-bi event viewing piece. I use PowerBI to act as a configurable "View" of the trace events that are emitted, again, by calling in to an http service. 

The backend of `echo-net` is a Service Fabric Application that uses an OWIN Service Listener.

All messages echoed to the `console` have a very ephemeral life span: 1 hour. 

Planned features inclue the ability to lock messages from deletion, and the creation of private channels.

## Examples
## Emitting Information
### `echo-net` to the 'console' to get developer-centric information...
We 'echo' messages in to a `channel: dotnet`, under the `category: noise`

```
curl --data-urlencode "message=Hello world" --get http://reliability/echonet/console?channel=dotnet&category=noise
```

### `echo-net` business intelligence to get an idea of the larger picture...

Emit a row in to a PowerBI `container: dotnet` and a PowerBI `table: noise`.
The event is of:
- `order: 0` 
- `cardinality: 1`
- `kind: start`
- `category: recv-file`
```
curl --get http://reliability/echonet/bi?container=dotnet&table=noise&order=0&cardinality=1&kind=start&category=recv-file
```
## Viewing Information
### Console View
`Coming soon`
### Canvas View
`Coming soon`

## Goals
- echoing information should be fast and non-essential (i.e. if we fail to find the service, move along)
- an end-user can union channels/categories together. This means independent pieces can be developed in isolation, and then
when the distributed pieces come together the same trace calls will work to examine the entire distributed system.
- echoing information to a 'canvas' is a whimsical way of saying that we're emitting an event in to our PowerBI container.
