// Fetch and display appointments
async function loadAppointments() {
  try {
    const response = await fetch('http://localhost:5124/api/appointment/with-details');
    const result = await response.json();

    console.log("API response:", result); // This should show the 'data' array
    console.log("Appointments Data:", result.data); // Check the structure of the 'data' array

    const appointments = result.data || []; // Assuming the appointments are inside the 'data' array

    const appointmentsTable = document.getElementById('appointmentsTable').getElementsByTagName('tbody')[0];
    appointmentsTable.innerHTML = ""; // Clear previous rows

    appointments.forEach(appointment => {
      console.log("Appointment:", appointment); // Check the individual appointment object

      const row = appointmentsTable.insertRow();
      const patientNameCell = row.insertCell(0);
      const doctorNameCell = row.insertCell(1);
      const appointmentDateCell = row.insertCell(2);
      const statusCell = row.insertCell(3);

      // Assuming patientName and doctorName are directly available in the appointment object
      const patientName = appointment.patientName || 'Unknown';
      const doctorName = appointment.doctorName || 'Unknown';  // Adjust this if doctorName exists

      patientNameCell.textContent = patientName;
      doctorNameCell.textContent = doctorName;
      appointmentDateCell.textContent = appointment.date || 'N/A';
      statusCell.textContent = appointment.status || 'N/A';
    });
  } catch (error) {
    console.error('Error loading appointments:', error);
  }
}

// Handle new doctor registration
document.getElementById('registerDoctorForm').addEventListener('submit', async function (event) {
  event.preventDefault();

  const firstName = document.getElementById('doctorFirstName').value;
  const lastName = document.getElementById('doctorLastName').value;
  const email = document.getElementById('doctorEmail').value;
  const password = document.getElementById('doctorPassword').value;
  const specialty = document.getElementById('doctorSpecialty').value;
  const contact = document.getElementById('doctorContact').value;

  console.log("Contact to be sent:", contact);


  try {
    const response = await fetch('http://localhost:5124/api/doctors/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: `${firstName} ${lastName}`,
        email: email,
        password: password,
        specialization: specialty,
        contact: contact  // Or whatever contact info you need
      })
    });

    // Log the raw response as text to see what the server is actually returning
    const rawResponse = await response.text();
    console.log("Raw Server Response:", rawResponse);  // Log raw response

    // Now try to parse the response as JSON
    const data = JSON.parse(rawResponse);
    
    document.getElementById('registerDoctorMessage').innerText = data.message || "Doctor registration failed";

    if (response.ok) {
      console.log("Doctor registered successfully!");
      document.getElementById('registerDoctorForm').reset();
    } else {
      console.log("Doctor registration failed:", data.message);
    }
  } catch (error) {
    console.error('Error registering doctor:', error);
    document.getElementById('registerDoctorMessage').innerText = 'Doctor registration failed: ' + error.message;
  }
});

async function loadDoctors() {
  try {
      console.log("Loading doctors...");  // Debugging log
      const response = await fetch('http://localhost:5124/api/doctors/list');
      const doctors = await response.json();
      
      console.log("Doctors Data:", doctors);  // Debugging log

      const doctorsTable = document.getElementById('doctorsTable').getElementsByTagName('tbody')[0];
      doctorsTable.innerHTML = ""; // Clear previous rows

      // Check if the response is an array
      if (Array.isArray(doctors)) {
        if (doctors.length === 0) {
          const row = doctorsTable.insertRow();
          const cell = row.insertCell(0);
          cell.colSpan = 5;  // Span across all columns
          cell.textContent = 'No doctors found';
        } else {
          doctors.forEach(doctor => {
              const row = doctorsTable.insertRow();

              const doctorNameCell = row.insertCell(0);
              const specializationCell = row.insertCell(1);
              const emailCell = row.insertCell(2);
              const contactCell = row.insertCell(3);
              const deleteCell = row.insertCell(4);

              doctorNameCell.textContent = doctor.name;
              specializationCell.textContent = doctor.specialization;
              emailCell.textContent = doctor.email;
              contactCell.textContent = doctor.contact;

              // Create delete button
              const deleteButton = document.createElement('button');
              deleteButton.textContent = 'Delete';
              deleteButton.onclick = () => deleteDoctor(doctor.doctorID);

              deleteCell.appendChild(deleteButton);
          });
        }
      } else {
        console.error("Expected an array of doctors, but got:", doctors);
      }

  } catch (error) {
      console.error('Error loading doctors:', error);
  }
}

// Load appointments and doctors when the page loads
window.addEventListener('load', () => {
  loadAppointments();
  loadDoctors();
});


async function deleteDoctor(doctorID) {
  try {
      const response = await fetch(`http://localhost:5124/api/doctors/${doctorID}`, {
          method: 'DELETE',
      });

      const result = await response.json();

      if (response.ok) {
          alert(result.message);  // Show success message
          loadDoctors();  // Refresh the list after deletion
      } else {
          alert(result.message);  // Show error message
      }
  } catch (error) {
      console.error('Error deleting doctor:', error);
      alert('Error deleting doctor');
  }
}

// Load appointments when the page loads
window.addEventListener('load', loadAppointments);

// Handle tab switching
document.querySelectorAll('.tab-button').forEach(button => {
  button.addEventListener('click', () => {
    const tabId = button.getAttribute('data-tab');

    // Remove active from all buttons
    document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));
    // Hide all tab contents
    document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));

    // Activate clicked tab and its content
    button.classList.add('active');
    document.getElementById(tabId).classList.add('active');
  });
});
