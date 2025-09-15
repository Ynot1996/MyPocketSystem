# MyPocket - Personal Finance Management System

MyPocket is a comprehensive web application designed to help users manage their personal finances effectively. It provides a suite of tools for tracking expenses, setting saving goals, and gaining insights into spending habits. The system also includes an administrative backend for user and subscription management.

**Live Demo:** [https://mypocket-cd-30212629511-asia-east1.run.app/](https://mypocket-cd-30212629511-asia-east1.run.app/)

## Key Features

* **User Authentication:** Secure user registration and login functionality.
* **Transaction Management:**
    * Users can create, view, update, and delete their financial transactions.
    * Transactions can be categorized for better financial tracking.
    * View transactions on a user-specific basis.
* **Saving Goals:**
    * Allows users to set and track their saving goals to motivate them to save money.
    * Full CRUD (Create, Read, Update, Delete) functionality for saving goals.
* **User Dashboard:**
    * A personalized dashboard for each user, displaying recent transactions and important announcements.
* **Admin Panel:**
    * A dedicated area for administrators to manage the application.
    * **User Management:** Admins can view and manage all non-admin users.
    * **Subscription Management:** Admins can manage user subscription plans, including updating a user's plan and viewing their subscription history.

## Technologies Used

* **Backend:** ASP.NET Core, Entity Framework Core
* **Frontend:** ASP.NET Core MVC (Razor Views)
* **Database:** Google Cloud SQL for SQL Server
* **Authentication:** BCrypt.Net for password hashing
* **Containerization:** Docker
* **Deployment & Services:**
    * Google Cloud Run for serverless deployment.
    * Google Secret Manager for managing sensitive data.

## Getting Started

To get a local copy up and running, follow these simple steps.

### Prerequisites

* .NET SDK
* A compatible database server (e.g., SQL Server)
* Docker Desktop (optional, for local containerized development)

### Installation

1.  **Clone the repo**
2.  **Configure the database connection**
    * Update the connection string in `appsettings.json`. For production, this is managed via Google Secret Manager.
3.  **Run database migrations**
4.  **Run the application**
