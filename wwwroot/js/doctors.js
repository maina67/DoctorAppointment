const doctorString = localStorage.getItem("doctor");
const doctorData = doctorString ? JSON.parse(doctorString) : null;

if (!doctorData || !doctorData.doctorId || !doctorData.token) {
  console.warn("Doctor data missing. Redirecting to login...");
  window.location.href = "login.html";
}

const doctorID = doctorData.doctorId || doctorData.DoctorID;
const token = doctorData.token;

// --- Display Welcome Message ---
async function loadDoctorProfile() {
  try {
    const response = await fetch(`http://localhost:5124/api/doctors/details/${doctorID}`, {
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) throw new Error("Failed to load doctor details");

    const doctor = await response.json();
    const welcomeMsg = document.getElementById("welcomeDoctor");
    if (welcomeMsg) {
      welcomeMsg.textContent = `Welcome ${doctor.Name || "Doctor"}`;
    }

    console.log("✅ Doctor profile loaded successfully");
  } catch (error) {
    console.error("❌ Failed to load doctor profile:", error);
  }
}

// --- Load Doctor's Appointments ---
async function loadDoctorAppointments() {
  const list = document.getElementById("doctorAppointments");
  if (!list) {
    console.error("❌ doctorAppointments element not found in HTML");
    return;
  }

  list.innerHTML = "<li>Loading...</li>";

  try {
    const url = `http://localhost:5124/api/doctors/${doctorID}/appointments`;
    const response = await fetch(url, {
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) throw new Error(`Failed to fetch appointments (${response.status})`);

    const appointments = await response.json();
    list.innerHTML = "";

    if (!appointments || appointments.length === 0) {
      list.innerHTML = "<li>No appointments found.</li>";
      return;
    }

    appointments.forEach((app) => {
      const li = document.createElement("li");
      li.innerHTML = `
        <strong>Patient ID:</strong> ${app.patientID} <br>
        <strong>Date:</strong> ${new Date(app.date).toLocaleDateString()} <br>
        <strong>Time:</strong> ${app.time} <br>
        <strong>Status:</strong> <span class="status ${app.status.toLowerCase()}">${app.status}</span>
      `;

      if (app.status.toLowerCase() === "pending") {
        const completeBtn = document.createElement("button");
        completeBtn.textContent = "Mark as Completed";
        completeBtn.classList.add("complete-btn");
        completeBtn.addEventListener("click", () =>
          updateAppointmentStatus(app.appointmentID, "Completed")
        );
        li.appendChild(completeBtn);
      }

      list.appendChild(li);
    });

    console.log("✅ Appointments loaded successfully");
  } catch (error) {
    console.error("❌ Error loading appointments:", error);
    list.innerHTML = "<li>Failed to load appointments.</li>";
  }
}

// --- Update Appointment Status ---
async function updateAppointmentStatus(appointmentId, newStatus) {
  try {
    const response = await fetch(`http://localhost:5124/api/appointment/${appointmentId}/status`, {
      method: "PUT",
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ status: newStatus }),
    });

    if (!response.ok) throw new Error("Failed to update status");

    const result = await response.json();
    alert(result.message || "Status updated successfully!");
    loadDoctorAppointments();
  } catch (error) {
    console.error("❌ Error updating appointment:", error);
    alert("Failed to update appointment status");
  }
}

// --- Logout Function ---
function logout() {
  localStorage.removeItem("doctor");
  window.location.href = "login.html";
}

// --- Initialize on Page Load ---
document.addEventListener("DOMContentLoaded", () => {
  loadDoctorProfile();
  loadDoctorAppointments();
});
