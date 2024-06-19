#!/bin/bash
#
# Files to work with:
KEYMAP="keyboard-map-en-us.txt"
DEFAULTIN="defaultProfile.xml"
CUSTOMIN="layout_Kenjiro_exported.xml"
DEFAULTOUT="default-keybinds.txt"
CUSTOMOUT="custom-keybinds.txt"
CTMP="customtmp.txt"
CTMP2="customtmp2.txt"
CTMP3="customtmp3.txt"

#
# Initiates some variables
###VARLINE=`grep "action name=" $DEFAULTIN | grep "keyboard="`
###echo $VARLINE
KEYB=""     # Name of the keybind
KEYV=""     # Value of the keybind
KEYV1=""    # First part of the KEYV (if compound)
KEYV2=""    # Second part of the KEYV (if compound)
KEYC=""     # Final Keycode
KEYC1=""    # First keycode (if compound)
KEYC2=""    # Second keycode (if compound)
KEYT=""     # Temporary value
VARF=""

#
# Default Keybinds
#
LINE=""
while read LINE
do
    echo "$LINE" | grep "action name=" | grep "keyboard=" > /dev/null
    if [ $? == 0 ]
    then
        # Gets the name of the keybind
        KEYB=`echo $LINE | cut -d'"' -f 2`
        #echo "KEYB = $KEYB"
        # Gets the name of the key to be used
        KEYV=`echo $LINE | sed -n -e 's/^.*keyboard//p' | cut -d'"' -f 2 | egrep -v -i "wheel|HMD|mouse"`
        #echo "KEYV = $KEYV"
        # Checks if KEYV isn't empty or doesn't have a white space
        if [[ ! $KEYV =~ ^( ||)$ ]]
        then
            # Checks if there is a "+" in the string
            echo $KEYV | grep "+" > /dev/null
            if [ $? == 0 ]
            then
                KEYV1=`echo $KEYV | cut -d"+" -f1`
                #echo "KEYV1 = $KEYV1"
                KEYV2=`echo $KEYV | cut -d"+" -f2`
                #echo "KEYV2 = $KEYV2"
                KEYC1=`grep -i "Key_$KEYV1;" $KEYMAP | cut -d";" -f2`
                #echo "KEYC1 = $KEYC1"
                KEYC2=`grep -i "Key_$KEYV2;" $KEYMAP | cut -d";" -f2`
                #echo "KEYC2 = $KEYC2"
                KEYC="[$KEYC1]+[$KEYC2]"    # Sets the compounded keycodes
                #echo "KEYC = $KEYC"        
            else
                KEYT=`grep -i "Key_$KEYV;" $KEYMAP | cut -d";" -f2`
                #echo "KEYT = $KEYT"
                KEYC="[$KEYT]"              # Sets the keycode
                #echo "KEYC = $KEYC"
            fi
            echo "$KEYB=$KEYC"
            echo "$KEYB=$KEYC" >> $DEFAULTOUT
#            VARF=`printf "$VARF\n$KEYB=$KEYC"`
#            echo "VARF = $VARF"
        fi      
    fi
done < $DEFAULTIN

#
# Custom Keybinds
#
> $CTMP
> $CTMP2
> $CTMP3
# 1- Picks only the lines containing "action name" or "rebind input"
# 2- Removes some characters and add a new line before "  action"
#
egrep "action name|rebind input" $CUSTOMIN | sed -e 's/<//' -e 's/\/>//' -e 's/>//' | sed -e 's/  action/\naction/' -e 's/\r//g' > $CTMP
# 3- Makes sure all lines begin with "action name" and contain "rebind input"
#
LINE=""
while read LINE
do
	if [ "$LINE" != "" ]
	then 
		printf "$LINE;" >> $CTMP2
	else
		printf "\n" >> $CTMP2
	fi
done < $CTMP
cat $CTMP2 | grep "kb" > $CTMP3
LINE=""
while read LINE
do
#    echo "$LINE" | grep "kb" > /dev/null
#    if [ $? == 0 ]
#    then
        # Gets the name of the keybind
        KEYB=`echo $LINE | cut -d'"' -f 2`
        #echo "KEYB = $KEYB"
        # Gets the name of the key to be used
        KEYV=`echo $LINE | cut -d'"' -f 4 | sed 's/kb1_//' | egrep -v -i "wheel|HMD|mouse"`
        #echo "KEYV = $KEYV"
        # Checks if KEYV isn't empty or doesn't have a white space
        if [[ ! $KEYV =~ ^( ||)$ ]]
        then
            # Checks if there is a "+" in the string
            echo $KEYV | grep "+" > /dev/null
            if [ $? == 0 ]
            then
                KEYV1=`echo $KEYV | cut -d"+" -f1`
                #echo "KEYV1 = $KEYV1"
                KEYV2=`echo $KEYV | cut -d"+" -f2`
                #echo "KEYV2 = $KEYV2"
                KEYC1=`grep -i "Key_$KEYV1;" $KEYMAP | cut -d";" -f2`
                #echo "KEYC1 = $KEYC1"
                KEYC2=`grep -i "Key_$KEYV2;" $KEYMAP | cut -d";" -f2`
                #echo "KEYC2 = $KEYC2"
                KEYC="[$KEYC1]+[$KEYC2]"    # Sets the compounded keycodes
                #echo "KEYC = $KEYC"        
            else
                KEYT=`grep -i "Key_$KEYV;" $KEYMAP | cut -d";" -f2`
                #echo "KEYT = $KEYT"
                KEYC="[$KEYT]"              # Sets the keycode
                #echo "KEYC = $KEYC"
            fi
            echo "$KEYB=$KEYC"
            echo "$KEYB=$KEYC" >> $CUSTOMOUT
###            VARF=`printf "$VARF\n$KEYB=$KEYC"`
###            echo "VARF = $VARF"
        fi      
#    fi
done < $CTMP3
rm $CTMP $CTMP2 $CTMP3 
#
###echo "VARF FINAL = $VARF"