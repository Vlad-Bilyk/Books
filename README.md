# 📚 Books (Entity Framework Core)

Console app (.NET 9) with SQL Server and Entity Framework Core (Code-First) for managing books.

## ✨ Features
- Code-First models: Book, **Genre, Author, Publisher
- Migrations to create/update DB
- Option 1: **Import** books from CSV (idempotent: no duplicates on re-import)
- Option 2: **Search** by filters from `filter.json`, print count/titles, save results to a timestamped file

## 🧰 Tech
- .NET 9 (Console)
- Entity Framework Core + SQL Server
- CSV parsing
- JSON filters (`filter.json`)

## ▶️ How to run
1. Update SQL Server connection string in `appsettings.json` (or `App.config`).
2. Apply migrations:
   ```bash
   dotnet ef database update

---

Developed as part of the **Foxminded .NET course** to gain experience in console application development.
