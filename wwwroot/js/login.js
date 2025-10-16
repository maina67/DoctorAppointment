// Show login form
function showLogin() {
  document.getElementById('loginBox').classList.remove('hidden');
  document.getElementById('registerBox').classList.add('hidden');
  document.getElementById('adminLoginBox').classList.add('hidden');
  document.getElementById('doctorLoginBox').classList.add('hidden');
}

// Show register form
function showRegister() {
  document.getElementById('registerBox').classList.remove('hidden');
  document.getElementById('loginBox').classList.add('hidden');
  document.getElementById('adminLoginBox').classList.add('hidden');
  document.getElementById('doctorLoginBox').classList.add('hidden');
}

// Show admin login form
function showAdminLogin() {
  document.getElementById('adminLoginBox').classList.remove('hidden');
  document.getElementById('loginBox').classList.add('hidden');
  document.getElementById('registerBox').classList.add('hidden');
  document.getElementById('doctorLoginBox').classList.add('hidden');
}

// Show doctor login form
function showDoctorLogin() {
  document.getElementById('doctorLoginBox').classList.remove('hidden');
  document.getElementById('loginBox').classList.add('hidden');
  document.getElementById('registerBox').classList.add('hidden');
  document.getElementById('adminLoginBox').classList.add('hidden');
}

// Handle regular login form submission
document.getElementById('loginForm').addEventListener('submit', async function (event) {
  event.preventDefault();

  const email = document.getElementById('loginEmail').value;
  const password = document.getElementById('loginPassword').value;

  try {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    const text = await response.text();
    const data = text ? JSON.parse(text) : {};

    document.getElementById('loginMessage').innerText = data.message || "Login failed";

    if (response.ok) {
      console.log("✅ Login successful!");

      // ✅ Save user info
      localStorage.setItem('user', JSON.stringify(data));

      // ✅ Redirect based on user type
      if (data.userType === "doctor") {
        window.location.href = 'doctors.html';
      } else {
        window.location.href = 'appointments.html';
      }
    } else {
      console.log("❌ Login failed:", data.message);
    }
  } catch (error) {
    console.error('Login error:', error);
    document.getElementById('loginMessage').innerText = 'Login failed: ' + error.message;
  }
});

// Handle admin login form submission
document.getElementById('adminLoginForm').addEventListener('submit', async function (event) {
  event.preventDefault();

  const email = document.getElementById('adminEmail').value;
  const password = document.getElementById('adminPassword').value;

  try {
    const response = await fetch('/api/AdminAuth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    const text = await response.text();
    const data = text ? JSON.parse(text) : {};

    document.getElementById('adminLoginMessage').innerText = data.message || "Admin login failed";

    if (response.ok) {
      console.log("✅ Admin login successful!");

      localStorage.setItem('admin', JSON.stringify(data));

      window.location.href = 'admin.html';
    } else {
      console.log("❌ Admin login failed:", data.message);
    }
  } catch (error) {
    console.error('Admin login error:', error);
    document.getElementById('adminLoginMessage').innerText = 'Admin login failed: ' + error.message;
  }
});

// Handle register form submission
document.getElementById('registerForm').addEventListener('submit', async function (event) {
  event.preventDefault();

  const firstName = document.getElementById('firstName').value;
  const lastName = document.getElementById('lastName').value;
  const email = document.getElementById('registerEmail').value;
  const phoneNumber = document.getElementById('phoneNumber').value;
  const password = document.getElementById('registerPassword').value;

  try {
    const response = await fetch('/api/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ firstName, lastName, email, phoneNumber, password })
    });

    const text = await response.text();
    const data = text ? JSON.parse(text) : {};

    document.getElementById('registerMessage').innerText = data.message || "Registration failed";

    if (response.ok) {
      console.log("✅ Registration successful!");
      showLogin();
    } else {
      console.log("❌ Registration failed:", data.message);
    }
  } catch (error) {
    console.error('Registration error:', error);
    document.getElementById('registerMessage').innerText = 'Registration failed: ' + error.message;
  }
});

// ✅ Correct Doctor Login Form Submission
document.getElementById('doctorLoginForm').addEventListener('submit', async function (event) {
  event.preventDefault();

  const email = document.getElementById('doctorEmail').value;
  const password = document.getElementById('doctorPassword').value;

  try {
    const response = await fetch('/api/auth/doctor-login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    const text = await response.text();
    const data = text ? JSON.parse(text) : {};

    if (!response.ok) {
      throw new Error(data.message || "Doctor login failed");
    }

    console.log("✅ Doctor login successful:", data);

    // ✅ Store only what backend actually returns
    localStorage.setItem('doctor', JSON.stringify({
      doctorId: data.doctorId,
      token: data.token
    }));

    alert('Doctor login successful!');
    window.location.href = 'doctors.html';
  } catch (error) {
    console.error('Doctor login error:', error);
    alert('Login failed: ' + error.message);
  }
});



// ✅ Logout for all user types
function logout() {
  localStorage.removeItem('user');
  localStorage.removeItem('admin');
  localStorage.removeItem('doctor');
  window.location.href = 'login.html';
}
