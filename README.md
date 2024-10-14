# Identity

This project serves as the Authentication and Token Granting Service.

## Table of Contents

- [Overview](#overview)
- [Documentation](#documentation)
- [Event Specifications](#event-specifications)

## Overview
Using OpenIddict this service has implemented issuing Access Tokens using the Client Credentials flow.

Packages needed for OpenIddict to work with AspNet and EntityFrameworkCore
```csharp
<PackageReference Include="OpenIddict" Version="5.3.0" />
<PackageReference Include="OpenIddict.AspNetCore" Version="5.3.0" />
<PackageReference Include="OpenIddict.EntityFrameworkCore" Version="5.3.0" />
```

## Documentation
[OpenIddict](https://documentation.openiddict.com)

## Event Specifications

### Initial Connection
The service expects the Redirection URI to be added to the Query Parameters of the initial Connection Request.
```web
https://server_uri?redirectionUri=your_redirection_uri
```
After the initial connection a Login Page will be displayed.

### Succesfull Login
Afte a succesfull login you will be redirected to your Redirection URI with the Access Token added as a Query Parameter.
```web
http://your_redirect_uri?token=your_access_token
```

