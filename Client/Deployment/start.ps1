[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Management.IntegrationServices")

$g_ssisConnectionString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;"
$g_sisCatalogPassword = "Password01!"
$g_ssisProjectName = "Diamond"
$g_ssisProjectFileName = "Diamond.ispac"
$g_ssisProjectEnvironments = @("SalesStartDate", "SalesEndDate")

Function Init-Web-Eventlog {
    $logExists = Get-EventLog -List | Where-Object { $_.Source -eq "Diamond Client Web" -and $_.LogName -eq "Application" }
    If (! $logExists) {
        New-EventLog -LogName "Application" -Source "Diamond Client Web"
    }
}

Function Init-SSIS-Project(
    [System.String]$projectName,
    [System.String]$projectFileName,
    [System.String[]]$environmentNames) {

    # connect to SSIS
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $g_ssisConnectionString
    $integrationServices = New-Object Microsoft.SqlServer.Management.IntegrationServices.IntegrationServices $sqlConnection

    # create catalog
    $catalog = $integrationServices.Catalogs["SSISDB"]
    If (! $catalog) {
        $catalog = New-Object Microsoft.SqlServer.Management.IntegrationServices.Catalog ($integrationServices, "SSISDB", $g_ssisCatalogPassword)
        $catalog.Create()
    }

    # create folder
    $folder = $catalog.Folders["Diamond"]
    if (! $folder)
    {
        $folder = New-Object Microsoft.SqlServer.Management.IntegrationServices.CatalogFolder ($catalog, "Diamond", "")            
        $folder.Create()  
    }

    # deploy project
    [byte[]] $projectFile = [System.IO.File]::ReadAllBytes($projectFileName)
    $folder.DeployProject($projectName, $projectFile)
    $project = $folder.Projects[$projectName]

    # create environments
    ForEach ($environmentName in $environmentNames) {
        $environment = $folder.Environments[$environmentName]

        if (! $environment)
        {
            $environment = New-Object Microsoft.SqlServer.Management.IntegrationServices.EnvironmentInfo ($folder, $environmentName, "")
            $environment.Create()            
        }

        $ref = $project.References[$environmentName, $folder.Name]
        if (! $ref)
        {
            $project.References.Add($environmentName, $folder.Name)
            $project.Alter() 
        }
    }
}

# run this script
# deploy database
  # generate the data of the date dimension
# deploy web site
  # modify the connection strings
  # modify the appSettings
# deploy SSAS by running deployment wizard
  # modify the data source
# deploy SSIS
    # create catalog
    # create folder with name Diamond
    # create environments
    # deploy SSIS project
    # modify the project parameters
    # modify the connections
# create SQL jobs
    # create job for SSIS packages (refer to source code for job names)
# create an Excel file which connect to SSAS (users only have read permission)

# ------------------------------------------------------------------------------#

# deploy SSRS
    # modify the data sources

Init-Web-Eventlog
# we manually deploy SSIS now
# Init-SSIS-Project $g_ssisProjectName $g_ssisProjectFileName $g_ssisProjectEnvironments