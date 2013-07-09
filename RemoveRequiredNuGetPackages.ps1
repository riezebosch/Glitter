###########################################################################################################################
# Remove the nuGet packages from the projects, so we can get the binaries
###########################################################################################################################
Write-Host "Removing nuGet packages.... "
Write-Host "Depending on your system, there may be errors in the removal of these nuGet packages."
Write-Host "Please ignore errors for a moment."

Uninstall-Package GraphSharp -Project Glitter
Uninstall-Package DotNetZip -Project Glitter
Uninstall-Package DotNetZip -Project Glitter.Test
Uninstall-Package WPFExtensions -Project Glitter

Write-Host "Done removing packages.  Please pay attention to errors now."

