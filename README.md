# Nexus Destiny 2 Manager

Nexus is a tool designed to help Destiny 2 players manage their in-game items efficiently. This is its current functionality but more is planned. Nexus offers a user-friendly interface to organize, transfer, and track your Destiny 2 inventory. This is really just a personal
project to help me learn C#, .NET and Blazor.

## Features

### Guardian Vault Manager
- **Item Management**: Easily move items between your characters and vault.
- **Loadout Creation**: Create and save loadouts for quick equipment changes. **Coming Soon TM**
- **Item Search and Filters**: Quickly find items using powerful search and filtering options. **Coming Soon TM**
- **Detailed Item Information**: View detailed stats and information for each item. **Coming Soon TM**
- **Easily Customizable**: Customise GVM to make it yours. **Coming Soon TM**

### <i>More Managers on their way!</i>

## Getting Started

### Prerequisites

- .NET Core 9.0 or later
- Any C# IDE. *Temp solution for now. I am attempting to implement MUAI or Web App with Blazor for a desktop application later on.*
- Your own Bungie.net application/API key. You can get one by registering a new application on the [Bungie Developer Portal](https://www.bungie.net/en/Application). You have to do this as I don't have a secure way
of including my own API key in the project or running a backend server to route requests.

### Installation

- Clone the repository into your IDE of choice and create a JSON file at the following location:
```angular2html
&#123;Solution Location&#125;/GuardianVaultManager/GuardianVaultManager/bin/Debug/net9.0/OAuthProfile
```
- Then make a JSON file called "UserBungieAPITokens" and input the following Bungie API information with this format:
```
{
  "BungieAPIKeys": {
    "API_Key": "[YOUR_API_KEY]",
    "Client_ID": "[YOUR_CLIENT_ID]",
    "Client_Secret": "[YOUR_CLIENT_SECRET]",
  }
}
```
- <i>This will be handled in a better way in the future but this is the current set up.
### Notes

- This project as I have said is really just a personal project, so I don't really expect people use it. If you do decide to use it for any reason (Why? when DIM exists), I would love feedback as to how I can be a better
developer as this is a learning exercise for me. I am still learning and would love to improve my skills.

## WARNING
 - I am not a front end developer. I am not a good front end designer. Even though I am using MudBlazor for all the front end, the GUI still looks terrible. Use DIM if you want a better GUI.