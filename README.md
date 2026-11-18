# FarmGate Backend

ASP.NET Core backend for [FarmGate](https://github.com/Emon-Ashraf/FarmGate), a farmer-to-consumer Android marketplace developed as a Software Engineering bachelor's thesis project at National Research Tomsk State University.

This repository contains backend source code supporting the FarmGate Android client. The project demonstrates REST API development, role-based workflows, JWT-based authentication, Entity Framework Core, and PostgreSQL.

## Features

- Browse products by city and manage listings and pickup locations.
- Place pickup orders, review them as a farmer, and track order status.
- Upload product images, leave ratings, and report issues for administrative review.

## Getting Started

Install the .NET 8 SDK and provision PostgreSQL. Set `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Jwt__Issuer`, and `Jwt__Audience` in your local environment. Apply the included database migrations with an EF Core 8 CLI tool and start the API:

```sh
dotnet ef database update --project DAL --startup-project "FarmGate Backend"
dotnet run --project "FarmGate Backend"
```

Open `/swagger` at the URL printed by the development host. See the [media documentation](FarmGate%20Backend/Media/README.md) for image storage settings.

## Related Project

- [FarmGate Android application](https://github.com/Emon-Ashraf/FarmGate)

## Project Status

FarmGate is an academic software development project and is not presented as a production commercial service.

## Copyright and Usage

Copyright (c) 2026 Md Ashraful Islam Emon. All rights reserved, to the extent of the author's ownership rights.

This repository is publicly accessible for academic and professional portfolio review. **It is not distributed under an open-source license.**

Except for permissions provided by GitHub's Terms of Service or applicable law, no permission is granted to reproduce, modify, redistribute, sublicense, or commercially exploit the original source code or project assets without prior written permission from the applicable copyright holder.

Third-party dependencies, libraries, and assets remain subject to their own licenses and copyright notices.

## Author

Md Ashraful Islam Emon — [GitHub](https://github.com/Emon-Ashraf)
