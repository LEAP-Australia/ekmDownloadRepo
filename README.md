# ekmDownloadRepo

## Usage

ekmDownloadRepo ekmHostAddress ekmUserName ekmUserPassword repoLocation localTargetDestination errorLog

## Requirements
Environment variable EKM_BASE needs to be set to locate binaries. Only required for build. 

Environment variable JAVA_HOME should be set - required for runtime. 

The working directory needs the dll required to run C# application (see ANSYS Help). A copy of $EKM_BASE/ekm-client/lib needs to be available in the working directory required for run-time. 


