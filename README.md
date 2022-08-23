# InstantSpockExecutionRunner
---
##### This is a utility created to execute Opkey Executions through its Spock Agent Capability on a fresh machine like Azure Pipeline. This can be extended to be used with Docker Images as well.

<br/>
<br/>

# Prerequisites
 - JDK 8 or 17(Open JDK or OracleJDK)
 - Browsers needs to be installed in the image(Chrome, MsEdge, Firefox). The drivers will be fetched from Opkeys Driver registry from AwsS3
 - dotnet core environment to build the project(optional)
   
 
```sh
InstantSpockExecutionRunner.exe "--environmentType=$(environmentType)" "--opkeyBaseUrl=$(opKeyBaseUrl)" "--sessionName=$(sessionName)" "--defaultPlugin=$(defaultPlugin)" "--build=$(build)" "--suitepath=$(suitePath)" "--browser=$(browser)" "--username=$(username)" "--apikey=$(apiKey)" "--project=$(project)"
```


### Example:
```bash
InstantSpockExecutionRunner.exe "--environmentType=PROD" "--opkeyBaseUrl=https://qa1.myopkey.com" "--sessionName=MySpockSession" "--defaultPlugin=Web" "--build=Build-One" "--suitepath=ProjectWorkspace/Folder1/Suite1" "--browser=Chrome" "--username=YOUR_OPKEY_EMAIL_ID" "--apikey=YOUR_OPKEY_API_KEY" "--project=Project_1"
```

### License
Free Software. Use at your own judgement. You may require licenses for OracleJDK and Opkey ofcourse.

### Opkey - https://www.opkey.com/

### Repository- https://github.com/anshumanchatterji/SpockAgentBuilds