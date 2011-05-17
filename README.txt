==========================
README -- LOG FILE MANAGER
==========================

This is a very quick beta of a log file manager for feature #630.

-----------------------------
Description (via Chris Heinz)
=============================

Since the change to make our scheduled tasks for order processing run every minute, orders are processing really fast, which is good. I placed an order this weekend and got my license before I got an email that the order was in processing! However, this means the log files are growing much more frequently.

Do you guys have any opinions on how to manage this in a better way? I was thinking a project for an intern to have something run once a day that zip and timestamps all log files named in a certain way. For instance, the commerce log files remain active with a .txt extension until it grows to 2MB. When the file hits the size threshold, it is renamed to .txt1, .txt2, .txt3, etc. Currently, order fulfillment is at .txt146, at a growth rate of 5 new files a day. If there was a tool that ran every day that would zip up all of the .txt%d extension files in an archive with the days timestamp as its name, then remove the logs, that would be a good solution that would preserve the logs in case they are needed, and free up the disc space and folder clutter.

------------
Instructions
============

The built .exe is included in LogFileManager/LogFileManager/bin/Debug, as are a set of .txt%d files that are known to contain some ASCII art and the file Ionic.Zip.dll, which is a library that assists interfacing with .zip files. To execute, simply run the executable file. The .txt%d files can be viewed by opening directly in Notepad(++) or some other text editor that suits your fancy.

The executable MUST (and I stress this) be in the same directory as the files to archive. If it is not, then... bad things will happen. Not sure what (probably some weird exception), but it's assumed throughout the whole program.

If you want to conduct a full-fledged test of the program, a File Generator is also included. The generator will create a number of random files with the .txt4 extension, each containing its own filename a random number of times. Simply copy LogFileManager.exe into the same folder as the files and execute for results.

-------------------
Other Documentation
===================

LogFileManager makes use of the freeware library DotNetZip, included with this program as Ionic.Zip.dll. The program makes use of a few regular expressions to search for files to archive. The core expression is:

	@"^.*\.txt\d+$"
	
@"" encloses the string literal, with ^ and $ marking the start and end of the string. .* indicates any number of any given character. \. is a delimited period, the start of the file extension txt. \d+ indicates any number greater than 0 of decimal digits (such that .txt files are not archived).

Should the program encounter any issues, it will display an error message depending on the severity of the issue. A minor issue (invalid filename when archiving or deleting files) will allow the program to continue. A major issue (no files to archive, invalid directory) will cause the program to terminate.

--------------
Update History
==============

v1.0b -- 05/17/2011:
+ Updated code to store the set of relevant filenames in a string array instead of a List. This allows for faster lookup times and a more compact storage. Not entirely noticeable on small amounts of files, but definitely noticeable on mass quantities.
+ Now with exception catches!
	+ If the SRC or DST directories are invalid, returns an appropriate error message. SRC errors 
	will cause the program to terminate. DST errors will cause the program to skip file deletion, 
	cleanup any leftover .tmp files, then terminate.
	+ If files cannot be deleted anywhere for any reason, an error message will be output detailing 
	the situation and the file will remain undeleted.
+ The program now reports success and the number of files archived and deleted, respectively.
+ A bar has been added that shows the program's progress through the number of files.

v0.1 -- 05/16/2011:
+ First draft of the code. Does what it's supposed to with some very basic functionality.
+ Exceptions are not caught, so don't be surprised if it crashes.

And that's all (for now) folks
~ Zac Forshee