# Development Instructions

## Required Environment Variables

Before running this project, make sure to set up the following environment variables in user secrets:
```bash
dotnet user-secrets set "Discord:Token" "your_discord_token"
dotnet user-secrets set "MongoDB:Uri" "your_mongodb_uri"
dotnet user-secrets set "MongoDB:Database" "your_database_name"
dotnet user-secrets set "StableDiffusion:Uri" "your_stablediffusion_uri"
```

## Running This Project
You can run this project by using command `dotnet run` or `dotnet watch`