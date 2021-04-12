Master Branch: ![.NET Core](https://github.com/CedModV2/CedMod/workflows/.NET%20Core%20Master/badge.svg?branch=master)

Dev Branch: ![.NET Core](https://github.com/CedModV2/CedMod/workflows/.NET%20Core%20Dev/badge.svg?branch=master)

CedMod builds are located on the Actions tab (download is also available on the CedMod Admin panel https://admin.cedmod.nl)

| config                              | type      | default  | description                                                           |
|-------------------------------------|----------:|:--------:|:---------------------------------------------------------------------:|
| cm: MainPlugin                                                                                                                     |
| is_enabled                          |   bool    | true     | If the plugin is enabled                                              |
| kick_same_namengscreen              |   bool    | true     | If Users with the same name will be kick                              |
| ced_mod_api_api                     |   string  | none     | API Key for the CedMod API                                            |
| autoban_enabled                     |   bool    | false    | if the FF autoban is enabled                                          |
| autoban_threshold                   |   int     | 3        | the amount of Teamkills before the autoban will ban                   |
| autoban_duration                    |   int     | 4320     | the amount of time a user will be banned after triggering the autoban |
| autoban_reason                      |   string  | You have teamkilled too many people | the reason for the autoban bans            |
| autoban_disarmed_class_d_tk         |   bool    | true     | if disarmed class D kills are considered teamkills                    | 
| autoban_disarmed_scientist_d_tk     |   bool    | true     | if disarmed scientist kills are considered teamkills                  |
| autoban_class_dvs_class_d           |   bool    | true     | if class D vs class D kills are considered teamkills                  |
| report_blacklist                    |   list    | []       | userids in this list will not be able to use ingame reports           |
| staff_report_allowed                |   bool    | false    | if staff can be reported using ingame reports                         |
| staff_report_message                |   string  | You can not report a staff member                                                |
|                                                                                                                                    |
| cm_WAPI                                                                                                                            |
| is_enabled                          |   bool    | true     | If the plugin is enabled                                              |
| disallowed_web_commands             |   list    | []       | Commands that will not be able to run                                 |
| security_key                        |   string  | string   | key for remote commands, set the same as in the panel                 |
| query_override                      |   bool    | bool     | UnUsed                                                                |
| Port                                |   int     | 8000     | Port that the QuerySystem (remote commands) will use                  |
