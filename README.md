Based on AsadSahi`s project implemented possiblity to upload contacts from Excel files.

To run project do the following steps:
1. Delete sln file if any.
2. Go to ContactsList folder - cd ContactsList
3. Restore packages - dotnet restore
4. Install global depencies using command: npm install protractor rimraf http-server @angular/cli -g
5. Restore packages for front-end: npm install protractor rimraf http-server @angular/cli -g
6. dotnet ef database update (or even better use another command if you have already create db: dotnet run dropdb migratedb seeddb)
7. Build front-end: npm run build:prod
8. Set config as prod or dev: (set ASPNETCORE_ENVIRONMENT=Production or set ASPNETCORE_ENVIRONMENT=Development)
9. dotnet run