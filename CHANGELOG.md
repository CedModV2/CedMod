
V: 3.4.25
- Introduced CMAC officially.
   - Prevents Harmless Tesla cheats (negates tesla damage)<sup>1*</sup>
   - Prevents pickup through walls cheats<sup>1*</sup>
   - Prevents Movement Unlock cheats (allows movement where the game would normally lock your movement)<sup>1*</sup>
   - Introduced a system to prevent wallhacks.<sup>2*</sup>
- Improved communication to Sentinal<sup>3*</sup>
- Fixed OnPlayerDying log on the panel when there was no killer.
- Fixed the jail command spawning you up in the air if unjailed after warhead detonation.

<sup>1*</sup> Detection results will be shared with the CedMod gateway to create a cheat profile on the user, which will be used to determine if the user is cheating based on the currently implemented detections
If enough detections are triggered the user will be banned based on their account standing (lower/newer standing means faster action)<br>
These features have been tested and adapted for users with higher than usual or unstable connections as best as possible
We continue to monitor results detection logs and making changes where necessary.
Please note that effectiveness is dependent on how many cheaters use the detected features until we add detections for more features with <sup>3*</sup><br>

<sup>2*</sup> This system will hide players from others when they cannot actually see them with a series of checks to make it effective yet unnoticeable for normal players<br>Please note that this currently does not apply to SCP's<br>

<sup>3*</sup> Sentinal is the WIP completely CedMod-server-side simulation system which is capable of reconstructing the entire round out side of SL.<br>
In the future it will be used to detect more sophisticated cheat features, eg aimbot, pSilent, etc. while completely hiding the detection code.