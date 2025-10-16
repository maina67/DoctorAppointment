// --- USER AUTH HANDLING ---

// 1. Read user from localStorage
const userStorage = JSON.parse(localStorage.getItem('user'));

if (!userStorage || !userStorage.token) {
    window.location.href = 'login.html'; // Redirect to login if not found
}

// 2. Function to parse JWT
function parseJwt(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        return JSON.parse(jsonPayload);
    } catch (e) {
        console.error("Failed to parse JWT:", e);
        return null;
    }
}

// 3. Decode token to get user info
const userInfo = parseJwt(userStorage.token);

// 4. Get patient email
const patientEmail = userInfo ? userInfo.sub : null;

if (!patientEmail) {
    console.error("Patient email not found in the token");
    window.location.href = 'login.html';
}

// 5. Fetch real PatientID from backend
let patientID = null;

async function fetchPatientID(email) {
    try {
        const response = await fetch(`http://localhost:5124/api/patient/byemail/${encodeURIComponent(email)}`);
        if (!response.ok) {
            throw new Error('Failed to fetch patient ID');
        }
        const data = await response.json();
        patientID = data.patientID;
        console.log("Fetched PatientID:", patientID);
    } catch (error) {
        console.error('Error fetching patient ID:', error);
        alert('Failed to load patient information.');
        window.location.href = 'login.html';
    }
}


// --- DOCTOR DROPDOWN (DO NOT TOUCH) ---
let doctorMap = {}; // doctorID → doctorName

async function populateDoctors() {
    try {
        const response = await fetch('http://localhost:5124/api/doctors');
        const doctors = await response.json();

        const select = document.getElementById('doctorSelect');
        select.innerHTML = "";

        const placeholder = document.createElement('option');
        placeholder.disabled = true;
        placeholder.selected = true;
        placeholder.textContent = "-- Select Doctor --";
        select.appendChild(placeholder);

        doctors.forEach(doctor => {
            doctorMap[doctor.doctorID] = doctor.name;
            const option = document.createElement('option');
            option.value = doctor.doctorID;
            option.textContent = doctor.name;
            select.appendChild(option);
        });
    } catch (error) {
        console.error('Error fetching doctors:', error);
    }
}

// --- HANDLE BOOKING FORM ---
document.getElementById('bookForm').addEventListener('submit', async function (event) {
    event.preventDefault();

    const doctorId = document.getElementById('doctorSelect').value;
    const date = document.getElementById('date').value;
    const time = document.getElementById('time').value;

    // Check if all required fields are filled
    if (!doctorId || !date || !time || !patientID) {
        alert("Please fill in all fields.");
        return;
    }

    try {
        const combinedDateTime = new Date(`${date}T${time}`).toISOString();

        const appointmentData = {
            DoctorID: parseInt(doctorId),
            PatientID: parseInt(patientID),
            Date: combinedDateTime,
            Time: time,
            Status: "Pending"
        };

        console.log("Sending appointment:", appointmentData);

        const response = await fetch('http://localhost:5124/api/appointment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(appointmentData)
        });

        const data = await response.json();

        if (response.ok) {
            alert(data.message || "Appointment booked successfully!");
            await loadAppointments(); // refresh list after booking
        } else {
            alert("Failed to book appointment: " + (data.message || "Unknown error"));
        }
    } catch (error) {
        alert("Failed to book appointment: " + error.message);
    }
});


// --- LOAD APPOINTMENTS ---
async function loadAppointments() {
    try {
        const response = await fetch(`http://localhost:5124/api/patient/${patientID}/appointments`);
        if (!response.ok) throw new Error("Failed to fetch appointments");
        const appointments = await response.json();

        const tableBody = document.getElementById("appointmentTableBody");
        tableBody.innerHTML = "";

        if (appointments.length === 0) {
            tableBody.innerHTML = "<tr><td colspan='4'>No appointments found.</td></tr>";
            return;
        }

        appointments.forEach(appointment => {
            const row = document.createElement("tr");
            const doctorName = doctorMap[appointment.doctorID] || "Unknown";
            row.innerHTML = `
                <td>${appointment.date}</td>
                <td>${appointment.time}</td>
                <td>${doctorName}</td>
                <td>${appointment.status}</td>
            `;
            tableBody.appendChild(row);
        });
    } catch (error) {
        console.error("Error loading appointments:", error);
    }
}


// --- ON PAGE LOAD ---
document.addEventListener('DOMContentLoaded', async () => {
    await populateDoctors();
    await fetchPatientID(patientEmail);
    await loadAppointments();
});
