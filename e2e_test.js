const https = require('https');
const http = require('http');

const API_URL = 'http://localhost:5000';
const MAX_RETRIES = 30;
const RETRY_DELAY = 2000;

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function request(url, method = 'GET', data = null) {
  return new Promise((resolve, reject) => {
    const lib = url.startsWith('https') ? https : http;
    const options = {
      method,
      rejectUnauthorized: false, // Ignore self-signed certs
      headers: {}
    };

    if (data) {
      options.headers['Content-Type'] = 'application/json';
    }

    const req = lib.request(url, options, (res) => {
      let body = '';
      res.on('data', chunk => body += chunk);
      res.on('end', () => resolve({ status: res.statusCode, body }));
    });

    req.on('error', (err) => reject(err));

    if (data) {
      req.write(JSON.stringify(data));
    }
    req.end();
  });
}

async function checkHealth() {
  for (let i = 0; i < MAX_RETRIES; i++) {
    try {
      console.log(`Attempt ${i + 1}/${MAX_RETRIES}: Checking root endpoint...`);
      const res = await request(`${API_URL}/`);
      if (res.status === 200) {
        console.log('Root endpoint is up!');
        return true;
      } else {
        console.log(`Root endpoint returned ${res.status}`);
      }
    } catch (err) {
      console.log(`Connection failed: ${err.message}`);
    }
    await sleep(RETRY_DELAY);
  }
  return false;
}

async function runTest() {
  console.log('Starting E2E test...');

  if (!await checkHealth()) {
    console.error('Failed to connect to backend after multiple attempts.');
    process.exit(1);
  }

  try {
    console.log('Verifying Swagger UI access...');
    const swaggerRes = await request(`${API_URL}/swagger/index.html`);
    if (swaggerRes.status === 200) {
      console.log('Swagger UI is accessible (Status 200).');
    } else {
      console.error(`Swagger UI returned status ${swaggerRes.status}`);
      process.exit(1);
    }

    console.log('Verifying Swagger JSON definition...');
    const swaggerJsonRes = await request(`${API_URL}/swagger/v1/swagger.json`);
    if (swaggerJsonRes.status === 200) {
      console.log('Swagger JSON is accessible (Status 200).');
    } else {
      console.error(`Swagger JSON returned status ${swaggerJsonRes.status}`);
      process.exit(1);
    }

    console.log('Attempting Login with admin credentials...');
    const loginData = {
      Email: 'opius2007@soarmfb.mfb.ng',
      Password: '@Searhealth123',
      RememberMe: false
    };

    const loginRes = await request(`${API_URL}/api/Auth/login`, 'POST', loginData);
    if (loginRes.status === 200) {
      const body = JSON.parse(loginRes.body);
      if (body.Success) {
        console.log('Login successful!');
        console.log('User Roles:', body.Data.Roles);
        console.log('Access Token:', body.Data.Token);
        console.log('E2E Test Passed: API is up, Swagger is serving, and Login works.');
      } else {
        console.error('Login failed:', body.Message);
        process.exit(1);
      }
    } else {
      console.error(`Login request failed with status ${loginRes.status}`);
      console.error('Body:', loginRes.body);
      process.exit(1);
    }
  } catch (err) {
    console.error('Error testing Swagger:', err);
    process.exit(1);
  }
}

runTest();
