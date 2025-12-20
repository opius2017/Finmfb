const http = require('http');

const API_BASE = 'http://localhost:5000/api';
const USERNAME = 'opius2007@soarmfb.mfb.ng';
const PASSWORD = '@Searhealth123';

function request(method, path, data = null) {
  return new Promise((resolve, reject) => {
    const url = new URL(API_BASE + path);
    
    const options = {
      method: method.toUpperCase(),
      hostname: url.hostname,
      port: url.port,
      path: url.pathname + url.search,
      headers: {
        'Content-Type': 'application/json',
      }
    };

    const req = http.request(options, (res) => {
      let body = '';
      res.on('data', chunk => body += chunk);
      res.on('end', () => {
        resolve({
          status: res.statusCode,
          body: body
        });
      });
    });

    req.on('error', err => reject(err));

    if (data) {
      req.write(JSON.stringify(data));
    }
    req.end();
  });
}

async function run() {
  console.log(`[Frontend Test] Simulating login for ${USERNAME}...`);
  
  try {
    const res = await request('POST', '/auth/login', {
      email: USERNAME,
      password: PASSWORD
    });

    console.log(`[Frontend Test] Response Status: ${res.status}`);
    const data = JSON.parse(res.body);
    console.log('[Frontend Test] Full Response Body:', JSON.stringify(data, null, 2));
    
    if (res.status === 200 && data.Success) {
      console.log('[Frontend Test] Login SUCCESS!');
      console.log('[Frontend Test] User:', data.Data.Username);
      console.log('[Frontend Test] Roles:', data.Data.Roles);
      console.log('[Frontend Test] Token acquired (starts with):', data.Data.Token.substring(0, 20) + '...');
    } else {
      console.error('[Frontend Test] Login FAILED!');
      console.error('[Frontend Test] Message:', data.Message || 'No message');
      if (data.Errors) console.error('[Frontend Test] Errors:', data.Errors);
    }
  } catch (err) {
    console.error('[Frontend Test] Connection Error:', err.message);
  }
}

run();
