# CMCS_Prototype
# 🧾 CMCS Prototype – Claim Management and Coordination System

## 📘 Overview
The *CMCS Prototype (Claim Management and Coordination System)* is a C# WPF desktop application designed to simplify and streamline lecturer claim submissions, coordinator approvals, and academic manager reviews within academic institutions.

It supports multi-role workflows, document uploads, status tracking, CSV exports, and real-time progress monitoring — all tailored for South African higher education environments.

---

## 🚀 Features

### 👩‍🏫 Lecturer Functions
- Submit monthly claims with working hours and notes  
- Upload supporting documents (.pdf, .docx, .xlsx)  
- Real-time Rand (ZAR) currency calculation  
- Track submission status and approval progress  
- Automatic claim ID generation and progress tracking  

### 🧑‍💼 Coordinator Functions
- Review, approve, or reject lecturer claims  
- Provide rejection reasons through a custom dialog box  
- Move approved claims to the Academic Manager queue  
- Export claims to .csv for record-keeping  
- Refresh and monitor pending claim lists  

### 🧑‍🏫 Academic Manager Functions
- View coordinator-approved claims  
- Grant final approval for payment processing  
- Return claims for clarification with notes  
- Generate summary reports (approved, pending, and total amounts)  
- Refresh and manage approval queue  

### 📊 Tracking & Reporting
- Track every claim’s lifecycle from submission to payment  
- Visual progress bar showing claim approval stage  
- Custom input dialog for reasons/comments  
- South African Rand (ZAR) currency formatting  

---

## 🧠 Technology Stack

| Component | Description |
|------------|-------------|
| *Language* | C# (.NET WPF) |
| *Framework* | .NET 6 / .NET Framework (WPF) |
| *UI Framework* | Windows Presentation Foundation (WPF) |
| *Data Structures* | ObservableCollection for real-time data binding |
| *File Support* | .pdf, .docx, .xlsx document uploads |
| *Export Format* | .csv (Excel compatible) |
| *Localization* | South African (en-ZA) currency formatting |

---

## ⚙ Setup Instructions

### Prerequisites
- Visual Studio 2022 or newer  
- .NET 6 SDK (or .NET Framework 4.8)  
- Windows OS (WPF-supported)

