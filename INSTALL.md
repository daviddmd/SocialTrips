# Technical Details

The backend of this application is an ASP.NET Core API, that runs on ASP.NET Core 6. The frontend of this application is
an Angular 13 web application, that runs on NodeJS.

The flow on the trips management interface on managing the daily itinerary requires interaction with a Google Maps
widget, that makes use of [agm - Angular Google Maps](https://github.com/sebholstein/angular-google-maps) for searching
for locations and presenting the itinerary for the current day.

The itinerary is made up of several activities, separated by the transportation that connects pairs of activities, that
may be car, bike, public transport or by foot.

Upon searching for a location, it contacts the backend, which makes use of the Google Maps Distance Matrix API to return
all available modes of transportation, alongside with the steps of transport, distance and expected time, which is
presented on the frontend as a table with the available transportation methods, which the trip administrators may pick
one, starting at the previous location, at a set beginning time, and ends at the expected ending time, on the
destination location.

The next activity is created alongside the transport activity. All activities, including transport activities have a
cost associated with it, and transport activities have a distance associated with it (the Distance Matrix distance), as
well as description/instructions, which for the transport activities are the Distance Matrix instructions by default,
which may be modified. Upon confirming the creation of an activity, the associated transport is automatically created.
Upon the removal of an activity, the transport that connected it to the previous one is automatically removed.

![Itinerary Map Map](files/itinerary.jpg?raw=true "Itinerary Map")
*Itinerary Map*

![Transport Creation Prompt](files/transport_create.png?raw=true "Transport Creation Prompt")
*Transport Creation Prompt*

![Activity Creation Prompt](files/activity_create.png?raw=true "Activity Creation Prompt")
*Activity Creation Prompt*

Therefore, an [API key](https://developers.google.com/maps/documentation/javascript/get-api-key) that supports Google
Maps Geocoding, Directions, Places and Distance Matrix., which are configured on the
backend (`Backend/appsettings.json`) and frontend (`Frontend/src/environments/environment.ts` ) respectively. The
responsible backend class for serving Google Maps routes is [GoogleMapsHelper](Backend/Helpers/GoogleMapsHelper.cs).

The application has support for users creating/editing/removing posts from and to trips, which are aggregated to the
groups of the respective trips. Those posts may have attachments, which may be images or videos. The uploaded media
isn't uploaded to the server, but rather to Google Cloud Storage, and then the images URLs are stored in the database
and served to the frontend. For usage of Google Cloud Storage a Google Cloud Storage bucket needs to be created, as well
as a [service key](https://cloud.google.com/iam/docs/creating-managing-service-account-keys). The Google Service Key may
be set at `Backend/appsettings.json` at the `Google:ServiceKey` section, as well as the Bucket Name and API Key. The
responsible class for managing files in the Cloud Storage bucket
is [GoogleCloudStorageHelper](Backend/Helpers/GoogleCloudStorageHelper.cs). It also makes use of
the [FileHelper](Backend/Helpers/FileHelper.cs) class for validating uploaded files, such as file extensions and file
size.

The application sends out emails to its user on many occasions, such as confirming accounts after registration,
recovering passwords, changing email address and deactivating accounts. It doesn't send email on its own (such as using
an SMTP server), but rather with [Mailjet](https://www.mailjet.com/), although other services may easily be configured
on [EmailHelper](Backend/Helpers/EmailHelper.cs). As such, the Mailjet/Mail configuration is set
at `Backend/appsettings.json` on the `EmailSettings` section. Two email addresses are supported, one for sending emails
for a single user, and another for bulk sending of emails such as newsletters or updates of trips/groups.

The application is designed to store data
in [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) RDBMS. The Entity Framework
Core Tools should be [installed](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
with `dotnet tool install --global dotnet-ef`. To generate migrations after changing/adding/removing an Entity Model,
run `dotnet ef migrations add MigrationName`and `dotnet ef database update` to apply the migrations. This repository
comes with a predefined migration that creates the database with all required entity instances, therefore creating a new
one isn't needed, but rather just applying the migration with the aforementioned command. It may also be used with Azure
SQL Server, and the procedure to use it both locally and remotely is the same, by setting up the connection string
in `Backend/appsettings.json`, in the `ConnectionStrings:BackendAPIContext` configuration section.

Finally, a JSON Web Token (JWT) needs to be generated in order to generate the user session tokens to be used by the
frontend users. It should be placed at `Backend/appsettings.json` in the `JWT:Secret` section. The domain the
application will be deployed on should also be set at the `Domain` section in the configuration file for emails.

# Installation Instructions

After getting the required API/Service Keys, the backend may be set up on a Linux or Windows Server. Instructions for
Ubuntu Server are provided. An example domain will be used to demonstrate the virtual host configuration on the web
server, and that domain should have the following records set up:

- A and AAAA records pointing to the server IP
- CNAME record to redirect the www domain to the non-www domain
- 3 TXT records to authenticate the domain with the email service to allow sending emails (adding the SPF and DKIM
  records to allow sending emails to Google and Microsoft inboxes)

Install dotnet and sql server, and configure them, and create a database with the required authentication credentials
and database name, settings them up in `Backend/appsettings.json` as mentioned before.

After deploying the virtual machine/VPS with Ubuntu Server and installing and configuring the nginx web server, create a
server block for the domain with a defined webroot, alongside with certbot for HTTPS. The application is configured to
proxy requests at port 5000 with Kestrel at [Startup.cs](Backend/Startup.cs), so add the api endpoint location at the
nginx server block.

```
    location /api {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
```

Therefore, the final server block should look like:

```
server {
        root /var/www/example;
        index index.html index.htm index.nginx-debian.html;

        server_name example.com www.example.com;

        location / {
                try_files $uri $uri/ =404;
        }
    location /api {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
    listen [::]:443 ssl ipv6only=on; # managed by Certbot
    listen 443 ssl; # managed by Certbot
    ssl_certificate /etc/letsencrypt/live/example.com/fullchain.pem; # managed by Certbot
    ssl_certificate_key /etc/letsencrypt/live/example.com/privkey.pem; # managed by Certbot
    include /etc/letsencrypt/options-ssl-nginx.conf; # managed by Certbot
    ssl_dhparam /etc/letsencrypt/ssl-dhparams.pem; # managed by Certbot
}
server {
    if ($host = www.example.com) {
        return 301 https://$host$request_uri;
    } # managed by Certbot
    if ($host = example.com) {
        return 301 https://$host$request_uri;
    } # managed by Certbot
        listen 80;
        listen [::]:80;
        server_name example.com www.example.com;
    return 404; # managed by Certbot
}
```

Either publish the backend program on a CI/CD pipeline and retrieve the DLL to the server, or
use `dotnet publish --configuration Release` on the machine to build the DLL. It can be run
with `dotnet $HOME/SocialTrips/Backend/bin/Release/net5.0/BackendAPI.dll` (or the respective path) or with a systemd
service:

```
[Unit]
Description=systemd unit for SocialTrips

[Service]
WorkingDirectory=/home/user/SocialTrips/Backend
ExecStart=/usr/bin/dotnet /home/user/SocialTrips/Backend/bin/Release/net5.0/BackendAPI.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-social-trips
User=user
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

For the frontend, a CI/CD pipeline should be set up to automatically [build](https://angular.io/guide/deployment) the
latest release (from tag or master) and deploy to the server's web root (in this example set at `/var/www/example`). The
configuration of the production variables of the Angular application will be set
on [environment.prod.ts](Frontend/src/environments/environment.prod.ts).

A different subdomain may be used for the API (or even a different server altogether), as long as such is defined in the
environment configuration file and on the domain A and AAAA DNS records.