# QueryAnalyzer

This is an API that allows users to execute SQL queries against a local MySQL database, analyze query performance, and receive optimization suggestions. The frontend can be found here: https://github.com/brandonkongwe/query-analyzer

## Features

- Execute SQL queries and retrieve results.
- Measure query execution time.
- Analyze query execution plans using EXPLAIN.
- Provide optimization suggestions based on query analysis.

## Prerequisites
-	.NET 9.0 SDK
- Visual Studio 2022 or any other compatible IDE
- MySQL database

## Setup

1. Clone the repository:
   
   ```bash
    git clone https://github.com/brandonkongwe/QueryAnalyzer.git
    cd QueryAnalyzer
   ```

2. Configure the Database Connection:
   
    Update the appsettings.json file with your database connection details:

    ```bash
      "Database": {
        "Type": "mysql"
        "ConnectionString": "Server=your_server;Database=your_db;User=your_user;Password=your_password;"
      }
    ```

3. Build and Run the Application

    ```bash
    dotnet build
    dotnet run
    ```

## API Endpoints

### Execute Query
-	URL: POST /api/query/execute
-	Request Body:
  
    ```bash
      {
      "query": "SELECT * FROM your_table"
      }
    ```
- Response:
  
  ```bash
    {
    "results": [ ... ],
    "executionTime": 123.45,
    "queryPlan": " ... ",
    "suggestions": [ ... ]
    }
  ```

## Structure

  ```bash
  QueryAnalyzer/
  ├── Controllers/
  │   └── QueryController.cs          # API endpoints
  ├── Services/
  │   └── DatabaseService.cs          # Database operations and query analysis
  ├── appsettings.json                # Configuration file
  ├── Program.cs                      # Application entry point
  └── README.md                       # Project documentation
  ```

## Usage

1. Execute a Query

    Send a `POST` request to `/api/query/execute` with the SQL query in the request body.
    
    Example Request:
    
    ```bash
    {
      "query": "SELECT * FROM Employees WHERE Department = 'Sales'"
    }
    ```

    Example Response:

   ```bash
   {
    "results": [
      {
        "id": 1,
        "name": "John Doe",
        "department": "Sales"
      }
    ],
    "executionTime": 123.45,
    "queryPlan": "id: 1; select_type: SIMPLE; table: Employees; partitions: ; type: ALL; possible_keys: ; key: ; key_len: ; ref: ; rows: 20; filtered: 100; Extra: ; ",
    "suggestions": [
      Suggestion: A full table scan was detected. Consider adding indexes to improve performance.
    ]
   }
   ```
