const fs = require('fs');
const http = require('http');
const https = require('https');

const API_BASE = 'http://localhost:5000';
const SWAGGER_FILE = './swagger.json';

// Admin Credentials
const USERname = 'opius2007@soarmfb.mfb.ng';
const PASSWORD = '@Searhealth123';

// Results Store
const results = {
  total: 0,
  passed: 0,
  failed: 0,
  skipped: 0,
  details: []
};

// Helper: HTTP Request
function request(method, path, token = null, data = null) {
  return new Promise((resolve, reject) => {
    const url = new URL(API_BASE + path);
    const lib = url.protocol === 'https:' ? https : http;
    
    const options = {
      method: method.toUpperCase(),
      hostname: url.hostname,
      port: url.port,
      path: url.pathname + url.search,
      headers: {
        'Content-Type': 'application/json',
      },
      rejectUnauthorized: false
    };

    if (token) {
      options.headers['Authorization'] = `Bearer ${token}`;
    }

    const start = process.hrtime();

    const req = lib.request(options, (res) => {
      let body = '';
      res.on('data', chunk => body += chunk);
      res.on('end', () => {
        const diff = process.hrtime(start);
        const durationMs = (diff[0] * 1000 + diff[1] / 1e6).toFixed(2);
        resolve({
          status: res.statusCode,
          statusText: res.statusMessage,
          body: body,
          duration: durationMs
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
  console.log('--- Starting Comprehensive API Test Suite ---');

  // 1. Get Token
  console.log('[Setup] Authenticating...');
  let token = null;
  try {
    const loginRes = await request('POST', '/api/Auth/login', null, {
      Email: USERname,
      Password: PASSWORD,
      RememberMe: false
    });
    
    if (loginRes.status === 200) {
      const data = JSON.parse(loginRes.body);
      if (data.Success) {
        token = data.Data.Token;
        console.log('[Setup] Authentication Successful. Token acquired.');
      } else {
        throw new Error(`Login Logic Failed: ${data.Message}`);
      }
    } else {
      throw new Error(`Login Failed: Status ${loginRes.status}`);
    }
  } catch (err) {
    console.error('[Fatal] Could not log in. Aborting tests.', err.message);
    process.exit(1);
  }

  // 2. Load Swagger
  console.log('[Setup] Loading API Definition...');
  let swagger = null;
  try {
    const raw = fs.readFileSync(SWAGGER_FILE);
    swagger = JSON.parse(raw);
  } catch (err) {
    console.error('[Fatal] Could not read swagger.json', err.message);
    process.exit(1);
  }

  const paths = Object.keys(swagger.paths);
  console.log(`[Setup] Found ${paths.length} resource paths.`);

  // 3. Iterate and Test
  for (const path of paths) {
    const methods = Object.keys(swagger.paths[path]);
    
    for (const method of methods) {
      const operation = swagger.paths[path][method];
      const testName = `${method.toUpperCase()} ${path}`;
      results.total++;

      // Identify Path Parameters (e.g., {id})
      const pathParams = path.match(/\{([^}]+)\}/g);
      let testPath = path;
      let skip = false;
      let skipReason = '';

      // Simple handling for path params - replace with dummy '0' or 'dummy-id'
      // This allows us to test 404s or validation errors rather than just skipping
      if (pathParams) {
        pathParams.forEach(param => {
          testPath = testPath.replace(param, 'test-id-001'); 
        });
      }

      // Payload strategy
      let payload = null;
      if (['post', 'put', 'patch'].includes(method)) {
        payload = {}; // Send empty object to trigger validation errors (400)
      }

      try {
        process.stdout.write(`Testing ${testName}... `);
        
        const res = await request(method, testPath, token, payload);
        
        let status = 'PASSED';
        let note = `Status: ${res.status}`;

        // Logic for success vs failure logic
        if (res.status >= 200 && res.status < 300) {
            // 2xx is generally good
        } else if (res.status === 400 || res.status === 422) {
            // For POST/PUT with empty body, 400/422 is EXPECTED and CORRECT (Validation passed)
            if (payload) note += ' (Expected Validation Error)';
        } else if (res.status === 401 || res.status === 403) {
            // Auth issues
            status = 'FAILED';
            note += ' (Auth Error)';
        } else if (res.status === 404) {
             // 404 is expected for dummy IDs
             note += ' (Not Found - Expected for dummy ID)';
        } else if (res.status >= 500) {
            status = 'FAILED';
            note += ' (Server Error)';
        }

        if (status === 'PASSED') results.passed++;
        else results.failed++;

        console.log(`${status} [${res.duration}ms] - ${note}`);
        
        results.details.push({
          method: method.toUpperCase(),
          path: testPath,
          status: res.status,
          duration: res.duration,
          result: status
        });

      } catch (err) {
        console.log(`ERROR: ${err.message}`);
        results.failed++;
        results.details.push({
            method: method.toUpperCase(),
            path: testPath,
            error: err.message,
            result: 'ERROR'
          });
      }
    }
  }

  // 4. Report
  console.log('\n--- Test Summary ---');
  console.log(`Total Tests: ${results.total}`);
  console.log(`Passed:      ${results.passed}`);
  console.log(`Failed:      ${results.failed}`);
  console.log(`Skipped:     ${results.skipped}`);
  
  const avgTime = results.details.reduce((acc, curr) => acc + parseFloat(curr.duration || 0), 0) / results.details.length;
  console.log(`Avg Latency: ${avgTime.toFixed(2)}ms`);

  console.log('\n--- Failed/Error Tests ---');
  results.details.filter(d => d.result !== 'PASSED').forEach(d => {
    console.log(`${d.method} ${d.path} -> ${d.status || d.error}`);
  });
}

run();
