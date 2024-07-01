
V: 3.4.18
 - Rewrote WebSocket code to use base c# websockets instead of a library, this appears to be more stable.
 - Added some log supression, so there is less console spam on servers that dont have any players for longer periods of time.
 - Modified the code responsible for autoupdating and sending heartbeats to work properly with Idle mode active.
 - Removed Message splitting in favor of message chunking as now supported by using the base c# websockets.