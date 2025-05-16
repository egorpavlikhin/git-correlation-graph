#!/usr/bin/env node

// This file uses CommonJS syntax
const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');

// Get the directory where this script is located
const scriptDir = __dirname;

// Check if node_modules exists, if not, install dependencies
if (!fs.existsSync(path.join(scriptDir, 'node_modules'))) {
  console.log('Installing dependencies...');
  const install = spawn('npm', ['install'], {
    cwd: scriptDir,
    stdio: 'inherit',
    shell: true
  });

  install.on('close', (code) => {
    if (code !== 0) {
      console.error('Failed to install dependencies');
      process.exit(1);
    }
    startApp();
  });
} else {
  startApp();
}

function startApp() {
  console.log('Starting Git Correlation Graph Visualizer...');
  console.log('The application will open in your default browser at http://localhost:3000');

  const vite = spawn('npx', ['vite'], {
    cwd: scriptDir,
    stdio: 'inherit',
    shell: true
  });

  vite.on('close', (code) => {
    if (code !== 0) {
      console.error(`Vite process exited with code ${code}`);
    }
  });
}
