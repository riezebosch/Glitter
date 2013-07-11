###########################################################################################################################
# Remove the nuGet packages from the projects, so we can get the binaries
###########################################################################################################################
.\RemoveRequiredNuGetPackages.ps1

###########################################################################################################################
# Install the nuGet packages for the projects
###########################################################################################################################
Write-Host "Installing nuGet packages.... "
#Install-Package GraphSharp -Project Glitter
Install-Package DotNetZip -Project Glitter
Install-Package DotNetZip -Project Glitter.Test
Install-Package WPFExtensions -Project Glitter

Write-Host "Done installing nuGet packages.... "
