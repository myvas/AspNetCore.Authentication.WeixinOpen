#!/bin/bash

echo "*************"
echo "** WARNING!"
echo "** This will delete all bin and obj folders!"
echo "** Press Ctrl-C to Cancel"
echo "*************"
echo
# Pause to wait for user input before exiting
read -p "Press any key to executing..." -n 1 -s
echo 

find . -type d \( -name "obj" -o -name "bin" \) -exec rm -rf {} +;

echo 
echo "*************"
echo "** Completed! All bin and obj folders are deleted."
echo "*************"
echo 
