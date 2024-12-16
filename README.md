# GuardianVaultManager

GuardianVaultManager (GVM) is a tool designed to help Destiny 2 players manage their in-game items efficiently. Similar to Destiny Item Manager (DIM), GVM offers a user-friendly interface to organize, transfer, and track your Destiny 2 inventory.

## Features

- **Item Management**: Easily move items between your characters and vault.
- **Loadout Creation**: Create and save loadouts for quick equipment changes.
- **Item Search and Filters**: Quickly find items using powerful search and filtering options.
- **Detailed Item Information**: View detailed stats and information for each item.
- **OAuth Authentication**: Securely authenticate and access your Destiny 2 profile using Bungie's OAuth.

## Getting Started

### Prerequisites

- .NET Core 9.0 or later
- Visual Studio or any C# IDE

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/yourusername/GuardianVaultManager.git
    ```
2. Navigate to the project directory:
    ```sh
    cd GuardianVaultManager
    ```
3. Restore the project dependencies:
    ```sh
    dotnet restore
    ```
4. Set up your Bungie.net API Key in the `appsettings.json` file:
    ```json
    {
      "Bungie": {
        "API_Key": "YOUR_API_KEY",
        "ClientId": "YOUR_CLIENT_ID",
        "ClientSecret": "YOUR_CLIENT_SECRET"
      }
    }
    ```
5. Run the application:
    ```sh
    dotnet run
    ```

## Usage

### Authentication

1. Open GuardianVaultManager.
2. Click on the "Sign In to Bungie.com" button.
3. Follow the OAuth authentication process to grant GVM access to your Destiny 2 profile.

### Managing Items

- **Move Items**: Select items and transfer them between your characters and vault.
- **Create Loadouts**: Save and manage multiple loadouts for quick equipment swaps.
- **Search and Filter**: Use the search bar and filters to find specific items quickly.

## To Do

- **Fix issue with Callback making multiple attemps at sign in. Causing issues with token creation and storage**