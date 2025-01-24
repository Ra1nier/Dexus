# Guardian Vault Manager

Guardian Vault Manager (GVM) is a tool designed to help Destiny 2 players manage their in-game items efficiently. Similar to Destiny Item Manager (DIM), GVM offers a user-friendly interface to organize, transfer, and track your Destiny 2 inventory. This is really just a personal
project to help me learn C#, .NET Core and Blazor.

## Features

- **Item Management**: Easily move items between your characters and vault.
- **Loadout Creation**: Create and save loadouts for quick equipment changes. **Coming Soon TM**
- **Item Search and Filters**: Quickly find items using powerful search and filtering options. **Coming Soon TM**
- **Detailed Item Information**: View detailed stats and information for each item. **Coming Soon TM**
- **Easily Customizable**: Customise GVM to make it ours. **Coming Soon TM**
## Getting Started

### Prerequisites

- .NET Core 9.0 or later
- Visual Studio or any C# IDE *For now as I don't want to make a webserver (I'm lazy and broke), I am attempting to implement MUAI for a desktop application later on.*
- Your own Bungie.net application/API key. You can get one by registering a new application on the [Bungie Developer Portal](https://www.bungie.net/en/Application). You have to do this as I don't have a secure way
of including my own API key in the project.

### Installation

- Clone the repository into your IDE of choice and input your api key into a secrets file in the OAuthService Project with the following format:
```
{
  "BungieAPIKeys": {
    "API_Key": "[YOUR_API_KEY]",
    "Client_ID": "[YOUR_CLIENT_ID]",
    "Client_Secret": "[YOUR_CLIENT_SECRET]",
    "State": "[YOUR_STATE]"
  }
}
```
- State referes to a random string that is used to prevent [CSRF attacks](https://en.wikipedia.org/wiki/Cross-site_request_forgery). It is recommended to use a random string generator to create this value. **This is a temporary solution and will be handled by GVM in the future.**

### Notes

- This project as i have said is really just a personal project so i don't really expect people use it. If you do decide to use it for any reason (Why? when DIM exists), I would love feedback as to how I can be a better
developer as this is a learning exercise for me. I am still learning and would love to improve my skills.