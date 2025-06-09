# alprog_boncos
This is an Electron application designed to read, store, and visualize sensor data. It features real-time data monitoring, database integration, serial communication capabilities, and a user-friendly dashboard.

🚀 Features
Real-time Sensor Data Monitoring: Continuously tracks and displays sensor readings.
Database Integration: Stores sensor data in a MySQL database for logging and analysis.
Serial Communication: Communicates with serial devices like Arduino to receive sensor data.
Express.js API: Provides a robust backend for seamless data handling.
Interactive Dashboard: Visualizes sensor data through an intuitive and responsive user interface.
User Authentication: Includes secure login and registration functionality for user management.
💻 Tech Stack
This project is built with a modern and powerful technology stack:

Frontend: HTML, CSS, JavaScript, with UI components from Tailwind CSS and Bootstrap.
Backend: Node.js with the Express.js framework.
Database: MySQL, accessed using the mysql2 driver.
Desktop Framework: Electron, for creating a cross-platform desktop application.
Serial Communication: The serialport library enables communication with hardware devices.
📂 File Structure
Here’s an overview of the key files and directories in the project:

main.js: The main entry point for the Electron application, managing the application's lifecycle and windows.
preload.js: A script that runs before the web page is loaded in the renderer process, used for secure Inter-Process Communication (IPC).
index.html & dashboard.html: The core HTML files that define the structure of the user interface.
controller/databaseController.js: Manages all database interactions, including inserting, retrieving, updating, and deleting sensor data.
lib/database.js: Contains the logic for connecting to the database and executing queries.
lib/serialCommunicator.js: Handles serial port communication, allowing the application to interface with hardware devices.
view/: A directory containing all UI-related files, including HTML, CSS, and client-side JavaScript.
🔧 Setup and Installation
To get the project up and running, follow these steps:

Clone the repository:
Bash

git clone https://github.com/coolguy916/alprog_boncos.git
Install dependencies:
Bash

npm install
Set up the database:
Make sure you have a MySQL server running.
Create a database and update the connection details in a .env file at the root of the project.
Run the application:
Bash

npm start
🗃️ Database
The application uses a MySQL database to store sensor data. The primary table is sensor_data, which includes columns for:

user_id
device_id
ph_reading
temperature_reading
moisture_percentage
The database.js file manages the database connection, while databaseController.js handles all CRUD (Create, Read, Update, Delete) operations.

📡 API Endpoints
The application exposes the following API endpoints for data management:

POST /sensor-data: Inserts new sensor data into the database.
GET /sensor-data: Retrieves sensor data based on specified filters.
DELETE /sensor-data: Deletes sensor data.
PUT /sensor-data: Updates existing sensor data.
🤝 Contributing
Contributions are welcome! If you have ideas for improvements or new features, please open an issue or submit a pull request.

📜 License
This project is licensed under the MIT License. See the package.json file for more details.
