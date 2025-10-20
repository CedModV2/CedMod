
V: 3.4.29
 - Improved prediction on Anti-ESP
   - Instead of a static prediction factor, it will now try to check in increments of 0.25 meters up to 3 attempts, instead of the previous static 0.75 meter prediction.
 
 - Removed the PlayerList spoofing override for scp 1344 (goggles), as it is no longer necessary.