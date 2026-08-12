# 🏥 Smart Hospital Management System — Backend API

A backend API for a Smart Hospital Management System built using **C# and ASP.NET Core Web API** with **PostgreSQL** as the database.

The API manages patients, doctors, appointments, queues, consultations, dashboards, and reports and provides the backend services used by the hospital management frontend.

---

## 🎯 Main Objective

The main objective of this project is to make hospital operations more organized and reduce the difficulties involved in manual patient, appointment, and queue management.

In a traditional hospital workflow, patients may have to wait without clear information about their token or expected turn. At the same time, hospital staff need to manage patient records, doctors, appointments, and queues.

This project brings these activities into a single digital system.

The system is designed to:

- Manage patient records digitally
- Manage doctors and their availability
- Schedule and manage appointments
- Generate and manage patient tokens
- Organize the hospital waiting queue
- Handle priority-based patients
- Provide estimated waiting-time information
- Track consultations
- Provide dashboard and report information
- Store hospital data in PostgreSQL
- Connect the frontend with the backend through REST APIs

The main focus is to improve **patient queue management, hospital data organization, and overall patient experience**.

---

## 📌 Project Overview

Smart Hospital Management System is a full-stack hospital management project developed to manage common hospital activities through a digital application.

The project includes:

- Patient Management
- Doctor Management
- Appointment Management
- Token & Queue Management
- Estimated Waiting Time
- Consultation Management
- Dashboard
- Reports
- PostgreSQL Database
- REST API
- Web-based Frontend

This repository contains the **backend API** of the complete system.

The frontend is maintained in a separate repository.

---

# ✨ Key Features

## 👨‍⚕️ Doctor Management

The system provides backend functionality for managing doctors.

- View doctors
- Add doctors
- Manage doctor information
- Manage doctor availability
- Track consultation information
- Associate doctors with patients and appointments

---

## 🧑‍🤝‍🧑 Patient Management

The patient module manages patient-related information.

- Patient registration
- View patient information
- Search patient records
- Update patient details
- Track patient status
- Manage patient tokens

---

## 📅 Appointment Management

The appointment module manages patient appointments.

- Create appointments
- View appointments
- Update appointment information
- Manage appointment status
- Associate patients with doctors
- Support appointment-based queue handling
- Appointment information/slip generation through the frontend

---

## 🎫 Queue & Token Management

Queue management is one of the main parts of the project.

The system provides:

- Patient token management
- Waiting queue
- Priority-based patient handling
- Calling the next patient
- Patient queue status
- Consultation status tracking

The purpose is to make the patient flow more organized and reduce confusion during hospital visits.

---

## ⏱️ Estimated Waiting Time

The system provides estimated waiting-time information to help patients understand their expected waiting period.

This helps improve:

- Queue transparency
- Patient experience
- Waiting-time management
- Hospital workflow

---

## 🩺 Consultation Management

The system maintains consultation-related information between doctors and patients.

Consultation information can also be used for maintaining consultation history and generating reports.

---

## 📊 Dashboard & Reports

The backend provides APIs used by the dashboard and reporting sections of the application.

The system provides information related to:

- Patients
- Doctors
- Appointments
- Queue
- Consultations
- Hospital activity
- Reports

---

# 🏗️ System Architecture

```text
                    ┌──────────────────────────┐
                    │   Smart Hospital UI      │
                    │   HTML / CSS / JavaScript│
                    └────────────┬─────────────┘
                                 │
                                 │ REST API
                                 ▼
                    ┌──────────────────────────┐
                    │   ASP.NET Core Web API   │
                    │        C# Backend        │
                    └────────────┬─────────────┘
                                 │
                ┌────────────────┼────────────────┐
                │                │                │
                ▼                ▼                ▼
        ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
        │   Patients   │ │   Doctors    │ │ Appointments │
        └──────────────┘ └──────────────┘ └──────────────┘
                │                │                │
                └────────────────┼────────────────┘
                                 ▼
                    ┌──────────────────────────┐
                    │       PostgreSQL         │
                    │        Database          │
                    └──────────────────────────┘
```

---

# 🔄 Project Evolution

The Smart Hospital Management System was initially developed as a **C++ console-based Hospital Management System**.

The original C++ version focused on implementing the core hospital workflow and data structures, including:

- Patient registration
- Doctor management
- Patient tokens
- Priority-based queue
- Appointment handling
- Waiting queue
- Consultation management

As the project evolved, the system was extended into a web-based application with a separate frontend, backend API, and database.

### Development Journey

```text
C++ Console Hospital Management System
                    │
                    ▼
          ASP.NET Core Web API
                    │
                    ▼
               PostgreSQL
                    │
                    ▼
         HTML + CSS + JavaScript
                    │
                    ▼
             Docker + Render
```

The original C++ implementation helped establish the core hospital workflow and queue-management logic. The current version extends these concepts into a web-based hospital management application.

---

# 🛠️ Technology Stack

## Programming Languages

- **C++** — Original console-based implementation
- **C#** — Backend development
- **JavaScript** — Frontend development
- **SQL** — Database operations

## Backend

- **ASP.NET Core Web API**
- **REST APIs**
- **C#**

## Database

- **PostgreSQL**

## Frontend

- **HTML5**
- **CSS3**
- **JavaScript**

## Tools & Deployment

- **Git**
- **GitHub**
- **Docker**
- **Render**

---

# 📂 Backend Project Structure

```text
SmartHospitalAPI/
│
├── Controllers/
│   ├── AppointmentsController.cs
│   ├── DashboardController.cs
│   ├── DoctorsController.cs
│   ├── PatientsController.cs
│   ├── QueueController.cs
│   └── ReportsController.cs
│
├── Models/
│
├── Properties/
│
├── Program.cs
├── Dockerfile
├── SmartHospitalAPI.csproj
├── SmartHospitalAPI.http
├── .gitignore
└── README.md
```
├── .gitignore
└── README.md

# 🔌 API Modules

The Smart Hospital Management System backend is divided into multiple API modules, with each module responsible for a specific hospital operation.

| Module | Purpose |
|---|---|
| Patients | Patient registration, patient records, status and token management |
| Doctors | Doctor information, specialization and availability |
| Appointments | Appointment creation, management and scheduling |
| Queue | Token generation, waiting queue and patient flow management |
| Dashboard | Hospital statistics and dashboard information |
| Reports | Patient, doctor, appointment and hospital reports |

---

# 🌐 REST API

The backend follows REST API principles and uses standard HTTP methods for communication between the frontend and backend.

## HTTP Methods

```text
GET       Retrieve data
POST      Create new data
PUT       Update existing data
DELETE    Delete data
```

---

# 🔌 Main API Modules

The Smart Hospital Management System backend is organized into multiple REST API modules. Each module handles a specific part of the hospital management workflow.

| API Module | Purpose |
|---|---|
| Patients | Patient registration, records, status and token management |
| Doctors | Doctor information, specialization and availability |
| Appointments | Appointment creation, scheduling and management |
| Queue | Token generation, waiting queue and patient flow |
| Dashboard | Hospital statistics and operational information |
| Reports | Patient, doctor, appointment and hospital reports |

## API Endpoints

```text
/api/patients
/api/doctors
/api/appointments
/api/queue
/api/dashboard
/api/reports
```

These API endpoints allow the frontend application to communicate with the ASP.NET Core backend and perform hospital management operations.

# 🗄️ PostgreSQL Database

PostgreSQL is used as the primary relational database for the Smart Hospital Management System.

The database provides persistent storage for hospital-related information and allows the backend API to manage patient, doctor, appointment, consultation and user data.

## Main Database Tables

```text
patients
doctors
appointments
consultation_history
users

# 🐳 Docker

The Smart Hospital Management System backend includes a `Dockerfile` for containerized deployment.

Docker packages the ASP.NET Core Web API and its required dependencies into a container, making the application easier to build, run and deploy across different environments.

## Dockerfile

The project contains a `Dockerfile` in the root directory:

```text
SmartHospitalAPI/
│
├── Dockerfile
├── Program.cs
├── SmartHospitalAPI.csproj
├── Controllers/
├── Models/
├── Properties/
├── SmartHospitalAPI.http
├── .gitignore
└── README.md
```

## Build the Docker Image

Run the following command from the project directory:

```bash
docker build -t smart-hospital-api .
```

## Run the Docker Container

```bash
docker run -p 8080:8080 smart-hospital-api
```

The API will be available on the configured local port.

## Docker Workflow

```text
Source Code
     │
     ▼
 Dockerfile
     │
     ▼
 Docker Image
     │
     ▼
Docker Container
     │
     ▼
ASP.NET Core Web API
```

Docker is used as part of the deployment setup and helps maintain a consistent application environment.

---

# 🚀 Deployment

The backend API is deployed using **Render**.

### Backend API

https://smarthospitalapi.onrender.com

The deployed backend API provides the services required by the Smart Hospital frontend.

The application is containerized using Docker and deployed on Render for online access.

---

# 🖥️ Frontend

The Smart Hospital frontend is maintained as a separate repository.

The frontend is built using:

- **HTML5**
- **CSS3**
- **JavaScript**

### Frontend Repository

https://github.com/kumarpriyam/SmartHospitalFrontend

### Live Frontend

https://smarthospitalfrontend.onrender.com

The frontend communicates with the ASP.NET Core backend through REST APIs.

---

# 🔗 Full Project Structure

```text
Smart Hospital Management System
│
├── SmartHospitalFrontend
│   ├── HTML5
│   ├── CSS3
│   └── JavaScript
│
├── SmartHospitalAPI
│   ├── C#
│   ├── ASP.NET Core Web API
│   ├── Controllers
│   ├── Models
│   └── Dockerfile
│
└── PostgreSQL
    ├── Patients
    ├── Doctors
    ├── Appointments
    ├── Consultation History
    └── Users
```

---

# 📚 What I Learned

This project gave me practical experience in developing and integrating a full-stack application while working with software development, backend APIs and database technologies.

Through this project, I gained hands-on experience with:

- **C++** and Object-Oriented Programming
- **Priority Queue** and data structures
- **C#**
- **ASP.NET Core Web API**
- **REST API development**
- **PostgreSQL**
- **SQL and database integration**
- **Frontend-backend communication**
- **Docker**
- **Cloud deployment using Render**
- **Git and GitHub**
- Designing a practical hospital management workflow

The project also helped me understand how a **C++ console-based application can be evolved into a web-based system** with a separate frontend, backend API, database and cloud deployment.

Most importantly, I learned how different technologies can work together to solve real-world problems such as **patient management, appointment scheduling, queue management and estimated waiting-time tracking**.

---

# 🔮 Future Improvements

The project can be further improved with additional features such as:

- Real-time queue updates
- SMS notifications for patients
- Email notifications
- Advanced authentication and authorization
- Role-based access control
- Automated CI/CD pipeline
- Advanced hospital analytics
- Predictive waiting-time estimation
- Application monitoring and logging
- Improved security and API validation

These improvements can make the system more **scalable, secure and suitable for larger hospital environments**.

---

# 👨‍💻 Author

## Priyam Kumar

**MCA | Data Analytics & Engineering | Software Development**

I enjoy turning data into meaningful insights and building practical software solutions that solve real-world problems.

### Contact

📧 **Email:**  
kumarpriyam1414@gmail.com

🔗 **LinkedIn:**  
[linkedin.com/in/priyamkumar01](https://linkedin.com/in/priyamkumar01)

🐙 **GitHub:**  
[github.com/kumarpriyam](https://github.com/kumarpriyam)

---

# 🔗 Project Links

### Backend Repository

[SmartHospitalAPI](https://github.com/kumarpriyam/SmartHospitalAPI)

### Frontend Repository

[SmartHospitalFrontend](https://github.com/kumarpriyam/SmartHospitalFrontend)

### Live Application

[Smart Hospital Frontend](https://smarthospitalfrontend.onrender.com)

### Backend API

[Smart Hospital API](https://smarthospitalapi.onrender.com)

---

## ⭐ Project

If you find this project useful or interesting, feel free to explore the complete Smart Hospital Management System.

Feedback, suggestions and collaboration are always welcome.

---

## 📄 License

This project is created for **educational, learning and portfolio purposes**.

