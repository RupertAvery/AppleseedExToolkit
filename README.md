# Appleseed EX AFS Viewer

This is a viewer and eventually editor for the ARC.AFS data file in the ISO of the PlayStation 2 game Appleseed EX.

The ARC.AFS file is an uncompressed packed file containing several files used by the game including

* Textures - images, fonts, loading screens, character images in TIM2 format
* Models - 3D objects in LDP and LFM format
* Other data - Data nested several layers deep in DAR format. This includes item descriptions, game dialog, system messages. The application focuses primarily on this data.

# Requirements

* .NET 8.0 Desktop Runtime
* A copy of ARC.AFS extracted from the Appleseed EX ISO

# Instructions

* Click File > Open and select the ARC.AFS file
* The list of files inside the archive will be listed in the left treeview.
* DAR files can be expanded to look at the data inside
* TM2 files can be viewed

# Features

* View TM2 texture images directly from the archive
* Custom view for items in `item.dar` entries - Select **Data View: Item**
* View data as Japanese text - Select **Data View: Shift-JIS**
* View data as hex - Select **Data View: Hexadecimal**

# TODO

* Text editing
* AFS repacking
* Image/File extracting (and insertion?)
