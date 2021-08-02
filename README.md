# IAutoM8

## Installation
```
1. Clone the repo
    git clone https://user@bitbucket.org/classicinfo/modus8.git
	(make sure git is installed globally https://git-scm.com/download/win)
2. run "check java version.bat" with admin rigths
    if you get Correct version in console you can go to next step
    otherwise you need to download and install java se development kit 8u221(https://www.oracle.com/technetwork/java/javase/downloads/2133151)
3. download and install neo4j server (https://neo4j.com/download-thanks/?edition=community&release=3.3.9)
    Find the zip file you just downloaded and right-click, extract all.
    Place the extracted files in a permanent home on your server, for example D:\neo4j\. The top level directory is referred to as NEO4J_HOME.
    To install Neo4j as a service use:
    run in command console <NEO4J_HOME>\bin\neo4j install-service.
    Visit http://localhost:7474 in your web browser.
    Connect using the username 'neo4j' with default password 'neo4j'. Change password to '111'.
4. dotnet restore
5. Install global dependencies (make sure npm 6.x.x is installed globally https://nodejs.org/uk/)
    npm http-server @angular/cli -g
6. move to web project IAutoM8 and run:
     npm install
7. Run the app (Development mode):
    * If you are running the project for first time:
	  - create local database from visual studio DB explorer, use that connection string in appsettings.dev.json
      - npm run update-db:dev (this will create database and apply migrations)
   
    Launch Visual Studio 2017 and hit F5
8. run 
     npm run dev-watch
    so that webpack will watch your changes of the source code and automatically rebuild 

```


### Creating migrations after adding new entities
1. Add new migration : dotnet ef migrations add your_name_of_migration (run on IAutoM8.Repository)
2. update DB schema based on new migrations npm run update-db:dev (run on IAutoM8 project)