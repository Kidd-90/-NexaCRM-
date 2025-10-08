#!/usr/bin/env node

/**
 * Admin Account Creation Script
 * Creates an admin account using the NexaCRM registration endpoint
 * 
 * Usage: node create-admin-account.js
 */

const https = require('https');
const http = require('http');

const ADMIN_EMAIL = 'admin@nexa.test';
const ADMIN_PASSWORD = 'admin123';
const ADMIN_USERNAME = 'admin';
const ADMIN_FULLNAME = 'System Administrator';

// Change this to your running server URL
const SERVER_URL = process.env.SERVER_URL || 'http://localhost:5000';

const registrationData = {
    email: ADMIN_EMAIL,
    password: ADMIN_PASSWORD,
    username: ADMIN_USERNAME,
    fullName: ADMIN_FULLNAME
};

function makeRequest(url, data) {
    return new Promise((resolve, reject) => {
        const urlObj = new URL(url);
        const isHttps = urlObj.protocol === 'https:';
        const lib = isHttps ? https : http;

        const postData = JSON.stringify(data);

        const options = {
            hostname: urlObj.hostname,
            port: urlObj.port || (isHttps ? 443 : 80),
            path: urlObj.pathname,
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Content-Length': Buffer.byteLength(postData)
            }
        };

        const req = lib.request(options, (res) => {
            let responseBody = '';

            res.on('data', (chunk) => {
                responseBody += chunk;
            });

            res.on('end', () => {
                if (res.statusCode >= 200 && res.statusCode < 300) {
                    resolve({ statusCode: res.statusCode, body: responseBody });
                } else {
                    reject(new Error(`HTTP ${res.statusCode}: ${responseBody}`));
                }
            });
        });

        req.on('error', (error) => {
            reject(error);
        });

        req.write(postData);
        req.end();
    });
}

async function createAdminAccount() {
    console.log('🚀 Creating admin account...');
    console.log(`📧 Email: ${ADMIN_EMAIL}`);
    console.log(`🔑 Password: ${ADMIN_PASSWORD}`);
    console.log(`👤 Username: ${ADMIN_USERNAME}`);
    console.log(`📝 Full Name: ${ADMIN_FULLNAME}`);
    console.log(`🌐 Server: ${SERVER_URL}`);
    console.log('');

    try {
        // Try to register the admin account
        const registrationUrl = `${SERVER_URL}/api/organization/register`;
        console.log(`📡 Sending registration request to: ${registrationUrl}`);

        const response = await makeRequest(registrationUrl, registrationData);

        console.log('');
        console.log('✅ Admin account created successfully!');
        console.log(`Status: ${response.statusCode}`);
        console.log(`Response: ${response.body}`);
        console.log('');
        console.log('🎉 You can now login with:');
        console.log(`   Email: ${ADMIN_EMAIL}`);
        console.log(`   Password: ${ADMIN_PASSWORD}`);
        console.log('');
        console.log('⚠️  Note: The account status may be "Pending" and require admin approval.');
        console.log('   You may need to manually activate it in the database or Supabase Dashboard.');

    } catch (error) {
        console.error('');
        console.error('❌ Failed to create admin account:');
        console.error(error.message);
        console.error('');
        console.error('💡 Troubleshooting:');
        console.error('   1. Make sure the NexaCRM server is running');
        console.error('   2. Check if the server URL is correct:', SERVER_URL);
        console.error('   3. Verify the registration endpoint is accessible');
        console.error('   4. Check if the account already exists');
        console.error('');
        console.error('   To specify a different server URL:');
        console.error('   SERVER_URL=http://localhost:5000 node create-admin-account.js');
        process.exit(1);
    }
}

// Run the script
createAdminAccount();
