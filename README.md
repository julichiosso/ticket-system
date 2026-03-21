# Ticket System (Helpdesk)

Full stack ticket management system built with ASP.NET Core and Vue.js, following Clean Architecture principles.

This system allows users to create, assign, and track support tickets, including real-time communication through an integrated chat system.

---

## Key Features

- JWT-based authentication and role-based authorization  
- Ticket lifecycle management (`create`, `assign`, `update status`)  
- Integrated real-time chat between users and operators  
- Administrative dashboard for ticket monitoring  
- RESTful API design  
- Automated testing (unit and integration tests)  

---

## Architecture

Backend designed using Clean Architecture:

- `Domain` → Business entities and rules  
- `Application` → Use cases and application logic  
- `Infrastructure` → Data access and external services  
- `API` → REST endpoints  

---

## Tech Stack

### Backend
- `C#`  
- `ASP.NET Core`  
- `Entity Framework Core`  
- `SQL Database`  
- `JWT Authentication`  

### Frontend
- `Vue.js`  
- `JavaScript`  
- `HTML` / `CSS`  
- `TailwindCSS`  

### Other
- `REST APIs`  
- `Clean Architecture`  
- `Git`  

---

## Real-Time Chat (Core Feature)

The system includes a built-in real-time chat module that allows users to communicate directly with operators and administrators within the platform.

- Direct communication without leaving the ticket system  
- Faster support resolution  
- Integrated into the ticket workflow  

---

## Testing

Includes automated tests covering:

- Business logic  
- Application services  
- API endpoints  

Run tests with:

```bash
dotnet test

## Running the Project

Clone the repository:

```bash
git clone https://github.com/julichiosso/TicketSystem.API.git

Configure the database connection in:

appsettings.json

Run migrations:

dotnet ef database update

Start the backend:

dotnet run

Start the frontend:

npm install
npm run dev
Project Structure
TicketSystem/
├── backend/
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   └── API/
│
└── frontend/
    ├── components/
    ├── pages/
    └── services/
