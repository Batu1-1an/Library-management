# Library Management System

A modern, full-featured Library Management System built with **ASP.NET Core** and **MySQL**. This project helps libraries manage their books, members, reservations, and borrowing history efficiently.

---

## Features

- User registration and authentication
- Role-based access control (Admin, Member)
- Book catalog management (add, edit, delete, search)
- Book reservation and borrowing
- Penalty management for overdue books
- Dashboard with analytics and reports
- Responsive UI with Razor Pages and Bootstrap

---

## Requirements

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) or newer
- Visual Studio 2022 or Visual Studio Code
- MySQL Server 8.0+
- Recommended VS Code Extensions:
  - C# (ms-dotnettools.csharp)
  - C# Dev Kit
  - NuGet Package Manager
  - SQL Server (ms-mssql.mssql)

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Batu1-1an/Library-management.git
```

### 2. Configure the Database

- Ensure MySQL Server is running.
- Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LibraryManagementSystem;User=root;Password=YOUR_PASSWORD;"
  }
}
```

### 3. Apply Migrations

```bash
dotnet ef database update
```

### 4. Build and Run

```bash
dotnet restore
dotnet build
dotnet run
```

Visit `https://localhost:5001` or `http://localhost:5000` in your browser.

---

## Contribution Guidelines

We welcome contributions! To get started:

1. **Fork** the repository.
2. **Create a branch** for your feature or fix.
3. **Commit** your changes with clear messages.
4. **Push** to your fork.
5. **Open a Pull Request** describing your changes.

Please ensure your code follows best practices and includes appropriate tests.

---

## License

This project is licensed under the MIT License. See the [LICENSE](../LICENSE) file for details.

---

## Contact

For questions or suggestions, please open an issue or contact the maintainer via GitHub.

---

## Acknowledgements

- Built with ASP.NET Core
- Uses Entity Framework Core
- Frontend styled with Bootstrap

---