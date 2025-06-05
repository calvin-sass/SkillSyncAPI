# SkillSyncAPI

SkillSyncAPI is a modern .NET 8 Web API backend for a service marketplace platform, enabling users to offer, discover, and book professional services. It features robust authentication, secure payments, real-time notifications, and media management.

## Features
- User & Seller registration/login with JWT authentication
- Role-based access control (User, Seller)
- Service listing, editing, and management
- Booking and order management
- Reviews and ratings
- Real analytics for sellers (orders, cancellations, ratings, review count)
- Payment integration with Stripe
- Image and media uploads via Cloudinary
- Real-time notifications
- Swagger/OpenAPI documentation

## Tech Stack
- **.NET 8.0** (ASP.NET Core Web API)
- **Entity Framework Core** (SQL Server)
- **ASP.NET Identity** (Role-based auth)
- **JWT Bearer Authentication**
- **Cloudinary** (media uploads)
- **Stripe** (payments)
- **Swashbuckle** (Swagger docs)

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)
- (Optional) Cloudinary & Stripe accounts for full functionality

### Setup
1. **Clone the repository:**
   ```bash
   git clone <your-repo-url>
   cd SkillSyncAPI
   ```
2. **Configure the database:**
   - Update the connection string in `SkillSyncAPI/appsettings.json` under `ConnectionStrings:DefaultSQLConnection`.
3. **Configure Cloudinary & Stripe:**
   - Add your Cloudinary and Stripe API keys to `appsettings.json`.
4. **Apply migrations:**
   ```bash
   dotnet ef database update --project SkillSyncAPI/SkillSyncAPI.csproj
   ```
5. **Run the API:**
   ```bash
   dotnet run --project SkillSyncAPI/SkillSyncAPI.csproj
   ```
6. **Access Swagger UI:**
   - Navigate to `https://localhost:5001/swagger` (or the port shown in your console)

## Project Structure
- `Controllers/` — API endpoints
- `Data/` — Database context
- `Domain/` — Entities and core models
- `Repositories/` — Data access logic
- `Services/` — Business logic
- `Migrations/` — EF Core migrations
- `Middleware/` — Custom middleware

## Contributing
Pull requests are welcome! For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](LICENSE)

---

For any questions or support, please open an issue on this repository.
