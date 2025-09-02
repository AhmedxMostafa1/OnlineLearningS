# 📘 Online Learning System

A **web-based learning platform** built with **ASP.NET Core MVC** that allows students to enroll in courses, instructors to manage course content, and admins to oversee the platform.  
The system includes role-based access, quizzes, and payment integration.

---

## 🚀 Features
- 👩‍🎓 **Student Role**
  - Register/login and manage profile
  - Enroll in courses
  - Take quizzes and view results
  

- 👨‍🏫 **Instructor Role**
  - Create and manage owned courses
  - Add modules, lessons, and quizzes
  

- 👨‍💼 **Admin Role**
  - Manage users (students/instructors)
  - Approve instructor applications
  - Dashboard with search and details

- 💳 **Payments**
  - Students can pay for courses
  

---

## 🛠 Tech Stack
- **Backend:** ASP.NET Core MVC (.NET 6/7/8)  
- **Frontend:** Razor Views, Bootstrap  
- **Database:** SQL Server, Entity Framework Core  
- **Language:** C#  

---

## ⚙️ Installation & Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/AhmedxMostafa1/OnlineLearningS.git
   cd OnlineLearningS/OnlineLearning


## Set Up Database

Update the connection string in appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=OnlineLearning;Trusted_Connection=True;MultipleActiveResultSets=true"
}

## Apply Migrations
dotnet ef database update

## Run the Project
dotnet run

## 📂 Project Structure
OnlineLearningS/
│── OnlineLearning.sln          # Solution file
│── OnlineLearning/             # Main project
│   ├── Controllers/            # MVC Controllers
│   ├── Models/                 # Entity models & DbContext
│   ├── Views/                  # Razor views
│   ├── wwwroot/                # Static files (CSS, JS, images)
│   ├── appsettings.json        # Configurations
│   ├── Program.cs              # Entry point
│   └── OnlineLearning.csproj   # Project file

## 👨‍💻 Author
Ahmed Darwish
Mohamed Hassona