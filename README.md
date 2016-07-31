# echo
Meant to be used as a diagnostic tool to aid in tracing distributed systems.

Echo consists of two logical pieces: The first is a console-like aspect for viewing trace messages that are emitted via http calls to the echo service. The second is a power-bi event viewing piece. I use PowerBI to act as a configurable "View" of the trace events that are emitted, again, by calling in to an http service. 
