WPL2USB
=======

Takes a Windows Play List and copies the music files to a USB drive or any other selected drive/folder

This is written in c# using visual studio 2012.

If you download open the WPL2USB.sln solution file with studio 2012

This is a very simple program. It will take a windows playlist and copy the files to a USB drive or anything else that supports 
drive type mappings.

I use it to put my Windows Play List onto a USB drive for my Nissan LEAF.

Some feature:
Playlist to Play Folder: (check the Use Playlist Name option), Copy files from a windows playlist to a 
destination folder with the destination folder being named the playlist and all song files copied to that folder. 
This feature will not copy to the destination the album or artist name. For example if you have 5 different artist 
each with multiple albums and the play list is called Mells Mix; on the usb drive there will be a folder 
called Mell Mix with just the songs in that folder (this ideal for the LEAF)

Clean Names: This remove any letters that don't fall into A-Z, a-z, 0-9, -,. , [space]

if you just need to copy and keep artist/album paths, just uncheck the Use Playlist Name option.


