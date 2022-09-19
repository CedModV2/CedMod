V: 3.2.0
- All QuerySystem requests now use the QuerySystem V3 api
- Changed setup command to support QuerySystem V3 (refer to https://cedmod.nl/ChangeLog)
- Teamkill log data will now contain room position and rotation (TK Viewer will include rooms)
- Changed Websocket library to hopefully get rid of an exception thrown by WebSocketSharp that caused connection loss
- Added a system that will make sure the plugin reconnects to the panel if the auto-reconnect fails for whatever reason
- Added more dependencies to the assembly as those are only present for servers using certain plugins
- Added a function that will let the CommunityManagementPanel know that a cheat report is infact a cheat report
- If a user has hidden their tag, their tag will no longer be shown when AutoSlPerms executes a permission refresh, the tags hidden state will now be persistant between permission refreshes


The full CommunityManagementPanel changelog can be found on https://cedmod.nl/ChangeLog