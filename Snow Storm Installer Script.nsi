# Includes ========================================================
!include InstallerConstants.nsh
!include LogicLib.nsh


# Installer attributes ============================================

# Installing files ================================================
Section "SnowStorm ScreenSaver"

SetOutPath $INSTDIR

File "\\SnowStorm\\bin\\Release\\SnowStorm.scr"
#File "Readme.txt"
OutFile "EmptyInstaller.msi"

SectionEnd

# Uninstaller  =====================================================
Section "Uninstall"

SectionEnd


Function .onInit
  MessageBox MB_YESNO "This will install My Program. Do you wish to continue?" IDYES gogogo
    Abort
  gogogo:
FunctionEnd

#$WINDIR Windows directory