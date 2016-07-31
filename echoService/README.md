# echo
Meant to be used as a 'printf-like diagnostic tool' to aid in tracing distributed systems.

Echo consists of two logical pieces: The first is a console-like aspect for viewing trace messages that are emitted via http calls to the echo service. The second is a power-bi event viewing piece. I use PowerBI to act as a configurable "View" of the trace events that are emitted, again, by calling in to an http service. 

## Example
### echo to the 'console'
We 'echo' messages in to a channel named 'dotnet', under the category 'noise.'

```
curl --data-urlencode "message=Hello world" --get http://localhost:8671/console/echo/dotnet/noise/
```

### echo to the 'canvas'
Emit a row in to a PowerBI `container: dotnet` and a PowerBI `table: noise`.
The event is of `order: 0`, `cardinality: 1`, and it is a `kind: start` event.

```
curl --get http://localhost:8671/canvas/echo/dotnet/noise/0/1/start
```

## Goals
- echoing information should be fast and non-essential (i.e. if we fail to find the service, move along)
- an end-user can union channels/categories together. This means independent pieces can be developed in isolation, and then
when the distributed pieces come together the same trace calls will work to examine the entire distributed system.
- echoing information to a 'canvas' is a whimsical way of saying that we're emitting an event in to our PowerBI container.
