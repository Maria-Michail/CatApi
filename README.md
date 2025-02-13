# Cat Images API

## Requirements
- .NET 8
- SQL Server

## Setup Instructions

1. **Clone the repository** and navigate to the project folder.
2. **Create a database** in SQL Server and update the connection string in `appsettings.json`.
3. **Set up the database** by running the following command:
   ```bash
   dotnet ef database update

## To run the application:
   ```bash
   dotnet run

## Endpoints

- **POST /api/cats/fetch**  
  - Fetches and stores 25 cat images from The Cat API.

- **GET /api/cats/{id}**  
  - Retrieves a cat by its unique ID.

- **GET /api/cats?tag={tag}**  
  - Retrieves cats filtered by a specific tag with pagination.  
  - **Parameters**:
    - `tag`: (optional) The tag (e.g., "playful") used to filter cats.
    - `page` (optional): The page number (default: 1).
    - `pageSize` (optional): The number of cats per page (default: 10).
